import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { IconPlus, IconEdit, IconTrash, IconPhoto } from '@tabler/icons-react'
import type { Exercise } from '../models/types'
import { useExercises } from '../hooks/useExercises'

export default function ExerciseManager() {
  const navigate = useNavigate()
  const { exercises, loading, loadExercises } = useExercises({ autoLoad: true })
  const [searchTerm, setSearchTerm] = useState('')
  const [deleteConfirm, setDeleteConfirm] = useState<number | null>(null)

  const filteredExercises = exercises.filter((exercise) =>
    exercise.spanish_name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    exercise.name.toLowerCase().includes(searchTerm.toLowerCase())
  )

  const handleDelete = async (exerciseId: number) => {
    try {
      await window.electronAPI.db.deleteExercise(exerciseId)
      await loadExercises()
      setDeleteConfirm(null)
    } catch (error) {
      console.error('Error deleting exercise:', error)
      alert('Error al eliminar el ejercicio')
    }
  }

  return (
    <div className="animate-fade-in">
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="section-title">Gestor de Ejercicios</h1>
          <p className="text-text-secondary">Administra ejercicios, imágenes y videos</p>
        </div>
        <button
          onClick={() => navigate('/manager/create')}
          className="group relative px-6 py-3 bg-gradient-to-r from-secondary via-secondary-dark to-secondary-light text-white font-semibold rounded-xl shadow-lg hover:shadow-glow-violet transition-all duration-300 hover:scale-105 overflow-hidden"
        >
          <div className="absolute inset-0 bg-gradient-to-r from-transparent via-white/20 to-transparent -translate-x-full group-hover:translate-x-full transition-transform duration-700"></div>
          <span className="relative z-10">Nuevo Ejercicio</span>
        </button>
      </div>

      {/* Search */}
      <div className="card mb-6">
        <div className="relative">
          <input
            type="text"
            className="input"
            placeholder="Buscar ejercicios por nombre..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>
      </div>

      {/* Exercise List */}
      {loading ? (
        <div className="flex justify-center items-center h-64">
          <div className="spinner" />
        </div>
      ) : (
        <div className="space-y-3">
          {filteredExercises.map((exercise) => (
            <ExerciseManagerCard
              key={exercise.id}
              exercise={exercise}
              onEdit={() => navigate('/manager/edit', { state: { exercise } })}
              onDelete={() => setDeleteConfirm(exercise.id!)}
              isDeleting={deleteConfirm === exercise.id}
              onConfirmDelete={() => handleDelete(exercise.id!)}
              onCancelDelete={() => setDeleteConfirm(null)}
            />
          ))}
        </div>
      )}
    </div>
  )
}

interface ExerciseManagerCardProps {
  exercise: Exercise
  onEdit: () => void
  onDelete: () => void
  isDeleting: boolean
  onConfirmDelete: () => void
  onCancelDelete: () => void
}

function ExerciseManagerCard({ exercise, onEdit, onDelete, isDeleting, onConfirmDelete, onCancelDelete }: ExerciseManagerCardProps) {
  return (
    <div className="card group">
      {isDeleting ? (
        <div className="flex items-center justify-between p-4 bg-error/10 rounded-lg">
          <p className="text-error font-semibold">¿Eliminar "{exercise.spanish_name}"?</p>
          <div className="flex gap-2">
            <button
              onClick={onCancelDelete}
              className="px-4 py-2 bg-surface hover:bg-border rounded-lg transition-colors"
            >
              Cancelar
            </button>
            <button
              onClick={onConfirmDelete}
              className="px-4 py-2 bg-error text-white rounded-lg hover:bg-error/90 transition-colors"
            >
              Eliminar
            </button>
          </div>
        </div>
      ) : (
        <div className="flex items-center gap-4">
          <div className="w-20 h-20 bg-surface rounded-lg flex items-center justify-center flex-shrink-0">
            <IconPhoto size={32} className="text-text-muted/30" />
          </div>

          <div className="flex-1">
            <h3 className="font-bold text-lg text-text mb-1">{exercise.spanish_name}</h3>
            <div className="flex items-center gap-3 text-sm text-text-muted">
              <span>{exercise.primary_muscle_group}</span>
              <span>•</span>
              <span>{exercise.equipment_type}</span>
              <span>•</span>
              <span>{exercise.exercise_type || 'Fuerza'}</span>
            </div>
          </div>

          <div className="flex items-center gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
            <button
              onClick={onEdit}
              className="p-2 hover:bg-primary/10 text-primary rounded-lg transition-all"
              title="Editar"
            >
              <IconEdit size={20} />
            </button>
            <button
              onClick={onDelete}
              className="p-2 hover:bg-error/10 text-error rounded-lg transition-all"
              title="Eliminar"
            >
              <IconTrash size={20} />
            </button>
          </div>
        </div>
      )}
    </div>
  )
}

