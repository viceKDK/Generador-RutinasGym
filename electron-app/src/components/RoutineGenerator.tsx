import { useState, useEffect } from 'react'
import { IconUserCircle, IconTarget, IconCalendar, IconSparkles, IconPhoto } from '@tabler/icons-react'
import type { UserProfile, WorkoutPlan, FitnessLevel } from '../models/types'
import { useRoutineGenerator } from '../hooks/useRoutineGenerator'
import { useExport } from '../hooks/useExport'

interface ExerciseImage {
  id: number
  exercise_id: number
  image_path: string
  is_primary: boolean
}

export default function RoutineGenerator() {
  const [step, setStep] = useState(1)
  const [useAI, setUseAI] = useState(true)

  const { loading, error, generatedPlan, generateRoutine, checkOllamaStatus } = useRoutineGenerator()
  const { loading: exportLoading, exportToWord } = useExport()

  const [profile, setProfile] = useState<UserProfile>({
    name: '',
    age: undefined,
    gender: undefined,
    fitnessLevel: 'Principiante' as FitnessLevel,
    trainingDays: 3,
    goals: [],
    limitations: [],
  })

  const handleGenerate = async () => {
    const plan = await generateRoutine(profile, useAI)

    if (plan) {
      setStep(3)
    } else if (error) {
      alert('Error generando rutina: ' + error)
    }
  }

  const handleExportToWord = async () => {
    if (!generatedPlan) return

    const success = await exportToWord(generatedPlan)
    if (success) {
      alert('Rutina exportada exitosamente')
    }
  }

  // Eliminada función de exportar a HTML

  return (
    <div className="max-w-4xl mx-auto animate-fade-in">
      <h1 className="text-3xl font-bold mb-8">Generar Nueva Rutina</h1>

      {/* Progress Steps */}
      <div className="flex items-center justify-between mb-12">
        <Step number={1} title="Perfil" active={step >= 1} completed={step > 1} />
        <div className="flex-1 h-1 bg-border mx-4">
          <div className={`h-full bg-primary transition-all duration-300 ${step > 1 ? 'w-full' : 'w-0'}`} />
        </div>
        <Step number={2} title="Objetivos" active={step >= 2} completed={step > 2} />
        <div className="flex-1 h-1 bg-border mx-4">
          <div className={`h-full bg-primary transition-all duration-300 ${step > 2 ? 'w-full' : 'w-0'}`} />
        </div>
        <Step number={3} title="Rutina" active={step >= 3} completed={step > 3} />
      </div>

      {/* Step Content */}
      {step === 1 && (
        <ProfileStep
          profile={profile}
          onChange={setProfile}
          onNext={() => setStep(2)}
        />
      )}

      {step === 2 && (
        <GoalsStep
          profile={profile}
          onChange={setProfile}
          onBack={() => setStep(1)}
          onGenerate={handleGenerate}
          loading={loading}
        />
      )}

      {step === 3 && generatedPlan && (
        <ResultStep
          plan={generatedPlan}
          onExportWord={handleExportToWord}
          loading={exportLoading}
          onNewRoutine={() => {
            setStep(1)
          }}
        />
      )}
    </div>
  )
}

interface StepProps {
  number: number
  title: string
  active: boolean
  completed: boolean
}

function Step({ number, title, active, completed }: StepProps) {
  return (
    <div className="flex flex-col items-center">
      <div
        className={`w-12 h-12 rounded-full flex items-center justify-center font-bold text-lg transition-all duration-300 ${
          completed
            ? 'bg-primary text-white'
            : active
            ? 'bg-primary text-white'
            : 'bg-surface-light text-text-muted'
        }`}
      >
        {number}
      </div>
      <span className={`mt-2 text-sm ${active ? 'text-white' : 'text-text-muted'}`}>
        {title}
      </span>
    </div>
  )
}

interface ProfileStepProps {
  profile: UserProfile
  onChange: (profile: UserProfile) => void
  onNext: () => void
}

function ProfileStep({ profile, onChange, onNext }: ProfileStepProps) {
  const [error, setError] = useState('')

  const handleNext = () => {
    const missing = []
    if (!profile.name) missing.push('Nombre')
    if (!profile.age) missing.push('Edad')
    if (!profile.gender) missing.push('Género')

    if (missing.length > 0) {
      setError(`Falta completar: ${missing.join(', ')}`)
      return
    }

    setError('')
    onNext()
  }

  return (
    <div className="card space-y-6">
      <div className="flex items-center gap-4 mb-6">
        <div className="p-3 bg-gradient-to-br from-secondary to-secondary-dark rounded-xl shadow-glow-violet">
          <IconUserCircle size={36} className="text-white" />
        </div>
        <div>
          <h2 className="text-2xl font-bold">Información Personal</h2>
        </div>
      </div>

      <div>
        <label className="label">Nombre</label>
        <input
          type="text"
          className="input"
          placeholder="Tu nombre"
          value={profile.name}
          onChange={(e) => onChange({ ...profile, name: e.target.value })}
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="label">Edad</label>
          <input
            type="number"
            className="input"
            placeholder="25"
            value={profile.age || ''}
            onChange={(e) => onChange({ ...profile, age: parseInt(e.target.value) || undefined })}
          />
        </div>

        <div>
          <label className="label">Género</label>
          <select
            className="input"
            value={profile.gender || ''}
            onChange={(e) => onChange({ ...profile, gender: e.target.value as any })}
          >
            <option value="" disabled>Seleccionar género</option>
            <option value="Masculino">Masculino</option>
            <option value="Femenino">Femenino</option>
          </select>
        </div>
      </div>

      <div>
        <label className="label">Nivel de Fitness</label>
        <select
          className="input"
          value={profile.fitnessLevel}
          onChange={(e) => onChange({ ...profile, fitnessLevel: e.target.value as FitnessLevel })}
        >
          <option value="Principiante">Principiante</option>
          <option value="Intermedio">Intermedio</option>
          <option value="Avanzado">Avanzado</option>
        </select>
      </div>

      <div>
        <label className="label">Días de entrenamiento por semana: {profile.trainingDays}</label>
        <input
          type="range"
          min="1"
          max="7"
          className="w-full"
          value={profile.trainingDays}
          onChange={(e) => onChange({ ...profile, trainingDays: parseInt(e.target.value) })}
        />
        <div className="flex justify-between text-sm text-text-muted mt-2">
          <span>1 día</span>
          <span>7 días</span>
        </div>
      </div>

      {error && (
        <div className="bg-error/10 border border-error text-error px-4 py-3 rounded-xl">
          {error}
        </div>
      )}

      <button
        className="group relative w-full px-6 py-3 bg-gradient-to-r from-secondary via-secondary-dark to-secondary-light rounded-xl shadow-lg hover:shadow-glow-violet transition-all duration-300 hover:scale-105 overflow-hidden"
        onClick={handleNext}
      >
        <div className="absolute inset-0 bg-gradient-to-r from-transparent via-white/20 to-transparent -translate-x-full group-hover:translate-x-full transition-transform duration-700"></div>
        <span className="relative z-10 text-white font-bold">Siguiente</span>
      </button>
    </div>
  )
}

interface GoalsStepProps {
  profile: UserProfile
  onChange: (profile: UserProfile) => void
  onBack: () => void
  onGenerate: () => void
  loading: boolean
}

function GoalsStep({ profile, onChange, onBack, onGenerate, loading }: GoalsStepProps) {
  const goalOptions = [
    'Perder peso',
    'Ganar músculo',
    'Mejorar resistencia',
    'Aumentar fuerza',
    'Tonificar',
    'Mantener forma',
  ]

  const toggleGoal = (goal: string) => {
    const goals = profile.goals.includes(goal)
      ? profile.goals.filter((g) => g !== goal)
      : [...profile.goals, goal]
    onChange({ ...profile, goals })
  }

  return (
    <div className="card space-y-6">
      <div className="flex items-center gap-4 mb-6">
        <div className="p-3 bg-gradient-to-br from-secondary to-secondary-dark rounded-xl shadow-glow-violet">
          <IconTarget size={36} className="text-white" />
        </div>
        <div>
          <h2 className="text-2xl font-bold">Objetivos de Entrenamiento</h2>
          <p className="text-text-secondary text-sm">¿Qué quieres lograr?</p>
        </div>
      </div>

      <div>
        <label className="label">Selecciona tus objetivos (uno o más)</label>
        <div className="grid grid-cols-2 gap-3">
          {goalOptions.map((goal) => (
            <button
              key={goal}
              onClick={() => toggleGoal(goal)}
              className={`p-4 rounded-xl border-2 transition-all duration-300 font-semibold ${
                profile.goals.includes(goal)
                  ? 'border-primary bg-gradient-to-br from-primary to-primary-dark text-white shadow-glow-gold'
                  : 'border-border hover:border-primary/50 text-text-muted hover:bg-surface'
              }`}
            >
              {goal}
            </button>
          ))}
        </div>
      </div>

      <div className="flex gap-4">
        <button
          className="group relative flex-1 px-6 py-3 bg-gradient-to-r from-secondary via-secondary-dark to-secondary-light text-white font-semibold rounded-xl shadow-lg hover:shadow-glow-violet transition-all duration-300 hover:scale-105 overflow-hidden"
          onClick={onBack}
        >
          <div className="absolute inset-0 bg-gradient-to-r from-transparent via-white/20 to-transparent -translate-x-full group-hover:translate-x-full transition-transform duration-700"></div>
          <span className="relative z-10">Atrás</span>
        </button>
        <button
          className="group relative flex-1 px-6 py-3 bg-gradient-to-r from-secondary via-secondary-dark to-secondary-light text-white font-bold rounded-xl shadow-lg hover:shadow-glow-violet transition-all duration-300 hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100 overflow-hidden"
          onClick={onGenerate}
          disabled={profile.goals.length === 0 || loading}
        >
          <div className="absolute inset-0 bg-gradient-to-r from-transparent via-white/20 to-transparent -translate-x-full group-hover:translate-x-full transition-transform duration-700"></div>
          {loading ? (
            <span className="relative z-10 flex items-center justify-center gap-2">
              <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
              <span>Generando...</span>
            </span>
          ) : (
            <span className="relative z-10 flex items-center justify-center gap-2">
              <IconSparkles size={20} />
              <span>Generar Rutina</span>
            </span>
          )}
        </button>
      </div>
    </div>
  )
}

interface ResultStepProps {
  plan: WorkoutPlan
  onExportWord: () => void
  loading: boolean
  onNewRoutine: () => void
}

function ExerciseCardWithImage({ exercise, index }: { exercise: any; index: number }) {
  const [imagePath, setImagePath] = useState<string | null>(null)
  const [imageLoading, setImageLoading] = useState(true)

  useEffect(() => {
    const loadImage = async () => {
      if (!exercise.exercise?.id) return

      try {
        setImageLoading(true)
        const images = await window.electronAPI.db.getExerciseImages(exercise.exercise.id)
        const primaryImage = images.find((img: ExerciseImage) => img.is_primary) || images[0]

        if (primaryImage) {
          const absolutePath = await window.electronAPI.file.getPath(primaryImage.image_path)
          setImagePath(absolutePath)
        }
      } catch (error) {
        console.error('Error loading image:', error)
      } finally {
        setImageLoading(false)
      }
    }

    loadImage()
  }, [exercise.exercise?.id])

  return (
    <div className="bg-surface p-4 rounded-lg border border-border-gold hover:border-primary transition-all">
      <div className="flex gap-4">
        {/* Imagen del ejercicio */}
        <div className="w-24 h-24 flex-shrink-0 bg-surface-light rounded-lg overflow-hidden">
          {imageLoading ? (
            <div className="w-full h-full flex items-center justify-center bg-gradient-to-br from-primary/10 to-secondary/10">
              <div className="spinner" style={{ width: '20px', height: '20px', borderWidth: '2px' }} />
            </div>
          ) : imagePath ? (
            <img
              src={`file://${imagePath}`}
              alt={exercise.exercise?.spanish_name}
              className="w-full h-full object-cover"
            />
          ) : (
            <div className="w-full h-full flex items-center justify-center bg-gradient-to-br from-primary/10 to-secondary/10">
              <IconPhoto size={32} className="text-text-muted/30" />
            </div>
          )}
        </div>

        {/* Información del ejercicio */}
        <div className="flex-1 flex items-start justify-between">
          <div>
            <h4 className="font-bold text-lg">
              {index + 1}. {exercise.exercise?.spanish_name || 'Ejercicio'}
            </h4>
            <p className="text-sm text-text-muted mt-1">
              {exercise.exercise?.primary_muscle_group}
            </p>
          </div>
          <div className="text-right text-sm bg-surface-light px-3 py-2 rounded-lg">
            <div className="font-semibold text-primary">Series: {exercise.sets}</div>
            <div className="font-semibold text-primary">Reps: {exercise.reps}</div>
            <div className="text-text-muted">Descanso: {exercise.restSeconds}s</div>
          </div>
        </div>
      </div>
    </div>
  )
}

function ResultStep({ plan, onExportWord, loading, onNewRoutine }: ResultStepProps) {
  return (
    <div className="space-y-6">
      <div className="card">
        <h2 className="text-2xl font-bold mb-4">Tu Rutina Personalizada</h2>
        <p className="text-text-muted mb-6">
          Rutina generada para {plan.userName} - {plan.trainingDays} días por semana
        </p>

        {/* Mostrar rutinas por día */}
        <div className="space-y-6">
          {plan.routines.map((routine, idx) => (
            <div key={idx} className="bg-surface-light p-6 rounded-xl border border-border-gold">
              <h3 className="text-xl font-bold mb-2 gradient-text">{routine.dayName}</h3>
              <p className="text-text-muted mb-4">Enfoque: {routine.focus}</p>

              <div className="space-y-3">
                {routine.exercises.map((exercise, exIdx) => (
                  <ExerciseCardWithImage key={exIdx} exercise={exercise} index={exIdx} />
                ))}
              </div>
            </div>
          ))}
        </div>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <button
          className="group relative px-6 py-3 bg-gradient-to-r from-secondary via-secondary-dark to-secondary-light text-white font-semibold rounded-xl shadow-lg hover:shadow-glow-violet transition-all duration-300 hover:scale-105 overflow-hidden"
          onClick={onNewRoutine}
        >
          <div className="absolute inset-0 bg-gradient-to-r from-transparent via-white/20 to-transparent -translate-x-full group-hover:translate-x-full transition-transform duration-700"></div>
          <span className="relative z-10">Nueva Rutina</span>
        </button>
        <button
          className="group relative px-6 py-3 bg-gradient-to-r from-secondary via-secondary-dark to-secondary-light text-white font-bold rounded-xl shadow-lg hover:shadow-glow-violet transition-all duration-300 hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100 overflow-hidden"
          onClick={onExportWord}
          disabled={loading}
        >
          <div className="absolute inset-0 bg-gradient-to-r from-transparent via-white/20 to-transparent -translate-x-full group-hover:translate-x-full transition-transform duration-700"></div>
          <span className="relative z-10">{loading ? 'Exportando...' : 'Exportar a Word'}</span>
        </button>
      </div>
    </div>
  )
}
