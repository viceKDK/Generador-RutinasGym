import { useState } from 'react'
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom'
import { IconHome, IconDumbbell, IconFileText, IconSettings } from '@tabler/icons-react'
import HomePage from './components/HomePage'
import RoutineGenerator from './components/RoutineGenerator'
import ExerciseLibrary from './components/ExerciseLibrary'
import Settings from './components/Settings'

function App() {
  const [activeTab, setActiveTab] = useState('home')

  return (
    <Router>
      <div className="min-h-screen flex flex-col bg-background">
        {/* Header */}
        <header className="bg-surface border-b border-border px-6 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <IconDumbbell size={32} className="text-primary" />
              <h1 className="text-2xl font-bold gradient-text">
                Generador de Rutinas de Gimnasio
              </h1>
            </div>
            <div className="flex items-center gap-2">
              <span className="text-sm text-text-muted">v1.0.0</span>
            </div>
          </div>
        </header>

        <div className="flex flex-1 overflow-hidden">
          {/* Sidebar */}
          <aside className="w-64 bg-surface border-r border-border p-4">
            <nav className="space-y-2">
              <NavLink
                to="/"
                icon={<IconHome size={20} />}
                label="Inicio"
                active={activeTab === 'home'}
                onClick={() => setActiveTab('home')}
              />
              <NavLink
                to="/generator"
                icon={<IconDumbbell size={20} />}
                label="Generar Rutina"
                active={activeTab === 'generator'}
                onClick={() => setActiveTab('generator')}
              />
              <NavLink
                to="/exercises"
                icon={<IconFileText size={20} />}
                label="Biblioteca de Ejercicios"
                active={activeTab === 'exercises'}
                onClick={() => setActiveTab('exercises')}
              />
              <NavLink
                to="/settings"
                icon={<IconSettings size={20} />}
                label="Configuración"
                active={activeTab === 'settings'}
                onClick={() => setActiveTab('settings')}
              />
            </nav>
          </aside>

          {/* Main content */}
          <main className="flex-1 overflow-auto">
            <div className="p-8">
              <Routes>
                <Route path="/" element={<HomePage />} />
                <Route path="/generator" element={<RoutineGenerator />} />
                <Route path="/exercises" element={<ExerciseLibrary />} />
                <Route path="/settings" element={<Settings />} />
              </Routes>
            </div>
          </main>
        </div>
      </div>
    </Router>
  )
}

interface NavLinkProps {
  to: string
  icon: React.ReactNode
  label: string
  active: boolean
  onClick: () => void
}

function NavLink({ to, icon, label, active, onClick }: NavLinkProps) {
  return (
    <Link
      to={to}
      onClick={onClick}
      className={`flex items-center gap-3 px-4 py-3 rounded-lg transition-all duration-200 ${
        active
          ? 'bg-primary text-white shadow-lg'
          : 'text-text-muted hover:bg-surface-light hover:text-white'
      }`}
    >
      {icon}
      <span className="font-medium">{label}</span>
    </Link>
  )
}

export default App
