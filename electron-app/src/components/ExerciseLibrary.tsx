import { useState, useEffect } from 'react'
import { IconSearch, IconBarbell, IconPhoto, IconFolder } from '@tabler/icons-react'
import type { Exercise } from '../models/types'
import { useExercises } from '../hooks/useExercises'

interface ExerciseImage {
  id: number
  exercise_id: number
  image_path: string
  is_primary: boolean
}

export default function ExerciseLibrary() {
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedMuscle, setSelectedMuscle] = useState('')
  const [selectedEquipment, setSelectedEquipment] = useState('')

  const { exercises, loading } = useExercises({
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
            <ExerciseCard key={exercise.id} exercise={exercise} />
          ))}
        </div>
      )}

      {!loading && filteredExercises.length === 0 && (
        <div className="card text-center py-12">
          <IconBarbell size={64} className="text-text-muted mx-auto mb-4" />
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
  const [imagePath, setImagePath] = useState<string | null>(null)
  const [imageLoading, setImageLoading] = useState(true)

  useEffect(() => {
    const loadImage = async () => {
      if (!exercise.id) return

      try {
        setImageLoading(true)
        const images = await window.electronAPI.db.getExerciseImages(exercise.id)
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
  }, [exercise.id])

  const handleOpenFolder = async () => {
    if (!exercise.id) return

    try {
      const images = await window.electronAPI.db.getExerciseImages(exercise.id)
      const primaryImage = images.find((img: ExerciseImage) => img.is_primary) || images[0]

      if (primaryImage) {
        await window.electronAPI.file.openFolder(primaryImage.image_path)
      } else {
        alert('Este ejercicio no tiene imágenes')
      }
    } catch (error) {
      console.error('Error opening folder:', error)
      alert('Error al abrir la carpeta')
    }
  }

  return (
    <div className="card hover:shadow-xl transition-all duration-300 group animate-scale-in">
      {/* Image Preview */}
      <div
        className="relative w-full h-48 bg-surface rounded-lg mb-4 overflow-hidden cursor-pointer"
        onClick={handleOpenFolder}
      >
        {imageLoading ? (
          <div className="absolute inset-0 flex items-center justify-center bg-gradient-to-br from-primary/10 to-secondary/10">
            <div className="spinner" style={{ width: '32px', height: '32px', borderWidth: '2px' }} />
          </div>
        ) : imagePath ? (
          <>
            <img
              src={`file://${imagePath}`}
              alt={exercise.spanish_name}
              className="w-full h-full object-cover"
              loading="lazy"
            />
            <div className="absolute inset-0 bg-black/0 group-hover:bg-black/50 transition-colors flex items-center justify-center">
              <div className="opacity-0 group-hover:opacity-100 transition-opacity flex flex-col items-center gap-2">
                <IconFolder size={48} className="text-white" />
                <span className="text-white font-semibold text-sm">Abrir Carpeta</span>
              </div>
            </div>
          </>
        ) : (
          <div className="absolute inset-0 flex items-center justify-center bg-gradient-to-br from-primary/10 to-secondary/10">
            <IconPhoto size={48} className="text-text-muted/30" />
          </div>
        )}
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

        {exercise.video_url && (
          <a
            href={exercise.video_url}
            target="_blank"
            rel="noopener noreferrer"
            className="flex items-center gap-2 text-primary hover:text-primary-dark text-sm font-semibold transition-colors"
            onClick={(e) => e.stopPropagation()}
          >
            <IconFolder size={16} />
            Ver Video Tutorial
          </a>
        )}
      </div>
    </div>
  )
}
