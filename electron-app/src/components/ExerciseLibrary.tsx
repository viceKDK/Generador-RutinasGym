import { useState } from 'react'
import { IconSearch, IconBarbell, IconPhoto } from '@tabler/icons-react'
import type { Exercise } from '../models/types'
import { useExercises } from '../hooks/useExercises'

export default function ExerciseLibrary() {
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedMuscle, setSelectedMuscle] = useState('')
  const [selectedEquipment, setSelectedEquipment] = useState('')
  const [selectedExercise, setSelectedExercise] = useState<Exercise | null>(null)

  const { exercises, loading, loadExercises } = useExercises({
    muscleGroup: selectedMuscle,
    equipment: selectedEquipment,
    autoLoad: true,
  })

  const filteredExercises = exercises.filter((exercise) =>
    exercise.spanish_name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    exercise.name.toLowerCase().includes(searchTerm.toLowerCase())
  )

  return (
    <div className="animate-fade-in">
      <div className="flex items-center justify-between mb-8">
        <h1 className="section-title">Biblioteca de Ejercicios</h1>
        <div className="text-sm text-text-muted bg-surface px-4 py-2 rounded-lg">
          {filteredExercises.length} ejercicios encontrados
        </div>
      </div>

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
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredExercises.map((exercise) => (
            <ExerciseCard
              key={exercise.id}
              exercise={exercise}
              onClick={() => setSelectedExercise(exercise)}
            />
          ))}
        </div>
      )}

      {!loading && filteredExercises.length === 0 && (
        <div className="card text-center py-12">
          <IconBarbell size={64} className="text-text-muted mx-auto mb-4" />
          <p className="text-text-muted">No se encontraron ejercicios</p>
        </div>
      )}

      {/* Exercise Detail Modal */}
      {selectedExercise && (
        <ExerciseDetailModal
          exercise={selectedExercise}
          onClose={() => setSelectedExercise(null)}
        />
      )}
    </div>
  )
}

interface ExerciseCardProps {
  exercise: Exercise
  onClick?: () => void
}

function ExerciseCard({ exercise, onClick }: ExerciseCardProps) {
  return (
    <div
      className="card hover:shadow-xl transition-all duration-300 cursor-pointer group animate-scale-in"
      onClick={onClick}
    >
      {/* Image Preview */}
      <div className="relative w-full h-48 bg-surface rounded-lg mb-4 overflow-hidden">
        <div className="absolute inset-0 flex items-center justify-center bg-gradient-to-br from-primary/10 to-secondary/10">
          <IconPhoto size={48} className="text-text-muted/30" />
        </div>
        <div className="absolute top-2 right-2 bg-white/90 backdrop-blur-sm px-2 py-1 rounded-lg text-xs font-semibold text-primary">
          {exercise.exercise_type || 'Fuerza'}
        </div>
      </div>

      <div className="space-y-3">
        <h3 className="font-bold text-lg text-text group-hover:text-primary transition-colors">
          {exercise.spanish_name}
        </h3>

        <div className="flex items-center gap-4 text-sm">
          <div className="flex items-center gap-1">
            <div className="w-2 h-2 rounded-full bg-primary"></div>
            <span className="text-text-muted">{exercise.primary_muscle_group}</span>
          </div>
          {exercise.secondary_muscle_group && (
            <div className="flex items-center gap-1">
              <div className="w-2 h-2 rounded-full bg-secondary"></div>
              <span className="text-text-muted">{exercise.secondary_muscle_group}</span>
            </div>
          )}
        </div>

        <div className="flex items-center gap-2 text-sm">
          <span className="px-3 py-1 bg-surface rounded-full text-text-muted">
            {exercise.equipment_type}
          </span>
        </div>

        {exercise.description && (
          <p className="text-sm text-text-muted line-clamp-2">
            {exercise.description}
          </p>
        )}
      </div>
    </div>
  )
}

interface ExerciseDetailModalProps {
  exercise: Exercise
  onClose: () => void
}

function ExerciseDetailModal({ exercise, onClose }: ExerciseDetailModalProps) {
  return (
    <div
      className="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center z-50 p-4"
      onClick={onClose}
    >
      <div
        className="bg-surface-light rounded-2xl max-w-2xl w-full max-h-[90vh] overflow-y-auto shadow-xl animate-scale-in"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="p-6">
          <div className="flex items-start justify-between mb-6">
            <div>
              <h2 className="text-2xl font-bold text-text mb-2">{exercise.spanish_name}</h2>
              <p className="text-text-muted">{exercise.name}</p>
            </div>
            <button
              onClick={onClose}
              className="text-text-muted hover:text-text transition-colors"
            >
              ✕
            </button>
          </div>

          <div className="space-y-4">
            <div className="w-full h-64 bg-surface rounded-lg flex items-center justify-center">
              <IconPhoto size={64} className="text-text-muted/30" />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-sm font-semibold text-text-muted mb-1">Músculo Principal</p>
                <p className="text-text">{exercise.primary_muscle_group}</p>
              </div>
              {exercise.secondary_muscle_group && (
                <div>
                  <p className="text-sm font-semibold text-text-muted mb-1">Músculo Secundario</p>
                  <p className="text-text">{exercise.secondary_muscle_group}</p>
                </div>
              )}
              <div>
                <p className="text-sm font-semibold text-text-muted mb-1">Equipo</p>
                <p className="text-text">{exercise.equipment_type}</p>
              </div>
              <div>
                <p className="text-sm font-semibold text-text-muted mb-1">Tipo</p>
                <p className="text-text">{exercise.exercise_type || 'Fuerza'}</p>
              </div>
            </div>

            {exercise.description && (
              <div>
                <p className="text-sm font-semibold text-text-muted mb-2">Descripción</p>
                <p className="text-text">{exercise.description}</p>
              </div>
            )}

            {exercise.instructions && (
              <div>
                <p className="text-sm font-semibold text-text-muted mb-2">Instrucciones</p>
                <p className="text-text whitespace-pre-line">{exercise.instructions}</p>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}
