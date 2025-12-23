import { useState, useEffect } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { IconArrowLeft, IconPhoto, IconVideo, IconUpload, IconLink, IconX, IconDeviceFloppy } from '@tabler/icons-react'
import type { Exercise } from '../models/types'

export default function CreateEditExercise() {
  const navigate = useNavigate()
  const location = useLocation()
  const exercise = location.state?.exercise as Exercise | null

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
    if (!files || files.length === 0) return

    // Solo tomar el primer archivo (solo 1 imagen permitida)
    const file = files[0]
    setImages([file])

    // Generate preview
    const preview = URL.createObjectURL(file)
    setImagePreviews([preview])
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
      const exerciseData = {
        name: formData.name,
        spanish_name: formData.spanish_name,
        description: formData.description,
        instructions: formData.instructions,
        primary_muscle_group: formData.primary_muscle_group,
        secondary_muscle_group: formData.secondary_muscle_group,
        equipment_type: formData.equipment_type,
        exercise_type: formData.exercise_type,
        video_url: formData.video_url,
      }

      let exerciseId: number

      if (exercise?.id) {
        // Update existing
        await window.electronAPI.db.updateExercise(exercise.id, exerciseData)
        exerciseId = exercise.id
      } else {
        // Create new
        exerciseId = await window.electronAPI.db.createExercise(exerciseData)
      }

      // Upload image (solo 1 imagen, reemplaza la existente)
      if (images.length > 0) {
        // Si estamos editando, primero eliminar imágenes existentes
        if (exercise?.id) {
          try {
            const existingImages = await window.electronAPI.db.getExerciseImages(exerciseId)
            for (const img of existingImages) {
              await window.electronAPI.db.deleteExerciseImage(img.id)
            }
          } catch (err) {
            console.error('Error deleting existing images:', err)
          }
        }

        const file = images[0]
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
                  true // Siempre es la imagen principal
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

      // Upload videos
      if (videos.length > 0) {
        for (const file of videos) {
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

      // Navigate back to manager
      navigate('/manager')
    } catch (error) {
      console.error('Error saving exercise:', error)
      alert('Error al guardar el ejercicio')
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="animate-fade-in max-w-5xl mx-auto">
      {/* Header */}
      <div className="mb-8">
        <button
          onClick={() => navigate('/manager')}
          className="flex items-center gap-2 text-text-muted hover:text-text mb-4 transition-colors"
        >
          <IconArrowLeft size={20} />
          <span>Volver al Gestor</span>
        </button>

        <h1 className="section-title">
          {exercise ? 'Editar Ejercicio' : 'Nuevo Ejercicio'}
        </h1>
        <p className="text-text-muted text-lg">
          {exercise
            ? 'Modifica la información del ejercicio'
            : 'Completa los datos para crear un nuevo ejercicio'}
        </p>
      </div>

      {/* Form */}
      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Basic Info Card */}
        <div className="card">
          <h2 className="text-xl font-bold text-text mb-6">Información Básica</h2>

          <div>
            <label className="label text-base">Nombre del Ejercicio</label>
            <input
              type="text"
              className="input text-base"
              value={formData.spanish_name}
              onChange={(e) => setFormData({ ...formData, spanish_name: e.target.value, name: e.target.value })}
              required
              placeholder="Press de Banca"
            />
          </div>

          <div className="mt-6">
            <label className="label text-base">Descripción (opcional)</label>
            <textarea
              className="input text-base"
              rows={3}
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              placeholder="Descripción breve del ejercicio..."
            />
          </div>

          <div className="mt-6">
            <label className="label text-base">Instrucciones (opcional)</label>
            <textarea
              className="input text-base"
              rows={6}
              value={formData.instructions}
              onChange={(e) => setFormData({ ...formData, instructions: e.target.value })}
              placeholder="Pasos detallados para realizar el ejercicio..."
            />
          </div>
        </div>

        {/* Categories Card */}
        <div className="card">
          <h2 className="text-xl font-bold text-text mb-6">Categorías</h2>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label className="label text-base">Grupo Muscular Principal</label>
              <select
                className="input text-base"
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
              <label className="label text-base">Grupo Muscular Secundario</label>
              <select
                className="input text-base"
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
              <label className="label text-base">Tipo de Equipo</label>
              <select
                className="input text-base"
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
              <label className="label text-base">Tipo de Ejercicio</label>
              <select
                className="input text-base"
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
        </div>

        {/* Video URL Card */}
        <div className="card">
          <h2 className="text-xl font-bold text-text mb-6 flex items-center gap-2">
            <IconLink size={24} />
            Video Tutorial
          </h2>

          <div>
            <label className="label text-base">URL del Video (YouTube, Vimeo, etc.)</label>
            <input
              type="url"
              className="input text-base"
              placeholder="https://www.youtube.com/watch?v=..."
              value={formData.video_url}
              onChange={(e) => setFormData({ ...formData, video_url: e.target.value })}
            />
          </div>
        </div>

        {/* Media Upload Card */}
        <div className="card">
          <h2 className="text-xl font-bold text-text mb-6">Multimedia</h2>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label className="label text-base flex items-center gap-2">
                <IconPhoto size={20} />
                Imagen del Ejercicio
              </label>
              <div className="border-2 border-dashed border-border rounded-xl p-8 text-center hover:border-primary transition-all cursor-pointer bg-surface/50">
                <input
                  type="file"
                  accept="image/*"
                  className="hidden"
                  id="image-upload"
                  onChange={(e) => handleImageChange(e.target.files)}
                />
                <label htmlFor="image-upload" className="cursor-pointer">
                  <IconUpload size={48} className="mx-auto mb-3 text-text-muted" />
                  <p className="text-base text-text mb-1 font-semibold">
                    {images.length > 0 ? 'Imagen seleccionada' : 'Click para subir imagen'}
                  </p>
                  <p className="text-sm text-text-muted">
                    Solo 1 imagen por ejercicio
                  </p>
                </label>
              </div>

              {/* Image Preview */}
              {imagePreviews.length > 0 && (
                <div className="mt-4">
                  <div className="relative group">
                    <img
                      src={imagePreviews[0]}
                      alt="Preview"
                      className="w-full h-48 object-cover rounded-lg border-2 border-primary"
                    />
                    <button
                      type="button"
                      onClick={() => {
                        setImages([])
                        setImagePreviews([])
                      }}
                      className="absolute -top-2 -right-2 bg-error text-white rounded-full p-1.5 opacity-0 group-hover:opacity-100 transition-opacity shadow-lg"
                    >
                      <IconX size={16} />
                    </button>
                  </div>
                </div>
              )}
            </div>

            <div>
              <label className="label text-base flex items-center gap-2">
                <IconVideo size={20} />
                Videos del Ejercicio
              </label>
              <div className="border-2 border-dashed border-border rounded-xl p-8 text-center hover:border-secondary transition-all cursor-pointer bg-surface/50">
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
                  <IconUpload size={48} className="mx-auto mb-3 text-text-muted" />
                  <p className="text-base text-text mb-1 font-semibold">
                    {videos.length > 0 ? `${videos.length} video(s) seleccionado(s)` : 'Click para subir videos'}
                  </p>
                  <p className="text-sm text-text-muted">
                    Opcional: videos demostrativos
                  </p>
                </label>
              </div>
            </div>
          </div>
        </div>

        {/* Actions */}
        <div className="flex items-center justify-end gap-4 pt-4">
          <button
            type="button"
            onClick={() => navigate('/manager')}
            className="px-8 py-3 text-base border-2 border-border text-text hover:bg-surface rounded-xl transition-all font-semibold"
            disabled={saving}
          >
            Cancelar
          </button>
          <button
            type="submit"
            className="btn-primary px-8 py-3 text-base flex items-center gap-2"
            disabled={saving}
          >
            <IconDeviceFloppy size={20} />
            {saving ? 'Guardando...' : exercise ? 'Actualizar Ejercicio' : 'Crear Ejercicio'}
          </button>
        </div>
      </form>
    </div>
  )
}
