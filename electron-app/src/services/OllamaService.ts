import type { UserProfile, WorkoutPlan, Routine } from '../models/types'
import { RoutineGeneratorService } from './RoutineGeneratorService'

interface OllamaResponse {
  model: string
  created_at: string
  response: string
  done: boolean
}

interface MuscleGroupDay {
  dayName: string
  muscleGroups: string[]
}

export class OllamaService {
  private readonly ollamaUrl: string
  private readonly model: string
  private routineGenerator: RoutineGeneratorService

  constructor(ollamaUrl: string = 'http://localhost:11434', model: string = 'mistral') {
    this.ollamaUrl = ollamaUrl
    this.model = model
    this.routineGenerator = new RoutineGeneratorService()
  }

  /**
   * Verifica si Ollama está disponible
   */
  async isAvailable(): Promise<boolean> {
    try {
      const response = await fetch(`${this.ollamaUrl}/api/tags`, {
        method: 'GET',
      })
      return response.ok
    } catch (error) {
      console.error('Ollama not available:', error)
      return false
    }
  }

  /**
   * Genera una rutina usando IA de Ollama
   */
  async generateRoutineWithAI(
    profile: UserProfile,
    availableExercises: any[]
  ): Promise<WorkoutPlan> {
    const isAvailable = await this.isAvailable()

    if (!isAvailable) {
      console.warn('Ollama not available, using fallback generator')
      return this.routineGenerator.generateRoutine(profile, availableExercises, false)
    }

    try {
      // Obtener división de grupos musculares
      const muscleSplit = this.determineMuscleGroupSplit(
        profile.trainingDays,
        profile.fitnessLevel
      )

      const routines: Routine[] = []

      // Generar rutina para cada día
      for (let i = 0; i < muscleSplit.length; i++) {
        const day = muscleSplit[i]
        const prompt = this.buildPrompt(profile, day, availableExercises, i + 1)

        const aiResponse = await this.callOllamaAPI(prompt)
        const routine = this.parseAIResponse(
          aiResponse,
          day.dayName,
          day.muscleGroups,
          availableExercises,
          i + 1
        )

        routines.push(routine)
      }

      return {
        userName: profile.name,
        userAge: profile.age,
        fitnessLevel: profile.fitnessLevel,
        trainingDays: profile.trainingDays,
        goals: profile.goals,
        routines,
      }
    } catch (error) {
      console.error('Error generating routine with AI:', error)
      // Fallback a generación sin IA
      return this.routineGenerator.generateRoutine(profile, availableExercises, false)
    }
  }

  /**
   * Llama a la API de Ollama
   */
  private async callOllamaAPI(prompt: string): Promise<string> {
    const response = await fetch(`${this.ollamaUrl}/api/generate`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        model: this.model,
        prompt,
        stream: false,
        options: {
          temperature: 0.7,
          top_p: 0.9,
        },
      }),
    })

    if (!response.ok) {
      throw new Error(`Ollama API error: ${response.statusText}`)
    }

    const data: OllamaResponse = await response.json()
    return data.response
  }

  /**
   * Construye el prompt para IA
   */
  private buildPrompt(
    profile: UserProfile,
    day: MuscleGroupDay,
    exercises: any[],
    dayNumber: number
  ): string {
    // Filtrar ejercicios disponibles para estos grupos musculares
    const relevantExercises = exercises.filter((ex) =>
      day.muscleGroups.some(
        (muscle) =>
          ex.primary_muscle_group === muscle || ex.secondary_muscle_group === muscle
      )
    )

    const exerciseList = relevantExercises
      .slice(0, 20) // Limitar para no saturar el prompt
      .map((ex) => `- ${ex.spanish_name} (${ex.primary_muscle_group})`)
      .join('\n')

    const goalsStr = profile.goals.join(', ')

    return `Eres un entrenador personal experto. Genera una rutina de entrenamiento para: ${day.dayName}

CLIENTE:
- Nombre: ${profile.name}
- Edad: ${profile.age || 'No especificada'}
- Nivel: ${profile.fitnessLevel}
- Objetivos: ${goalsStr}

GRUPOS MUSCULARES A TRABAJAR: ${day.muscleGroups.join(', ')}

EJERCICIOS DISPONIBLES:
${exerciseList}

INSTRUCCIONES:
1. Selecciona EXACTAMENTE 5 ejercicios de la lista de arriba
2. Usa SOLO los nombres de ejercicios de la lista (NO inventes otros)
3. Distribuye los ejercicios entre los grupos musculares: ${day.muscleGroups.join(', ')}
4. Especifica series, repeticiones y descanso apropiados para el nivel ${profile.fitnessLevel}

FORMATO DE RESPUESTA (seguir estrictamente):
Ejercicio 1: [Nombre del ejercicio]
Series: [número]
Repeticiones: [rango o número]
Descanso: [segundos]

Ejercicio 2: [Nombre del ejercicio]
Series: [número]
Repeticiones: [rango o número]
Descanso: [segundos]

... (continuar para los 5 ejercicios)

NO agregues explicaciones adicionales, solo la lista de ejercicios en el formato especificado.`
  }

  /**
   * Parsea la respuesta de la IA
   */
  private parseAIResponse(
    aiResponse: string,
    dayName: string,
    muscleGroups: string[],
    availableExercises: any[],
    dayNumber: number
  ): Routine {
    const exercises: any[] = []
    const lines = aiResponse.split('\n').filter((line) => line.trim())

    let currentExercise: any = {}
    let orderIndex = 0

    for (const line of lines) {
      const trimmedLine = line.trim()

      // Detectar inicio de ejercicio
      if (trimmedLine.match(/^Ejercicio\s+\d+:/i)) {
        if (currentExercise.name) {
          exercises.push({ ...currentExercise, orderIndex: orderIndex++ })
        }
        const exerciseName = trimmedLine.split(':')[1]?.trim()
        currentExercise = { name: exerciseName }
      }
      // Parsear series
      else if (trimmedLine.match(/^Series:/i)) {
        const value = trimmedLine.split(':')[1]?.trim()
        currentExercise.sets = parseInt(value) || 3
      }
      // Parsear repeticiones
      else if (trimmedLine.match(/^Repeticiones:/i)) {
        const value = trimmedLine.split(':')[1]?.trim()
        currentExercise.reps = value || '10-12'
      }
      // Parsear descanso
      else if (trimmedLine.match(/^Descanso:/i)) {
        const value = trimmedLine.split(':')[1]?.trim()
        const seconds = parseInt(value) || 60
        currentExercise.restSeconds = seconds
      }
    }

    // Agregar último ejercicio
    if (currentExercise.name) {
      exercises.push({ ...currentExercise, orderIndex: orderIndex++ })
    }

    // Si no se parsearon ejercicios correctamente, usar fallback
    if (exercises.length === 0) {
      return this.createFallbackRoutine(dayName, muscleGroups, availableExercises, dayNumber)
    }

    // Mapear ejercicios con la BD
    const routineExercises = exercises.map((ex) => {
      // Buscar ejercicio en la BD por nombre
      const foundExercise = availableExercises.find(
        (dbEx) =>
          dbEx.spanish_name.toLowerCase().includes(ex.name?.toLowerCase()) ||
          ex.name?.toLowerCase().includes(dbEx.spanish_name.toLowerCase())
      )

      return {
        exerciseId: foundExercise?.id || 0,
        exercise: foundExercise,
        sets: ex.sets || 3,
        reps: ex.reps || '10-12',
        restSeconds: ex.restSeconds || 60,
        orderIndex: ex.orderIndex,
      }
    })

    return {
      dayNumber,
      dayName,
      focus: muscleGroups.join(', '),
      exercises: routineExercises.filter((ex) => ex.exercise), // Filtrar ejercicios no encontrados
    }
  }

  /**
   * Crea una rutina de respaldo si falla el parsing
   */
  private createFallbackRoutine(
    dayName: string,
    muscleGroups: string[],
    availableExercises: any[],
    dayNumber: number
  ): Routine {
    const exercises: any[] = []

    // Seleccionar 5 ejercicios aleatorios de los grupos musculares
    const relevantExercises = availableExercises.filter((ex) =>
      muscleGroups.some(
        (muscle) =>
          ex.primary_muscle_group === muscle || ex.secondary_muscle_group === muscle
      )
    )

    const shuffled = [...relevantExercises].sort(() => Math.random() - 0.5)
    const selected = shuffled.slice(0, 5)

    selected.forEach((ex, index) => {
      exercises.push({
        exerciseId: ex.id,
        exercise: ex,
        sets: 3,
        reps: '10-12',
        restSeconds: 60,
        orderIndex: index,
      })
    })

    return {
      dayNumber,
      dayName,
      focus: muscleGroups.join(', '),
      exercises,
    }
  }

  /**
   * Determina división de grupos musculares
   */
  private determineMuscleGroupSplit(
    trainingDays: number,
    fitnessLevel: string
  ): MuscleGroupDay[] {
    // Usar la misma lógica del RoutineGeneratorService
    return this.routineGenerator['determineMuscleGroupSplit'](trainingDays, fitnessLevel, 30)
  }
}
