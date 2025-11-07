import { useState } from 'react'
import type { UserProfile, WorkoutPlan, Exercise } from '../models/types'
import { RoutineGeneratorService } from '../services/RoutineGeneratorService'
import { OllamaService } from '../services/OllamaService'

export function useRoutineGenerator() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [generatedPlan, setGeneratedPlan] = useState<WorkoutPlan | null>(null)

  const routineGenerator = new RoutineGeneratorService()
  const ollamaService = new OllamaService()

  /**
   * Genera una rutina usando IA o fallback
   */
  const generateRoutine = async (
    profile: UserProfile,
    useAI: boolean = true
  ): Promise<WorkoutPlan | null> => {
    setLoading(true)
    setError(null)

    try {
      // Obtener ejercicios disponibles
      const exercises = await window.electronAPI.db.getExercises()

      let plan: WorkoutPlan

      if (useAI) {
        // Intentar con IA
        try {
          plan = await ollamaService.generateRoutineWithAI(profile, exercises)
        } catch (aiError) {
          console.warn('AI generation failed, using fallback:', aiError)
          plan = await routineGenerator.generateRoutine(profile, exercises, false)
        }
      } else {
        // Generación sin IA
        plan = await routineGenerator.generateRoutine(profile, exercises, false)
      }

      setGeneratedPlan(plan)

      // Guardar en base de datos
      await savePlanToDatabase(plan)

      return plan
    } catch (err: any) {
      setError(err.message || 'Error generating routine')
      console.error('Error generating routine:', err)
      return null
    } finally {
      setLoading(false)
    }
  }

  /**
   * Guarda el plan en la base de datos
   */
  const savePlanToDatabase = async (plan: WorkoutPlan): Promise<number | null> => {
    try {
      const planId = await window.electronAPI.db.saveWorkoutPlan(plan)

      // Guardar rutinas
      for (const routine of plan.routines) {
        const routineId = await window.electronAPI.db.saveRoutine(planId, routine)

        // Guardar ejercicios de la rutina
        for (const exercise of routine.exercises) {
          await window.electronAPI.db.saveRoutineExercise(routineId, exercise)
        }
      }

      return planId
    } catch (err) {
      console.error('Error saving plan to database:', err)
      return null
    }
  }

  /**
   * Verifica si Ollama está disponible
   */
  const checkOllamaStatus = async (): Promise<boolean> => {
    try {
      return await ollamaService.isAvailable()
    } catch {
      return false
    }
  }

  return {
    loading,
    error,
    generatedPlan,
    generateRoutine,
    checkOllamaStatus,
  }
}
