import { IconRocket, IconBook, IconSparkles } from '@tabler/icons-react'
import { useNavigate } from 'react-router-dom'

export default function HomePage() {
  const navigate = useNavigate()

  return (
    <div className="max-w-6xl mx-auto animate-fade-in">
      <div className="text-center mb-12">
        <h1 className="text-5xl font-bold gradient-text mb-4">
          Bienvenido a tu Generador de Rutinas
        </h1>
        <p className="text-xl text-text-muted">
          Crea rutinas personalizadas de entrenamiento con inteligencia artificial
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-12">
        <FeatureCard
          icon={<IconRocket size={48} />}
          title="Generación Rápida"
          description="Crea rutinas personalizadas en segundos basadas en tus objetivos"
          onClick={() => navigate('/generator')}
        />
        <FeatureCard
          icon={<IconBook size={48} />}
          title="Biblioteca Completa"
          description="Accede a cientos de ejercicios con instrucciones detalladas"
          onClick={() => navigate('/exercises')}
        />
        <FeatureCard
          icon={<IconSparkles size={48} />}
          title="IA Integrada"
          description="Usa inteligencia artificial local para optimizar tus entrenamientos"
        />
      </div>

      <div className="card">
        <h2 className="text-2xl font-bold mb-4">Cómo empezar</h2>
        <ol className="space-y-4 text-text-muted">
          <li className="flex items-start gap-3">
            <span className="flex-shrink-0 w-8 h-8 bg-primary rounded-full flex items-center justify-center text-white font-bold">
              1
            </span>
            <div>
              <strong className="text-white">Completa tu perfil:</strong> Ingresa tu edad, nivel de fitness y objetivos
            </div>
          </li>
          <li className="flex items-start gap-3">
            <span className="flex-shrink-0 w-8 h-8 bg-primary rounded-full flex items-center justify-center text-white font-bold">
              2
            </span>
            <div>
              <strong className="text-white">Genera tu rutina:</strong> Deja que la IA cree un plan personalizado para ti
            </div>
          </li>
          <li className="flex items-start gap-3">
            <span className="flex-shrink-0 w-8 h-8 bg-primary rounded-full flex items-center justify-center text-white font-bold">
              3
            </span>
            <div>
              <strong className="text-white">Exporta y entrena:</strong> Guarda tu rutina en Word o PDF y comienza a entrenar
            </div>
          </li>
        </ol>
      </div>

      <div className="mt-8 text-center">
        <button
          onClick={() => navigate('/generator')}
          className="btn-primary text-lg px-8 py-4"
        >
          Comenzar Ahora
        </button>
      </div>
    </div>
  )
}

interface FeatureCardProps {
  icon: React.ReactNode
  title: string
  description: string
  onClick?: () => void
}

function FeatureCard({ icon, title, description, onClick }: FeatureCardProps) {
  return (
    <div
      onClick={onClick}
      className={`card hover:shadow-2xl transition-all duration-300 ${
        onClick ? 'cursor-pointer hover:scale-105' : ''
      }`}
    >
      <div className="text-primary mb-4">{icon}</div>
      <h3 className="text-xl font-bold mb-2">{title}</h3>
      <p className="text-text-muted">{description}</p>
    </div>
  )
}
