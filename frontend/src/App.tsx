import { useEffect, useMemo, useState } from 'react'
import './App.css'
import {
  createTodo,
  deleteTodo,
  getTodos,
  login,
  register,
  signOut,
  type TodoDto,
  updateTodo,
} from './api'

function App() {
  const [mode, setMode] = useState<'login' | 'register'>('login')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [title, setTitle] = useState('')
  const [todos, setTodos] = useState<TodoDto[]>([])
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  const signedIn = useMemo(() => !!localStorage.getItem('accessToken'), [])

  async function load() {
    try {
      setError(null)
      setTodos(await getTodos())
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Something went wrong')
    }
  }

  useEffect(() => {
    if (signedIn) void load()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  async function onAuthSubmit(e: React.FormEvent) {
    e.preventDefault()
    setBusy(true)
    setError(null)
    try {
      if (mode === 'register') await register(email, password)
      else await login(email, password)
      await load()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Something went wrong')
    } finally {
      setBusy(false)
    }
  }

  async function onCreate(e: React.FormEvent) {
    e.preventDefault()
    if (!title.trim()) return
    setBusy(true)
    setError(null)
    try {
      await createTodo(title.trim())
      setTitle('')
      await load()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Something went wrong')
    } finally {
      setBusy(false)
    }
  }

  async function toggle(todo: TodoDto) {
    setBusy(true)
    setError(null)
    try {
      await updateTodo({ ...todo, isDone: !todo.isDone })
      await load()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Something went wrong')
    } finally {
      setBusy(false)
    }
  }

  async function remove(id: string) {
    setBusy(true)
    setError(null)
    try {
      await deleteTodo(id)
      await load()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Something went wrong')
    } finally {
      setBusy(false)
    }
  }

  return (
    <div style={{ maxWidth: 720, margin: '40px auto', padding: 16 }}>
      <h1 style={{ marginBottom: 8 }}>TaskFlow.API</h1>
      <p style={{ marginTop: 0, opacity: 0.8 }}>
        React client for an ASP.NET Core API.
      </p>
      <p>
- **Backend**: ASP.NET Core Web API (.NET 8), Clean Architecture-ish layering <br/>
- Auth: JWT access tokens + rotating refresh tokens (stored hashed) <br/>
- Mapping: AutoMapper (manual DI registration) <br/>
- Caching: cache abstraction + in-memory implementation <br/>
- Database: SQL Server + EF Core (migrations included) <br/>
- **Tests**: xUnit + Moq
      </p>

      {error && (
        <div
          style={{
            border: '1px solid #b91c1c',
            color: '#b91c1c',
            padding: 12,
            borderRadius: 8,
            marginBottom: 12,
          }}
        >
          {error}
        </div>
      )}

      {!localStorage.getItem('accessToken') ? (
        <form onSubmit={onAuthSubmit} style={{ display: 'grid', gap: 8 }}>
          <div style={{ display: 'flex', gap: 8 }}>
            <button
              type="button"
              onClick={() => setMode('login')}
              disabled={busy}
            >
              Login
            </button>
            <button
              type="button"
              onClick={() => setMode('register')}
              disabled={busy}
            >
              Register
            </button>
          </div>
          <input
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="email"
            autoComplete="email"
          />
          <input
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="password"
            autoComplete={mode === 'login' ? 'current-password' : 'new-password'}
            type="password"
          />
          <button type="submit" disabled={busy || !email || !password}>
            {busy ? 'Please wait…' : mode === 'login' ? 'Login' : 'Register'}
          </button>
        </form>
      ) : (
        <>
          <div style={{ display: 'flex', gap: 8, marginBottom: 12 }}>
            <button
              type="button"
              onClick={() => {
                signOut()
                setTodos([])
              }}
              disabled={busy}
            >
              Sign out
            </button>
            <button type="button" onClick={load} disabled={busy}>
              Refresh
            </button>
          </div>

          <form onSubmit={onCreate} style={{ display: 'flex', gap: 8 }}>
            <input
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="Add a todo…"
              style={{ flex: 1 }}
            />
            <button type="submit" disabled={busy || !title.trim()}>
              Add
            </button>
          </form>

          <ul style={{ marginTop: 16, paddingLeft: 16 }}>
            {todos.map((t) => (
              <li key={t.id} style={{ marginBottom: 8 }}>
                <label style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
                  <input
                    type="checkbox"
                    checked={t.isDone}
                    onChange={() => void toggle(t)}
                    disabled={busy}
                  />
                  <span style={{ textDecoration: t.isDone ? 'line-through' : undefined }}>
                    {t.title}
                  </span>
                  <button
                    type="button"
                    onClick={() => void remove(t.id)}
                    disabled={busy}
                    style={{ marginLeft: 'auto' }}
                  >
                    Delete
                  </button>
                </label>
              </li>
            ))}
          </ul>
        </>
      )}
    </div>
  )
}

export default App
