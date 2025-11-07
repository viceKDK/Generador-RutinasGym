import { useState, useEffect } from 'react'
import { IconSearch, IconFilter, IconDumbbell } from '@tabler/icons-react'
import type { Exercise } from '../models/types'

export default function ExerciseLibrary() {
  const [exercises, setExercises] = useState<Exercise[]>([])
  const [loading, setLoading] = useState(true)
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedMuscle, setSelectedMuscle] = useState('')
  const [selectedEquipment, setSelectedEquipment] = useState('')

  useEffect(() => {
    loadExercises()
  }, [selectedMuscle, selectedEquipment])

  const loadExercises = async () => {
    setLoading(true)
    try {
      const data = await window.electronAPI.db.getExercises({
        muscleGroup: selectedMuscle || undefined,
        equipment: selectedEquipment || undefined,
      })
      setExercises(data)
    } catch (error) {
      console.error('Error loading exercises:', error)
    } finally {
      setLoading(false)
    }
  }

  const filteredExercises = exercises.filter((exercise) =>
    exercise.spanish_name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    exercise.name.toLowerCase().includes(searchTerm.toLowerCase())
  )

  return (
    <div className="animate-fade-in">
      <h1 className="text-3xl font-bold mb-8">Biblioteca de Ejercicios</h1>

      {/* Filters */}
      <div className="card mb-6">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label className="label">Buscar</label>
            <div className="relative">
              <IconSearch className="absolute left-3 top-1/2 transform -translate-y-1/2 text-text-muted" size={20} />
              <input
                type="text"
                className="input pl-10"
                placeholder="Buscar ejercicios..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>
          </div>

          <div>
            <label className="label">Grupo Muscular</label>
            <select
              className="input"
              value={selectedMuscle}
              onChange={(e) => setSelectedMuscle(e.target.value)}
            >
              <option value="">Todos</option>
              <option value="Pecho">Pecho</option>
              <option value="Espalda">Espalda</option>
              <option value="Hombros">Hombros</option>
              <option value="Brazos">Brazos</option>
              <option value="Piernas">Piernas</option>
              <option value="Core">Core</option>
            </select>
          </div>

          <div>
            <label className="label">Equipo</label>
            <select
              className="input"
              value={selectedEquipment}
              onChange={(e) => setSelectedEquipment(e.target.value)}
            >
              <option value="">Todos</option>
              <option value="Barra">Barra</option>
              <option value="Mancuernas">Mancuernas</option>
              <option value="Máquina">Máquina</option>
              <option value="Peso Corporal">Peso Corporal</option>
              <option value="Polea">Polea</option>
            </select>
          </div>
        </div>
      </div>

      {/* Exercise Grid */}
      {loading ? (
        <div className="flex justify-center items-center h-64">
          <div className="spinner" />
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {filteredExercises.map((exercise) => (
            <ExerciseCard key={exercise.id} exercise={exercise} />
          ))}
        </div>
      )}

      {!loading && filteredExercises.length === 0 && (
        <div className="card text-center py-12">
          <IconDumbbell size={64} className="text-text-muted mx-auto mb-4" />
          <p className="text-text-muted">No se encontraron ejercicios</p>
        </div>
      )}
    </div>
  )
}

interface ExerciseCardProps {
  exercise: Exercise
}

function ExerciseCard({ exercise }: ExerciseCardProps) {
  return (
    <div className="card hover:shadow-2xl transition-all duration-300">
      <div className="flex items-start justify-between mb-3">
        <h3 className="font-bold text-lg">{exercise.spanish_name}</h3>
        <span className={`px-2 py-1 rounded text-xs ${
          exercise.difficulty_level === 'Fácil'
            ? 'bg-green-500/20 text-green-400'
            : exercise.difficulty_level === 'Medio'
            ? 'bg-yellow-500/20 text-yellow-400'
            : 'bg-red-500/20 text-red-400'
        }`}>
          {exercise.difficulty_level}
        </span>
      </div>

      <div className="space-y-2 text-sm text-text-muted">
        <div className="flex items-center gap-2">
          <span className="font-medium">Músculo:</span>
          <span>{exercise.primary_muscle_group}</span>
        </div>
        <div className="flex items-center gap-2">
          <span className="font-medium">Equipo:</span>
          <span>{exercise.equipment_type}</span>
        </div>
      </div>

      {exercise.description && (
        <p className="mt-3 text-sm text-text-muted line-clamp-2">
          {exercise.description}
        </p>
      )}
    </div>
  )
}
