import { useState, useEffect } from 'react'
import { IconBarbell, IconPhoto, IconFolder } from '@tabler/icons-react'
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
      <div className="flex items-center justify-between mb-6">
        <h1 className="section-title">Biblioteca de Ejercicios</h1>
        <div className="text-sm text-text-secondary bg-surface-light px-4 py-2 rounded-lg border border-border-gold">
          <span className="font-semibold text-primary">{filteredExercises.length}</span> ejercicios
        </div>
      </div>

      {/* Filters */}
      <div className="card mb-6">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label className="label">Buscar Ejercicio</label>
            <input
              type="text"
              className="input"
              placeholder="Nombre del ejercicio..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>

          <div>
            <label className="label">Grupo Muscular</label>
            <select
              className="input"
              value={selectedMuscle}
              onChange={(e) => setSelectedMuscle(e.target.value)}
            >
              <option value="">Todos los grupos</option>
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
              <option value="">Todo el equipo</option>
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
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-4">
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
        className="relative w-full h-36 bg-surface rounded-lg mb-3 overflow-hidden cursor-pointer"
        onClick={handleOpenFolder}
      >
        {imageLoading ? (
          <div className="absolute inset-0 flex items-center justify-center bg-gradient-to-br from-primary/10 to-secondary/10">
            <div className="spinner" style={{ width: '24px', height: '24px', borderWidth: '2px' }} />
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
              <div className="opacity-0 group-hover:opacity-100 transition-opacity flex flex-col items-center gap-1">
                <IconFolder size={32} className="text-white" />
                <span className="text-white font-semibold text-xs">Ver carpeta</span>
              </div>
            </div>
          </>
        ) : (
          <div className="absolute inset-0 flex items-center justify-center bg-gradient-to-br from-primary/10 to-secondary/10">
            <IconPhoto size={32} className="text-text-muted/30" />
          </div>
        )}
        <div className="absolute top-2 right-2 bg-secondary/90 backdrop-blur-sm px-2 py-0.5 rounded-lg text-xs font-semibold text-white">
          {exercise.exercise_type || 'Fuerza'}
        </div>
      </div>

      <div className="space-y-2">
        <h3 className="font-bold text-sm text-text group-hover:text-primary transition-colors line-clamp-2">
          {exercise.spanish_name}
        </h3>

        <div className="flex items-center gap-2 text-xs">
          <div className="flex items-center gap-1">
            <div className="w-1.5 h-1.5 rounded-full bg-primary"></div>
            <span className="text-text-muted truncate">{exercise.primary_muscle_group}</span>
          </div>
        </div>

        <div className="flex items-center gap-1 text-xs">
          <span className="px-2 py-0.5 bg-surface rounded-full text-text-muted truncate">
            {exercise.equipment_type}
          </span>
        </div>
      </div>
    </div>
  )
}
