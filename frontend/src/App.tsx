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

const savedEmail = localStorage.getItem('userEmail') ?? ''

function App() {
  const [mode, setMode] = useState<'login' | 'register'>('login')
  const [email, setEmail] = useState(savedEmail)
  const [password, setPassword] = useState('')
  const [title, setTitle] = useState('')
  const [tasks, setTasks] = useState<TodoDto[]>([])
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)
  const [signedIn, setSignedIn] = useState(() => !!localStorage.getItem('accessToken'))
  const [theme, setTheme] = useState<'light' | 'dark'>(
    () => (localStorage.getItem('uiTheme') === 'dark' ? 'dark' : 'light'),
  )
  const [filter, setFilter] = useState<'all' | 'active' | 'completed'>('all')
  const [search, setSearch] = useState('')
  const [userEmail, setUserEmail] = useState(savedEmail)

  const filteredTasks = useMemo(() => {
    const normalizedSearch = search.trim().toLowerCase()
    return tasks.filter((task) => {
      if (filter === 'active' && task.isDone) return false
      if (filter === 'completed' && !task.isDone) return false
      return !normalizedSearch || task.title.toLowerCase().includes(normalizedSearch)
    })
  }, [tasks, filter, search])

  const stats = useMemo(
    () => ({
      total: tasks.length,
      completed: tasks.filter((task) => task.isDone).length,
      pending: tasks.filter((task) => !task.isDone).length,
    }),
    [tasks],
  )

  const completionPercent = useMemo(
    () => (stats.total ? Math.round((stats.completed / stats.total) * 100) : 0),
    [stats.completed, stats.total],
  )

  useEffect(() => {
    document.documentElement.dataset.theme = theme
    localStorage.setItem('uiTheme', theme)
  }, [theme])

  async function load() {
    try {
      setError(null)
      setTasks(await getTodos())
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Something went wrong')
    }
  }

  useEffect(() => {
    if (signedIn) void load()
  }, [signedIn])

  function persistEmail(value: string) {
    localStorage.setItem('userEmail', value)
    setUserEmail(value)
  }

  async function onAuthSubmit(e: React.FormEvent) {
    e.preventDefault()
    setBusy(true)
    setError(null)

    try {
      if (mode === 'register') await register(email, password)
      else await login(email, password)

      persistEmail(email)
      setSignedIn(true)
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

  async function toggle(task: TodoDto) {
    setBusy(true)
    setError(null)

    try {
      await updateTodo({ ...task, isDone: !task.isDone })
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

  function handleSignOut() {
    signOut()
    localStorage.removeItem('userEmail')
    setSignedIn(false)
    setTasks([])
    setFilter('all')
    setSearch('')
    setUserEmail('')
  }

  return (
    <div className="app-shell">
      <header className="topbar">
        <div>
          <span className="brand">TaskFlow</span>
          <span className="brand-tag">.NET Task Studio</span>
        </div>
        <div className="topbar-actions">
          <span className="status-pill">
            {signedIn ? 'Secure session active' : 'Sign in to manage tasks'}
          </span>
          <button
            type="button"
            className="ghost-btn theme-toggle"
            onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')}
            aria-label={theme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'}
          >
            {theme === 'dark' ? 'Light mode' : 'Dark mode'}
          </button>
          {signedIn && (
            <button className="ghost-btn" type="button" onClick={handleSignOut} disabled={busy}>
              Sign out
            </button>
          )}
        </div>
      </header>

      <main className="page-grid">
        <section className="hero-panel">
          <div className="hero-copy">
            <span className="eyebrow">Productivity built for devs</span>
            <h1>Clean workflows for .NET-backed task management.</h1>
            <p>
              An elegant frontend experience for authentication-powered
              work item management. Login, register, search, and organize your
              backlog with style.
            </p>
            <div className="hero-badges">
              <span>JWT auth</span>
              <span>Responsive UI</span>
              <span>Task filters</span>
            </div>
          </div>

          <div className="hero-panel-card">
            <div className="hero-stats">
              <div>
                <strong>Fast onboarding</strong>
                <span>Impression-ready auth flows</span>
              </div>
              <div>
                <strong>Modern polish</strong>
                <span>Clean cards, soft shadows, crisp spacing</span>
              </div>
            </div>
            <div className="hero-terminal">
              <div className="hero-terminal-bar">
                <span className="terminal-dot red" />
                <span className="terminal-dot yellow" />
                <span className="terminal-dot green" />
              </div>
              <pre>frontend/src/App.tsx</pre>
              <p>Professional task UI built with React and Vite.</p>
            </div>
          </div>
        </section>

        <section className="workspace-panel">
          {error && <div className="alert">{error}</div>}

          {!signedIn ? (
            <div className="auth-card">
              <div className="auth-card-header">
                <div>
                  <p className="auth-title">{mode === 'login' ? 'Welcome back' : 'Create your account'}</p>
                  <p className="auth-subtitle">
                    Sign in securely to keep your work item backlog in sync with your API.
                  </p>
                </div>
                <div className="auth-toggle">
                  <button
                    type="button"
                    className={`toggle ${mode === 'login' ? 'active' : ''}`}
                    onClick={() => setMode('login')}
                    disabled={busy}
                  >
                    Login
                  </button>
                  <button
                    type="button"
                    className={`toggle ${mode === 'register' ? 'active' : ''}`}
                    onClick={() => setMode('register')}
                    disabled={busy}
                  >
                    Register
                  </button>
                </div>
              </div>

              <form onSubmit={onAuthSubmit} className="auth-form">
                <label className="input-group">
                  <span>Email</span>
                  <input
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    placeholder="example@company.com"
                    autoComplete="email"
                    type="email"
                  />
                </label>

                <label className="input-group">
                  <span>Password</span>
                  <input
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    placeholder="Strong password"
                    autoComplete={mode === 'login' ? 'current-password' : 'new-password'}
                    type="password"
                  />
                </label>

                <button type="submit" className="primary-btn" disabled={busy || !email || !password}>
                  {busy ? 'Processing…' : mode === 'login' ? 'Sign in' : 'Create account'}
                </button>
              </form>

              <div className="auth-footnote">
                Built for real developer workflows: solid auth, clean spacing, and polished feedback.
              </div>
            </div>
          ) : (
            <>
              <div className="dashboard-header">
                <div>
                  <span className="eyebrow">Daily focus</span>
                  <h2>Welcome back, {userEmail || 'developer'}.</h2>
                  <p>
                    Keep your backlog lean and finish work faster with task filters,
                    search, and structured status tracking.
                  </p>
                </div>
                <div className="dashboard-actions">
                  <button className="ghost-btn" type="button" onClick={handleSignOut} disabled={busy}>
                    Sign out
                  </button>
                  <button className="secondary-btn" type="button" onClick={load} disabled={busy}>
                    Refresh
                  </button>
                </div>
              </div>

              <div className="profile-panel">
                <div>
                  <p className="profile-label">Profile</p>
                  <h3>{userEmail || 'developer@example.com'}</h3>
                  <p>Logged in with secure JWT authentication.</p>
                </div>
                <div className="profile-metrics">
                  <div>
                    <span>{completionPercent}%</span>
                    <small>Completion</small>
                  </div>
                  <div>
                    <span>{stats.pending}</span>
                    <small>Open tasks</small>
                  </div>
                </div>
              </div>

              <div className="stats-grid">
                <article className="stat-card">
                  <p>Total work items</p>
                  <strong>{stats.total}</strong>
                </article>
                <article className="stat-card">
                  <p>Closed work items</p>
                  <strong>{stats.completed}</strong>
                </article>
                <article className="stat-card accent-card">
                  <p>Open work items</p>
                  <strong>{stats.pending}</strong>
                </article>
              </div>

              <section className="task-shell">
                <div className="task-toolbar">
                  <div className="search-group">
                    <label className="sr-only" htmlFor="search-input">
                      Search work items
                    </label>
                    <input
                      id="search-input"
                      value={search}
                      onChange={(e) => setSearch(e.target.value)}
                      placeholder="Search work items…"
                    />
                  </div>

                  <div className="filter-group">
                    {(['all', 'active', 'completed'] as const).map((value) => (
                      <button
                        key={value}
                        type="button"
                        className={`chip ${filter === value ? 'active' : ''}`}
                        onClick={() => setFilter(value)}
                        disabled={busy}
                      >
                        {value === 'all' ? 'All' : value === 'active' ? 'Active' : 'Completed'}
                      </button>
                    ))}
                  </div>
                </div>

                <form onSubmit={onCreate} className="task-create-form">
                  <input
                    value={title}
                    onChange={(e) => setTitle(e.target.value)}
                    placeholder="Add a new work item…"
                  />
                  <button type="submit" className="primary-btn" disabled={busy || !title.trim()}>
                    Add work item
                  </button>
                </form>

                <div className="task-card">
                  {filteredTasks.length === 0 ? (
                    <div className="empty-state">
                      <p>No work items found for the selected filter.</p>
                      <small>Use the search bar or add a new work item to get started.</small>
                    </div>
                  ) : (
                    <ul className="task-list">
                      {filteredTasks.map((task) => (
                        <li key={task.id} className={`task-item ${task.isDone ? 'done' : ''}`}>
                          <label className="task-label">
                            <input
                              type="checkbox"
                              checked={task.isDone}
                              onChange={() => void toggle(task)}
                              disabled={busy}
                            />
                            <span>{task.title}</span>
                          </label>
                          <button className="icon-btn" type="button" onClick={() => void remove(task.id)} disabled={busy}>
                            Delete
                          </button>
                        </li>
                      ))}
                    </ul>
                  )}
                </div>
              </section>
            </>
          )}
        </section>
      </main>
    </div>
  )
}

export default App
