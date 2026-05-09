export type AuthResponse = { accessToken: string; refreshToken: string }
export type TodoDto = { id: string; title: string; isDone: boolean }

const API_BASE = import.meta.env.VITE_API_URL ?? 'http://localhost:5078'

function getTokens() {
  return {
    accessToken: localStorage.getItem('accessToken') ?? '',
    refreshToken: localStorage.getItem('refreshToken') ?? '',
  }
}

function setTokens(tokens: AuthResponse) {
  localStorage.setItem('accessToken', tokens.accessToken)
  localStorage.setItem('refreshToken', tokens.refreshToken)
}

export function signOut() {
  localStorage.removeItem('accessToken')
  localStorage.removeItem('refreshToken')
}

async function request<T>(
  path: string,
  init?: RequestInit,
  retryOn401: boolean = true,
): Promise<T> {
  const { accessToken, refreshToken } = getTokens()
  const res = await fetch(`${API_BASE}${path}`, {
    ...init,
    headers: {
      'content-type': 'application/json',
      ...(accessToken ? { authorization: `Bearer ${accessToken}` } : {}),
      ...(init?.headers ?? {}),
    },
  })

  if (res.status === 401 && retryOn401 && refreshToken) {
    const refreshed = await fetch(`${API_BASE}/api/auth/refresh`, {
      method: 'POST',
      headers: { 'content-type': 'application/json' },
      body: JSON.stringify({ refreshToken }),
    })
    if (!refreshed.ok) {
      signOut()
      throw new Error('Session expired. Please sign in again.')
    }
    const tokens = (await refreshed.json()) as AuthResponse
    setTokens(tokens)
    return request<T>(path, init, false)
  }

  if (!res.ok) {
    const body = await res.json().catch(() => null)
    throw new Error(body?.message ?? `Request failed (${res.status})`)
  }

  if (res.status === 204) {
    return undefined as T
  }

  return (await res.json()) as T
}

export async function register(email: string, password: string) {
  const tokens = await request<AuthResponse>('/api/auth/register', {
    method: 'POST',
    body: JSON.stringify({ email, password }),
  })
  setTokens(tokens)
}

export async function login(email: string, password: string) {
  const tokens = await request<AuthResponse>('/api/auth/login', {
    method: 'POST',
    body: JSON.stringify({ email, password }),
  })
  setTokens(tokens)
}

export async function getTodos() {
  return request<TodoDto[]>('/api/todos')
}

export async function createTodo(title: string) {
  return request<TodoDto>('/api/todos', {
    method: 'POST',
    body: JSON.stringify({ title }),
  })
}

export async function updateTodo(todo: TodoDto) {
  return request<TodoDto>(`/api/todos/${todo.id}`, {
    method: 'PUT',
    body: JSON.stringify({ title: todo.title, isDone: todo.isDone }),
  })
}

export async function deleteTodo(id: string) {
  await request<void>(`/api/todos/${id}`, { method: 'DELETE' })
}

