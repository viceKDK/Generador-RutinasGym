import { useState, useEffect } from 'react'
import { BrowserRouter as Router, Routes, Route, Link, Navigate } from 'react-router-dom'
import { IconBarbell, IconFileText, IconSettings, IconEdit } from '@tabler/icons-react'
import RoutineGenerator from './components/RoutineGenerator'
import ExerciseLibrary from './components/ExerciseLibrary'
import ExerciseManager from './components/ExerciseManager'
import CreateEditExercise from './components/CreateEditExercise'
import Settings from './components/Settings'

function App() {
  const [activeTab, setActiveTab] = useState('generator')

  return (
    <Router>
      <div className="min-h-screen flex flex-col bg-background">
        {/* Header */}
        <header className="bg-surface-light border-b border-border px-6 py-4 shadow-sm">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-gradient-to-br from-primary to-secondary rounded-xl shadow-md">
                <IconBarbell size={28} className="text-white" />
              </div>
              <div>
                <h1 className="text-2xl font-bold gradient-text">
                  Generador de Rutinas de Gimnasio
                </h1>
              </div>
            </div>
          </div>
        </header>

        <div className="flex flex-1 overflow-hidden">
          {/* Sidebar */}
          <aside className="w-72 bg-surface-light border-r border-border p-4">
            <nav className="space-y-2">
              <NavLink
                to="/generator"
                icon={<IconBarbell size={20} />}
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
                to="/manager"
                icon={<IconEdit size={20} />}
                label="Gestor de Ejercicios"
                active={activeTab === 'manager'}
                onClick={() => setActiveTab('manager')}
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
          <main className="flex-1 overflow-auto bg-background">
            <div className="p-8">
              <Routes>
                <Route path="/" element={<Navigate to="/generator" replace />} />
                <Route path="/generator" element={<RoutineGenerator />} />
                <Route path="/exercises" element={<ExerciseLibrary />} />
                <Route path="/manager" element={<ExerciseManager />} />
                <Route path="/manager/create" element={<CreateEditExercise />} />
                <Route path="/manager/edit" element={<CreateEditExercise />} />
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
      className={`flex items-center gap-3 px-4 py-3 rounded-xl transition-all duration-300 ${
        active
          ? 'bg-gradient-to-r from-primary to-primary-dark text-white shadow-md'
          : 'text-text-muted hover:bg-surface hover:text-text hover:shadow-sm'
      }`}
    >
      {icon}
      <span className="font-semibold">{label}</span>
    </Link>
  )
}

export default App
