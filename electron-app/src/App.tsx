import { useState } from 'react'
import { BrowserRouter as Router, Routes, Route, Link, Navigate } from 'react-router-dom'
import { IconBarbell, IconFileText, IconSettings, IconEdit, IconMenu2, IconX } from '@tabler/icons-react'
import RoutineGenerator from './components/RoutineGenerator'
import ExerciseLibrary from './components/ExerciseLibrary'
import ExerciseManager from './components/ExerciseManager'
import CreateEditExercise from './components/CreateEditExercise'
import Settings from './components/Settings'

function App() {
  const [activeTab, setActiveTab] = useState('generator')
  const [sidebarOpen, setSidebarOpen] = useState(true)

  return (
    <Router>
      <div className="min-h-screen flex flex-col bg-background">
        {/* Header Premium */}
        <header className="drag-region bg-gradient-to-r from-surface via-surface-light to-surface border-b border-border-gold px-6 py-4 shadow-premium relative overflow-hidden">
          {/* Efecto de brillo en el header */}
          <div className="absolute inset-0 bg-gradient-to-r from-transparent via-primary/5 to-transparent animate-shimmer pointer-events-none"></div>
          
          <div className="flex items-center justify-between relative z-10">
            <div className="flex items-center gap-4">
              <div className="no-drag p-3 bg-gradient-to-br from-secondary via-secondary-dark to-secondary-light rounded-2xl shadow-glow-violet hover-lift hover-glow">
                <IconBarbell size={32} className="text-white filter drop-shadow-lg" />
              </div>
              <div>
                <h1 className="text-3xl font-bold gradient-text animate-fade-in tracking-tight">
                  Generador de Rutinas de Gimnasio
                </h1>
              </div>
            </div>
          </div>
        </header>

        <div className="flex flex-1 overflow-hidden">
          {/* Sidebar Premium - Desplegable */}
          <aside className={`bg-gradient-to-b from-surface to-surface-light border-r border-border-gold shadow-xl transition-all duration-300 ${sidebarOpen ? 'w-72 p-4' : 'w-0 p-0 overflow-hidden'}`}>
            <nav className="space-y-3">
              <NavLink
                to="/generator"
                icon={<IconBarbell size={22} />}
                label="Generar Rutina"
                active={activeTab === 'generator'}
                onClick={() => setActiveTab('generator')}
              />
              <NavLink
                to="/exercises"
                icon={<IconFileText size={22} />}
                label="Biblioteca de Ejercicios"
                active={activeTab === 'exercises'}
                onClick={() => setActiveTab('exercises')}
              />
              <NavLink
                to="/manager"
                icon={<IconEdit size={22} />}
                label="Gestor de Ejercicios"
                active={activeTab === 'manager'}
                onClick={() => setActiveTab('manager')}
              />
              <NavLink
                to="/settings"
                icon={<IconSettings size={22} />}
                label="Configuración"
                active={activeTab === 'settings'}
                onClick={() => setActiveTab('settings')}
              />
            </nav>
          </aside>

          {/* Toggle Button */}
          <button
            onClick={() => setSidebarOpen(!sidebarOpen)}
            className="absolute left-0 top-32 z-50 bg-gradient-to-r from-secondary to-secondary-dark text-white p-3 rounded-r-xl shadow-glow-violet hover:shadow-glow-violet transition-all duration-300 hover:scale-110"
            title={sidebarOpen ? 'Ocultar menú' : 'Mostrar menú'}
          >
            {sidebarOpen ? <IconX size={20} /> : <IconMenu2 size={20} />}
          </button>

          {/* Main content Premium */}
          <main className="flex-1 overflow-auto bg-background relative">
            {/* Efecto de gradiente de fondo */}
            <div className="absolute inset-0 bg-gradient-to-br from-primary/5 via-transparent to-secondary/5 pointer-events-none"></div>
            
            <div className="p-8 relative z-10">
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
      className={`
        group flex items-center gap-3 px-5 py-4 rounded-xl transition-all duration-300 relative overflow-hidden
        ${
          active
            ? 'bg-gradient-to-r from-secondary via-secondary-dark to-secondary text-white shadow-glow-violet border border-secondary-light'
            : 'text-text-secondary hover:text-white bg-transparent border border-transparent hover:border-border-gold hover:bg-surface'
        }
      `}
    >
      {/* Efecto de brillo en hover */}
      <div className={`absolute inset-0 bg-gradient-to-r from-transparent via-white/10 to-transparent transition-transform duration-700 ${active ? 'translate-x-full' : '-translate-x-full group-hover:translate-x-full'}`}></div>
      
      <div className={`relative z-10 transition-transform duration-300 ${active ? 'scale-110' : 'group-hover:scale-110'}`}>
        {icon}
      </div>
      <span className={`relative z-10 font-semibold tracking-wide ${active ? 'font-bold' : ''}`}>
        {label}
      </span>
      
      {/* Indicador activo */}
      {active && (
        <div className="absolute left-0 top-1/2 -translate-y-1/2 w-1 h-8 bg-white rounded-r-full shadow-lg"></div>
      )}
    </Link>
  )
}

export default App
