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

      // Upload images
      if (images.length > 0) {
        for (let i = 0; i < images.length; i++) {
          const file = images[i]
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

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label className="label text-base">Nombre (Inglés)</label>
              <input
                type="text"
                className="input text-base"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                required
                placeholder="Bench Press"
              />
            </div>
            <div>
              <label className="label text-base">Nombre (Español)</label>
              <input
                type="text"
                className="input text-base"
                value={formData.spanish_name}
                onChange={(e) => setFormData({ ...formData, spanish_name: e.target.value })}
                required
                placeholder="Press de Banca"
              />
            </div>
          </div>

          <div className="mt-6">
            <label className="label text-base">Descripción</label>
            <textarea
              className="input text-base"
              rows={3}
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              placeholder="Descripción breve del ejercicio..."
            />
          </div>

          <div className="mt-6">
            <label className="label text-base">Instrucciones</label>
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
                Imágenes del Ejercicio
              </label>
              <div className="border-2 border-dashed border-border rounded-xl p-8 text-center hover:border-primary transition-all cursor-pointer bg-surface/50">
                <input
                  type="file"
                  accept="image/*"
                  multiple
                  className="hidden"
                  id="image-upload"
                  onChange={(e) => handleImageChange(e.target.files)}
                />
                <label htmlFor="image-upload" className="cursor-pointer">
                  <IconUpload size={48} className="mx-auto mb-3 text-text-muted" />
                  <p className="text-base text-text mb-1 font-semibold">
                    {images.length > 0 ? `${images.length} imagen(es) seleccionada(s)` : 'Click para subir imágenes'}
                  </p>
                  <p className="text-sm text-text-muted">
                    La primera imagen será la principal
                  </p>
                </label>
              </div>

              {/* Image Previews */}
              {imagePreviews.length > 0 && (
                <div className="grid grid-cols-3 gap-3 mt-4">
                  {imagePreviews.map((preview, index) => (
                    <div key={index} className="relative group">
                      <img
                        src={preview}
                        alt={`Preview ${index + 1}`}
                        className="w-full h-28 object-cover rounded-lg border-2 border-border"
                      />
                      <button
                        type="button"
                        onClick={() => {
                          const newImages = images.filter((_, i) => i !== index)
                          const newPreviews = imagePreviews.filter((_, i) => i !== index)
                          setImages(newImages)
                          setImagePreviews(newPreviews)
                        }}
                        className="absolute -top-2 -right-2 bg-error text-white rounded-full p-1.5 opacity-0 group-hover:opacity-100 transition-opacity shadow-lg"
                      >
                        <IconX size={16} />
                      </button>
                      {index === 0 && (
                        <span className="absolute bottom-2 left-2 bg-primary text-white text-xs px-2 py-1 rounded-md font-semibold">
                          Principal
                        </span>
                      )}
                    </div>
                  ))}
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
