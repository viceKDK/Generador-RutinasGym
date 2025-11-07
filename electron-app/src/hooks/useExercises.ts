import { useState, useEffect } from 'react'
import type { Exercise } from '../models/types'

interface UseExercisesOptions {
  muscleGroup?: string
  equipment?: string
  autoLoad?: boolean
}

export function useExercises(options: UseExercisesOptions = {}) {
  const [exercises, setExercises] = useState<Exercise[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const loadExercises = async (filter?: any) => {
    setLoading(true)
    setError(null)

    try {
      const data = await window.electronAPI.db.getExercises(filter || {
        muscleGroup: options.muscleGroup,
        equipment: options.equipment,
      })
      setExercises(data)
    } catch (err: any) {
      setError(err.message || 'Error loading exercises')
      console.error('Error loading exercises:', err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    if (options.autoLoad !== false) {
      loadExercises()
    }
  }, [options.muscleGroup, options.equipment])

  return {
    exercises,
    loading,
    error,
    loadExercises,
    refetch: loadExercises,
  }
}
