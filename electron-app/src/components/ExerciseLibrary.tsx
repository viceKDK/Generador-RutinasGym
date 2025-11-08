import { useState, useEffect } from 'react'
import { IconSearch, IconBarbell, IconPhoto, IconZoomIn } from '@tabler/icons-react'
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

  return (
    <div
      className="card hover:shadow-xl transition-all duration-300 cursor-pointer group animate-scale-in"
      onClick={onClick}
    >
      {/* Image Preview */}
      <div className="relative w-full h-48 bg-surface rounded-lg mb-4 overflow-hidden">
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
            <div className="absolute inset-0 bg-black/0 group-hover:bg-black/10 transition-colors flex items-center justify-center">
              <IconZoomIn
                size={32}
                className="text-white opacity-0 group-hover:opacity-100 transition-opacity"
              />
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
      </div>
    </div>
  )
}

interface ExerciseDetailModalProps {
  exercise: Exercise
  onClose: () => void
}

function ExerciseDetailModal({ exercise, onClose }: ExerciseDetailModalProps) {
  const [images, setImages] = useState<string[]>([])
  const [currentImageIndex, setCurrentImageIndex] = useState(0)
  const [imagesLoading, setImagesLoading] = useState(true)
  const [lightboxOpen, setLightboxOpen] = useState(false)

  useEffect(() => {
    const loadImages = async () => {
      if (!exercise.id) return

      try {
        setImagesLoading(true)
        const exerciseImages = await window.electronAPI.db.getExerciseImages(exercise.id)

        const paths = await Promise.all(
          exerciseImages.map(async (img: ExerciseImage) => {
            return await window.electronAPI.file.getPath(img.image_path)
          })
        )

        setImages(paths.filter(Boolean))
      } catch (error) {
        console.error('Error loading images:', error)
      } finally {
        setImagesLoading(false)
      }
    }

    loadImages()
  }, [exercise.id])

  return (
    <>
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
              {/* Main Image */}
              <div className="relative w-full h-64 bg-surface rounded-lg overflow-hidden group">
                {imagesLoading ? (
                  <div className="absolute inset-0 flex items-center justify-center">
                    <div className="spinner" />
                  </div>
                ) : images.length > 0 ? (
                  <>
                    <img
                      src={`file://${images[currentImageIndex]}`}
                      alt={exercise.spanish_name}
                      className="w-full h-full object-cover cursor-pointer"
                      onClick={() => setLightboxOpen(true)}
                    />
                    <button
                      onClick={() => setLightboxOpen(true)}
                      className="absolute inset-0 bg-black/0 hover:bg-black/20 transition-colors flex items-center justify-center"
                    >
                      <IconZoomIn size={48} className="text-white opacity-0 group-hover:opacity-100 transition-opacity" />
                    </button>
                  </>
                ) : (
                  <div className="absolute inset-0 flex items-center justify-center">
                    <IconPhoto size={64} className="text-text-muted/30" />
                  </div>
                )}
              </div>

              {/* Image Thumbnails */}
              {images.length > 1 && (
                <div className="flex gap-2 overflow-x-auto pb-2">
                  {images.map((imgPath, index) => (
                    <button
                      key={index}
                      onClick={() => setCurrentImageIndex(index)}
                      className={`flex-shrink-0 w-20 h-20 rounded-lg overflow-hidden border-2 transition-all ${
                        currentImageIndex === index
                          ? 'border-primary ring-2 ring-primary/20'
                          : 'border-border hover:border-primary/50'
                      }`}
                    >
                      <img
                        src={`file://${imgPath}`}
                        alt={`${exercise.spanish_name} ${index + 1}`}
                        className="w-full h-full object-cover"
                      />
                    </button>
                  ))}
                </div>
              )}

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

      {/* Lightbox for Image Zoom */}
      {lightboxOpen && images.length > 0 && (
        <ImageLightbox
          images={images}
          currentIndex={currentImageIndex}
          onClose={() => setLightboxOpen(false)}
          onNavigate={setCurrentImageIndex}
        />
      )}
    </>
  )
}

interface ImageLightboxProps {
  images: string[]
  currentIndex: number
  onClose: () => void
  onNavigate: (index: number) => void
}

function ImageLightbox({ images, currentIndex, onClose, onNavigate }: ImageLightboxProps) {
  const handlePrevious = () => {
    onNavigate(currentIndex > 0 ? currentIndex - 1 : images.length - 1)
  }

  const handleNext = () => {
    onNavigate(currentIndex < images.length - 1 ? currentIndex + 1 : 0)
  }

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose()
      if (e.key === 'ArrowLeft') handlePrevious()
      if (e.key === 'ArrowRight') handleNext()
    }

    window.addEventListener('keydown', handleKeyDown)
    return () => window.removeEventListener('keydown', handleKeyDown)
  }, [currentIndex])

  return (
    <div
      className="fixed inset-0 bg-black/90 backdrop-blur-md flex items-center justify-center z-[100] p-4 animate-fade-in"
      onClick={onClose}
    >
      {/* Close Button */}
      <button
        onClick={onClose}
        className="absolute top-4 right-4 text-white hover:text-white/80 transition-colors z-10"
      >
        <span className="text-3xl font-light">✕</span>
      </button>

      {/* Navigation Buttons */}
      {images.length > 1 && (
        <>
          <button
            onClick={(e) => {
              e.stopPropagation()
              handlePrevious()
            }}
            className="absolute left-4 top-1/2 -translate-y-1/2 text-white hover:text-white/80 transition-colors bg-black/30 hover:bg-black/50 rounded-full p-3"
          >
            <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
            </svg>
          </button>
          <button
            onClick={(e) => {
              e.stopPropagation()
              handleNext()
            }}
            className="absolute right-4 top-1/2 -translate-y-1/2 text-white hover:text-white/80 transition-colors bg-black/30 hover:bg-black/50 rounded-full p-3"
          >
            <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
            </svg>
          </button>
        </>
      )}

      {/* Image Counter */}
      {images.length > 1 && (
        <div className="absolute bottom-4 left-1/2 -translate-x-1/2 bg-black/50 text-white px-4 py-2 rounded-full text-sm">
          {currentIndex + 1} / {images.length}
        </div>
      )}

      {/* Main Image */}
      <div
        className="max-w-[90vw] max-h-[90vh] flex items-center justify-center"
        onClick={(e) => e.stopPropagation()}
      >
        <img
          src={`file://${images[currentIndex]}`}
          alt={`Image ${currentIndex + 1}`}
          className="max-w-full max-h-full object-contain rounded-lg shadow-2xl animate-scale-in"
        />
      </div>
    </div>
  )
}
