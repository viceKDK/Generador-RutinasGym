export interface Exercise {
  id: number
  name: string
  spanish_name: string
  description?: string
  instructions?: string
  primary_muscle_group: string
  secondary_muscle_group?: string
  equipment_type: string
  difficulty_level: DifficultyLevel
  exercise_type: ExerciseType
  video_url?: string
  is_active: boolean
  created_at?: string
}

export interface WorkoutPlan {
  id?: number
  userName: string
  userAge?: number
  fitnessLevel: FitnessLevel
  trainingDays: number
  goals: string[]
  routines: Routine[]
  created_at?: string
}

export interface Routine {
  id?: number
  dayNumber: number
  dayName: string
  focus: string
  exercises: RoutineExercise[]
}

export interface RoutineExercise {
  id?: number
  exerciseId: number
  exercise?: Exercise
  sets: number
  reps: string
  restSeconds: number
  notes?: string
  orderIndex: number
}

export enum FitnessLevel {
  Beginner = 'Principiante',
  Intermediate = 'Intermedio',
  Advanced = 'Avanzado',
}

export enum DifficultyLevel {
  Easy = 'Fácil',
  Medium = 'Medio',
  Hard = 'Difícil',
}

export enum ExerciseType {
  Strength = 'Fuerza',
  Cardio = 'Cardio',
  Flexibility = 'Flexibilidad',
  Balance = 'Balance',
}

export enum MuscleGroup {
  Chest = 'Pecho',
  Back = 'Espalda',
  Shoulders = 'Hombros',
  Arms = 'Brazos',
  Legs = 'Piernas',
  Core = 'Core',
  Glutes = 'Glúteos',
  Calves = 'Pantorrillas',
  FullBody = 'Cuerpo Completo',
}

export enum EquipmentType {
  Barbell = 'Barra',
  Dumbbell = 'Mancuernas',
  Cable = 'Polea',
  Machine = 'Máquina',
  Bodyweight = 'Peso Corporal',
  Kettlebell = 'Kettlebell',
  Bands = 'Bandas',
  None = 'Sin Equipo',
}

export interface UserProfile {
  name: string
  age?: number
  gender?: 'Masculino' | 'Femenino' | 'Otro'
  fitnessLevel: FitnessLevel
  trainingDays: number
  goals: string[]
  limitations?: string[]
}
