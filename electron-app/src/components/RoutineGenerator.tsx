import { useState } from 'react'
import { IconUserCircle, IconTarget, IconCalendar, IconSparkles } from '@tabler/icons-react'
import type { UserProfile, WorkoutPlan, FitnessLevel } from '../models/types'

export default function RoutineGenerator() {
  const [step, setStep] = useState(1)
  const [loading, setLoading] = useState(false)
  const [generatedPlan, setGeneratedPlan] = useState<WorkoutPlan | null>(null)

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
    setLoading(true)

    try {
      // Call Ollama API through Electron
      const result = await window.electronAPI.ollama.generateRoutine({
        userName: profile.name,
        userAge: profile.age,
        fitnessLevel: profile.fitnessLevel,
        trainingDays: profile.trainingDays,
        goals: profile.goals,
      })

      if (result.success) {
        // Process AI response and create workout plan
        // This is a simplified version - you'd parse the AI response properly
        const plan: WorkoutPlan = {
          userName: profile.name,
          userAge: profile.age,
          fitnessLevel: profile.fitnessLevel,
          trainingDays: profile.trainingDays,
          goals: profile.goals,
          routines: [], // Parse from AI response
        }
        setGeneratedPlan(plan)
        setStep(3)
      } else {
        alert('Error generando rutina: ' + result.error)
      }
    } catch (error) {
      console.error('Error:', error)
      alert('Error al generar rutina')
    } finally {
      setLoading(false)
    }
  }

  const handleExportToWord = async () => {
    if (!generatedPlan) return

    const result = await window.electronAPI.export.toWord(generatedPlan)
    if (result.success) {
      alert('Rutina exportada exitosamente a: ' + result.path)
    }
  }

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
          onExport={handleExportToWord}
          onNewRoutine={() => {
            setStep(1)
            setGeneratedPlan(null)
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
  return (
    <div className="card space-y-6">
      <div className="flex items-center gap-3 mb-6">
        <IconUserCircle size={32} className="text-primary" />
        <h2 className="text-2xl font-bold">Información Personal</h2>
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
            <option value="">Seleccionar</option>
            <option value="Masculino">Masculino</option>
            <option value="Femenino">Femenino</option>
            <option value="Otro">Otro</option>
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

      <button
        className="btn-primary w-full"
        onClick={onNext}
        disabled={!profile.name}
      >
        Siguiente
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
      <div className="flex items-center gap-3 mb-6">
        <IconTarget size={32} className="text-primary" />
        <h2 className="text-2xl font-bold">Objetivos de Entrenamiento</h2>
      </div>

      <div>
        <label className="label">Selecciona tus objetivos (uno o más)</label>
        <div className="grid grid-cols-2 gap-3">
          {goalOptions.map((goal) => (
            <button
              key={goal}
              onClick={() => toggleGoal(goal)}
              className={`p-4 rounded-lg border-2 transition-all duration-200 ${
                profile.goals.includes(goal)
                  ? 'border-primary bg-primary/10 text-white'
                  : 'border-border hover:border-primary/50 text-text-muted'
              }`}
            >
              {goal}
            </button>
          ))}
        </div>
      </div>

      <div className="flex gap-4">
        <button className="btn-outline flex-1" onClick={onBack}>
          Atrás
        </button>
        <button
          className="btn-primary flex-1 flex items-center justify-center gap-2"
          onClick={onGenerate}
          disabled={profile.goals.length === 0 || loading}
        >
          {loading ? (
            <>
              <div className="spinner" style={{ width: 20, height: 20, borderWidth: 2 }} />
              <span>Generando...</span>
            </>
          ) : (
            <>
              <IconSparkles size={20} />
              <span>Generar Rutina</span>
            </>
          )}
        </button>
      </div>
    </div>
  )
}

interface ResultStepProps {
  plan: WorkoutPlan
  onExport: () => void
  onNewRoutine: () => void
}

function ResultStep({ plan, onExport, onNewRoutine }: ResultStepProps) {
  return (
    <div className="space-y-6">
      <div className="card">
        <h2 className="text-2xl font-bold mb-4">Tu Rutina Personalizada</h2>
        <p className="text-text-muted mb-6">
          Rutina generada para {plan.userName} - {plan.trainingDays} días por semana
        </p>

        {/* This would show the actual routine details */}
        <div className="bg-surface-light p-6 rounded-lg">
          <p className="text-center text-text-muted">
            La rutina se mostrará aquí con todos los ejercicios, series y repeticiones
          </p>
        </div>
      </div>

      <div className="flex gap-4">
        <button className="btn-outline flex-1" onClick={onNewRoutine}>
          Nueva Rutina
        </button>
        <button className="btn-primary flex-1" onClick={onExport}>
          Exportar a Word
        </button>
      </div>
    </div>
  )
}
