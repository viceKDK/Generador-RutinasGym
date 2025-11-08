import { useState, useEffect } from 'react'
import { IconPlus, IconEdit, IconTrash, IconPhoto, IconVideo, IconSearch, IconUpload, IconLink, IconX } from '@tabler/icons-react'
import type { Exercise } from '../models/types'
import { useExercises } from '../hooks/useExercises'

export default function ExerciseManager() {
  const { exercises, loading, loadExercises } = useExercises({ autoLoad: true })
  const [selectedExercise, setSelectedExercise] = useState<Exercise | null>(null)
  const [isCreating, setIsCreating] = useState(false)
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
          <p className="text-text-muted">Administra ejercicios, imágenes y videos</p>
        </div>
        <button
          onClick={() => setIsCreating(true)}
          className="btn-primary flex items-center gap-2"
        >
          <IconPlus size={20} />
          Nuevo Ejercicio
        </button>
      </div>

      {/* Search */}
      <div className="card mb-6">
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
              onEdit={() => setSelectedExercise(exercise)}
              onDelete={() => setDeleteConfirm(exercise.id!)}
              isDeleting={deleteConfirm === exercise.id}
              onConfirmDelete={() => handleDelete(exercise.id!)}
              onCancelDelete={() => setDeleteConfirm(null)}
            />
          ))}
        </div>
      )}

      {/* Create/Edit Modal */}
      {(isCreating || selectedExercise) && (
        <ExerciseEditorModal
          exercise={selectedExercise}
          onClose={() => {
            setIsCreating(false)
            setSelectedExercise(null)
          }}
          onSave={async (data) => {
            try {
              const exerciseData = {
                name: data.name,
                spanish_name: data.spanish_name,
                description: data.description,
                instructions: data.instructions,
                primary_muscle_group: data.primary_muscle_group,
                secondary_muscle_group: data.secondary_muscle_group,
                equipment_type: data.equipment_type,
                exercise_type: data.exercise_type,
                video_url: data.video_url,
              }

              let exerciseId: number

              if (selectedExercise?.id) {
                // Update existing
                await window.electronAPI.db.updateExercise(selectedExercise.id, exerciseData)
                exerciseId = selectedExercise.id
              } else {
                // Create new
                exerciseId = await window.electronAPI.db.createExercise(exerciseData)
              }

              // Upload images
              if (data.images && data.images.length > 0) {
                for (let i = 0; i < data.images.length; i++) {
                  const file = data.images[i]
                  const reader = new FileReader()

                  await new Promise((resolve, reject) => {
                    reader.onload = async () => {
                      try {
                        const base64 = reader.result as string
                        const imagePath = await window.electronAPI.file.uploadImage(base64, exerciseId)

                        if (imagePath) {
                          await window.electronAPI.db.saveExerciseImage(
                            exerciseId,
                            imagePath,
                            i === 0 // First image is primary
                          )
                        }
                        resolve(null)
                      } catch (err) {
                        reject(err)
                      }
                    }
                    reader.onerror = reject
                    reader.readAsDataURL(file)
                  })
                }
              }

              // Upload videos
              if (data.videos && data.videos.length > 0) {
                for (const file of data.videos) {
                  const reader = new FileReader()

                  await new Promise((resolve, reject) => {
                    reader.onload = async () => {
                      try {
                        const base64 = reader.result as string
                        await window.electronAPI.file.uploadVideo(base64, exerciseId)
                        resolve(null)
                      } catch (err) {
                        reject(err)
                      }
                    }
                    reader.onerror = reject
                    reader.readAsDataURL(file)
                  })
                }
              }

              setIsCreating(false)
              setSelectedExercise(null)
              await loadExercises()
            } catch (error) {
              console.error('Error saving exercise:', error)
              alert('Error al guardar el ejercicio')
            }
          }}
        />
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

interface ExerciseEditorModalProps {
  exercise: Exercise | null
  onClose: () => void
  onSave: (data: any) => Promise<void>
}

function ExerciseEditorModal({ exercise, onClose, onSave }: ExerciseEditorModalProps) {
  const [formData, setFormData] = useState({
    name: exercise?.name || '',
    spanish_name: exercise?.spanish_name || '',
    description: exercise?.description || '',
    instructions: exercise?.instructions || '',
    primary_muscle_group: exercise?.primary_muscle_group || '',
    secondary_muscle_group: exercise?.secondary_muscle_group || '',
    equipment_type: exercise?.equipment_type || '',
    exercise_type: exercise?.exercise_type || 'Fuerza',
    video_url: exercise?.video_url || '',
  })

  const [images, setImages] = useState<File[]>([])
  const [imagePreviews, setImagePreviews] = useState<string[]>([])
  const [videos, setVideos] = useState<File[]>([])
  const [saving, setSaving] = useState(false)

  const handleImageChange = (files: FileList | null) => {
    if (!files) return

    const fileArray = Array.from(files)
    setImages(fileArray)

    // Generate previews
    const previews = fileArray.map(file => URL.createObjectURL(file))
    setImagePreviews(previews)
  }

  // Cleanup previews on unmount
  useEffect(() => {
    return () => {
      imagePreviews.forEach(url => URL.revokeObjectURL(url))
    }
  }, [imagePreviews])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setSaving(true)
    try {
      await onSave({ ...formData, images, videos })
    } finally {
      setSaving(false)
    }
  }

  return (
    <div
      className="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center z-50 p-4"
      onClick={onClose}
    >
      <div
        className="bg-surface-light rounded-2xl max-w-4xl w-full max-h-[90vh] overflow-y-auto shadow-xl animate-scale-in"
        onClick={(e) => e.stopPropagation()}
      >
        <form onSubmit={handleSubmit} className="p-6">
          <div className="flex items-start justify-between mb-6">
            <div>
              <h2 className="text-2xl font-bold text-text mb-2">
                {exercise ? 'Editar Ejercicio' : 'Nuevo Ejercicio'}
              </h2>
              <p className="text-text-muted">Completa la información del ejercicio</p>
            </div>
            <button
              type="button"
              onClick={onClose}
              className="text-text-muted hover:text-text transition-colors"
            >
              ✕
            </button>
          </div>

          <div className="space-y-6">
            {/* Basic Info */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="label">Nombre (Inglés)</label>
                <input
                  type="text"
                  className="input"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  required
                />
              </div>
              <div>
                <label className="label">Nombre (Español)</label>
                <input
                  type="text"
                  className="input"
                  value={formData.spanish_name}
                  onChange={(e) => setFormData({ ...formData, spanish_name: e.target.value })}
                  required
                />
              </div>
            </div>

            <div>
              <label className="label">Descripción</label>
              <textarea
                className="input"
                rows={3}
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              />
            </div>

            <div>
              <label className="label">Instrucciones</label>
              <textarea
                className="input"
                rows={5}
                value={formData.instructions}
                onChange={(e) => setFormData({ ...formData, instructions: e.target.value })}
              />
            </div>

            {/* Categories */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="label">Grupo Muscular Principal</label>
                <select
                  className="input"
                  value={formData.primary_muscle_group}
                  onChange={(e) => setFormData({ ...formData, primary_muscle_group: e.target.value })}
                  required
                >
                  <option value="">Seleccionar...</option>
                  <option value="Pecho">Pecho</option>
                  <option value="Espalda">Espalda</option>
                  <option value="Hombros">Hombros</option>
                  <option value="Brazos">Brazos</option>
                  <option value="Piernas">Piernas</option>
                  <option value="Core">Core</option>
                </select>
              </div>

              <div>
                <label className="label">Grupo Muscular Secundario</label>
                <select
                  className="input"
                  value={formData.secondary_muscle_group}
                  onChange={(e) => setFormData({ ...formData, secondary_muscle_group: e.target.value })}
                >
                  <option value="">Ninguno</option>
                  <option value="Pecho">Pecho</option>
                  <option value="Espalda">Espalda</option>
                  <option value="Hombros">Hombros</option>
                  <option value="Brazos">Brazos</option>
                  <option value="Piernas">Piernas</option>
                  <option value="Core">Core</option>
                </select>
              </div>

              <div>
                <label className="label">Tipo de Equipo</label>
                <select
                  className="input"
                  value={formData.equipment_type}
                  onChange={(e) => setFormData({ ...formData, equipment_type: e.target.value })}
                  required
                >
                  <option value="">Seleccionar...</option>
                  <option value="Barra">Barra</option>
                  <option value="Mancuernas">Mancuernas</option>
                  <option value="Máquina">Máquina</option>
                  <option value="Peso Corporal">Peso Corporal</option>
                  <option value="Polea">Polea</option>
                  <option value="Kettlebell">Kettlebell</option>
                  <option value="Bandas">Bandas</option>
                </select>
              </div>

              <div>
                <label className="label">Tipo de Ejercicio</label>
                <select
                  className="input"
                  value={formData.exercise_type}
                  onChange={(e) => setFormData({ ...formData, exercise_type: e.target.value })}
                  required
                >
                  <option value="Fuerza">Fuerza</option>
                  <option value="Cardio">Cardio</option>
                  <option value="Flexibilidad">Flexibilidad</option>
                  <option value="Pliométrico">Pliométrico</option>
                  <option value="Funcional">Funcional</option>
                </select>
              </div>
            </div>

            {/* Video URL */}
            <div>
              <label className="label flex items-center gap-2">
                <IconLink size={18} />
                URL del Video (YouTube, Vimeo, etc.)
              </label>
              <input
                type="url"
                className="input"
                placeholder="https://www.youtube.com/watch?v=..."
                value={formData.video_url}
                onChange={(e) => setFormData({ ...formData, video_url: e.target.value })}
              />
            </div>

            {/* Media Upload */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="label flex items-center gap-2">
                  <IconPhoto size={18} />
                  Imágenes
                </label>
                <div className="border-2 border-dashed border-border rounded-lg p-6 text-center hover:border-primary transition-colors cursor-pointer">
                  <input
                    type="file"
                    accept="image/*"
                    multiple
                    className="hidden"
                    id="image-upload"
                    onChange={(e) => handleImageChange(e.target.files)}
                  />
                  <label htmlFor="image-upload" className="cursor-pointer">
                    <IconUpload size={32} className="mx-auto mb-2 text-text-muted" />
                    <p className="text-sm text-text-muted">
                      {images.length > 0 ? `${images.length} imagen(es) seleccionada(s)` : 'Click para subir imágenes'}
                    </p>
                  </label>
                </div>

                {/* Image Previews */}
                {imagePreviews.length > 0 && (
                  <div className="grid grid-cols-3 gap-2 mt-4">
                    {imagePreviews.map((preview, index) => (
                      <div key={index} className="relative group">
                        <img
                          src={preview}
                          alt={`Preview ${index + 1}`}
                          className="w-full h-24 object-cover rounded-lg border border-border"
                        />
                        <button
                          type="button"
                          onClick={() => {
                            const newImages = images.filter((_, i) => i !== index)
                            const newPreviews = imagePreviews.filter((_, i) => i !== index)
                            setImages(newImages)
                            setImagePreviews(newPreviews)
                          }}
                          className="absolute -top-2 -right-2 bg-error text-white rounded-full p-1 opacity-0 group-hover:opacity-100 transition-opacity"
                        >
                          <IconX size={16} />
                        </button>
                        {index === 0 && (
                          <span className="absolute bottom-1 left-1 bg-primary text-white text-xs px-2 py-0.5 rounded">
                            Principal
                          </span>
                        )}
                      </div>
                    ))}
                  </div>
                )}
              </div>

              <div>
                <label className="label flex items-center gap-2">
                  <IconVideo size={18} />
                  Videos
                </label>
                <div className="border-2 border-dashed border-border rounded-lg p-6 text-center hover:border-primary transition-colors cursor-pointer">
                  <input
                    type="file"
                    accept="video/*"
                    multiple
                    className="hidden"
                    id="video-upload"
                    onChange={(e) => {
                      if (e.target.files) {
                        setVideos(Array.from(e.target.files))
                      }
                    }}
                  />
                  <label htmlFor="video-upload" className="cursor-pointer">
                    <IconUpload size={32} className="mx-auto mb-2 text-text-muted" />
                    <p className="text-sm text-text-muted">
                      {videos.length > 0 ? `${videos.length} video(s) seleccionado(s)` : 'Click para subir videos'}
                    </p>
                  </label>
                </div>
              </div>
            </div>

            {/* Actions */}
            <div className="flex items-center justify-end gap-3 pt-4 border-t border-border">
              <button
                type="button"
                onClick={onClose}
                className="btn-outline"
                disabled={saving}
              >
                Cancelar
              </button>
              <button
                type="submit"
                className="btn-primary"
                disabled={saving}
              >
                {saving ? 'Guardando...' : exercise ? 'Actualizar' : 'Crear'}
              </button>
            </div>
          </div>
        </form>
      </div>
    </div>
  )
}
