import type { UserProfile, WorkoutPlan, Routine, RoutineExercise, Exercise } from '../models/types'

interface MuscleGroupDay {
  dayName: string
  muscleGroups: string[]
}

export class RoutineGeneratorService {
  /**
   * Genera una rutina personalizada basada en el perfil del usuario
   */
  async generateRoutine(
    profile: UserProfile,
    exercises: Exercise[],
    useAI: boolean = true
  ): Promise<WorkoutPlan> {
    // 1. Determinar división de grupos musculares según días
    const muscleSplit = this.determineMuscleGroupSplit(
      profile.trainingDays,
      profile.fitnessLevel,
      profile.age || 30
    )

    // 2. Generar rutinas día por día
    const routines: Routine[] = []

    for (let i = 0; i < muscleSplit.length; i++) {
      const day = muscleSplit[i]
      const routine = this.generateRoutineForDay(
        day,
        exercises,
        profile,
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
  }

  /**
   * Genera rutina para un día específico
   */
  private generateRoutineForDay(
    day: MuscleGroupDay,
    exercises: Exercise[],
    profile: UserProfile,
    dayNumber: number
  ): Routine {
    const totalExercises = 5
    const distribution = this.calculateExerciseDistribution(
      day.muscleGroups.length,
      totalExercises
    )

    const routineExercises: RoutineExercise[] = []
    let orderIndex = 0

    // Seleccionar ejercicios para cada grupo muscular
    for (let i = 0; i < day.muscleGroups.length; i++) {
      const muscleGroup = day.muscleGroups[i]
      const exerciseCount = distribution[i]

      // Filtrar ejercicios del grupo muscular
      const muscleExercises = exercises.filter(
        (ex) =>
          ex.primary_muscle_group === muscleGroup ||
          ex.secondary_muscle_group === muscleGroup
      )

      // Seleccionar ejercicios (aleatorio)
      const selectedExercises = this.selectRandomExercises(
        muscleExercises,
        exerciseCount
      )

      // Determinar series y reps según nivel y objetivos
      const { sets, reps, rest } = this.getSeriesRepsRest(
        profile.fitnessLevel,
        profile.goals
      )

      // Agregar ejercicios a la rutina
      for (const exercise of selectedExercises) {
        routineExercises.push({
          exerciseId: exercise.id,
          exercise,
          sets,
          reps,
          restSeconds: rest,
          orderIndex: orderIndex++,
        })
      }
    }

    return {
      dayNumber,
      dayName: day.dayName,
      focus: day.muscleGroups.join(', '),
      exercises: routineExercises,
    }
  }

  /**
   * Determina división de grupos musculares según días disponibles
   */
  private determineMuscleGroupSplit(
    trainingDays: number,
    fitnessLevel: string,
    age: number
  ): MuscleGroupDay[] {
    const split: MuscleGroupDay[] = []

    switch (trainingDays) {
      case 1:
        split.push({
          dayName: 'Día 1 - Cuerpo Completo',
          muscleGroups: ['Pecho', 'Espalda', 'Piernas', 'Core'],
        })
        break

      case 2:
        split.push({
          dayName: 'Día 1 - Torso',
          muscleGroups: ['Pecho', 'Espalda', 'Hombros'],
        })
        split.push({
          dayName: 'Día 2 - Piernas y Brazos',
          muscleGroups: ['Piernas', 'Brazos', 'Core'],
        })
        break

      case 3:
        split.push({
          dayName: 'Día 1 - Empuje',
          muscleGroups: ['Pecho', 'Hombros', 'Triceps'],
        })
        split.push({
          dayName: 'Día 2 - Tirón',
          muscleGroups: ['Espalda', 'Biceps'],
        })
        split.push({
          dayName: 'Día 3 - Piernas',
          muscleGroups: ['Piernas', 'Glúteos', 'Core'],
        })
        break

      case 4:
        split.push({
          dayName: 'Día 1 - Pecho y Tríceps',
          muscleGroups: ['Pecho', 'Triceps'],
        })
        split.push({
          dayName: 'Día 2 - Piernas',
          muscleGroups: ['Piernas', 'Glúteos'],
        })
        split.push({
          dayName: 'Día 3 - Espalda y Bíceps',
          muscleGroups: ['Espalda', 'Biceps'],
        })
        split.push({
          dayName: 'Día 4 - Hombros y Core',
          muscleGroups: ['Hombros', 'Core'],
        })
        break

      case 5:
        split.push({ dayName: 'Día 1 - Pecho', muscleGroups: ['Pecho', 'Triceps'] })
        split.push({ dayName: 'Día 2 - Espalda', muscleGroups: ['Espalda', 'Biceps'] })
        split.push({ dayName: 'Día 3 - Piernas', muscleGroups: ['Piernas'] })
        split.push({ dayName: 'Día 4 - Hombros', muscleGroups: ['Hombros', 'Core'] })
        split.push({ dayName: 'Día 5 - Glúteos', muscleGroups: ['Glúteos', 'Core'] })
        break

      case 6:
        split.push({ dayName: 'Día 1 - Pecho', muscleGroups: ['Pecho'] })
        split.push({ dayName: 'Día 2 - Espalda', muscleGroups: ['Espalda'] })
        split.push({ dayName: 'Día 3 - Piernas', muscleGroups: ['Piernas'] })
        split.push({ dayName: 'Día 4 - Hombros', muscleGroups: ['Hombros'] })
        split.push({ dayName: 'Día 5 - Brazos', muscleGroups: ['Brazos'] })
        split.push({ dayName: 'Día 6 - Core y Glúteos', muscleGroups: ['Core', 'Glúteos'] })
        break

      case 7:
        split.push({ dayName: 'Día 1 - Pecho', muscleGroups: ['Pecho'] })
        split.push({ dayName: 'Día 2 - Espalda', muscleGroups: ['Espalda'] })
        split.push({ dayName: 'Día 3 - Piernas', muscleGroups: ['Piernas'] })
        split.push({ dayName: 'Día 4 - Hombros', muscleGroups: ['Hombros'] })
        split.push({ dayName: 'Día 5 - Bíceps', muscleGroups: ['Brazos'] })
        split.push({ dayName: 'Día 6 - Tríceps', muscleGroups: ['Brazos'] })
        split.push({ dayName: 'Día 7 - Core Full Body', muscleGroups: ['Core', 'Cuerpo Completo'] })
        break

      default:
        split.push({
          dayName: 'Día 1',
          muscleGroups: ['Pecho', 'Espalda', 'Piernas'],
        })
        break
    }

    return split
  }

  /**
   * Calcula distribución de ejercicios por grupo muscular
   */
  private calculateExerciseDistribution(
    muscleGroupCount: number,
    totalExercises: number
  ): number[] {
    const distribution = new Array(muscleGroupCount).fill(0)

    if (muscleGroupCount === 1) {
      distribution[0] = totalExercises
    } else if (muscleGroupCount === 2) {
      // Primer grupo: 3 ejercicios, Segundo: 2
      distribution[0] = 3
      distribution[1] = 2
    } else if (muscleGroupCount === 3) {
      // Distribución 2-2-1
      distribution[0] = 2
      distribution[1] = 2
      distribution[2] = 1
    } else {
      // Distribuir equitativamente
      const baseCount = Math.floor(totalExercises / muscleGroupCount)
      const remainder = totalExercises % muscleGroupCount

      for (let i = 0; i < muscleGroupCount; i++) {
        distribution[i] = baseCount + (i < remainder ? 1 : 0)
      }
    }

    return distribution
  }

  /**
   * Selecciona ejercicios aleatorios de una lista
   */
  private selectRandomExercises(exercises: Exercise[], count: number): Exercise[] {
    if (exercises.length === 0) return []
    if (exercises.length <= count) return exercises

    // Shuffle and take
    const shuffled = [...exercises].sort(() => Math.random() - 0.5)
    return shuffled.slice(0, count)
  }

  /**
   * Determina series, repeticiones y descanso según nivel y objetivos
   */
  private getSeriesRepsRest(
    fitnessLevel: string,
    goals: string[]
  ): { sets: number; reps: string; rest: number } {
    // Detectar objetivo principal
    const isPowerGoal = goals.some((g) => g.includes('fuerza') || g.includes('Aumentar fuerza'))
    const isMuscleGoal = goals.some((g) => g.includes('músculo') || g.includes('Ganar músculo'))
    const isEnduranceGoal = goals.some((g) => g.includes('resistencia'))
    const isWeightLossGoal = goals.some((g) => g.includes('peso') || g.includes('Perder peso'))

    // Principiante
    if (fitnessLevel === 'Principiante') {
      return {
        sets: 3,
        reps: '10-12',
        rest: 60,
      }
    }

    // Intermedio
    if (fitnessLevel === 'Intermedio') {
      if (isPowerGoal) {
        return { sets: 4, reps: '6-8', rest: 120 }
      }
      if (isMuscleGoal) {
        return { sets: 4, reps: '8-12', rest: 90 }
      }
      if (isEnduranceGoal || isWeightLossGoal) {
        return { sets: 3, reps: '12-15', rest: 45 }
      }
      return { sets: 3, reps: '10-12', rest: 60 }
    }

    // Avanzado
    if (isPowerGoal) {
      return { sets: 5, reps: '4-6', rest: 180 }
    }
    if (isMuscleGoal) {
      return { sets: 4, reps: '8-12', rest: 90 }
    }
    if (isEnduranceGoal || isWeightLossGoal) {
      return { sets: 4, reps: '15-20', rest: 30 }
    }

    return { sets: 4, reps: '8-12', rest: 90 }
  }
}
