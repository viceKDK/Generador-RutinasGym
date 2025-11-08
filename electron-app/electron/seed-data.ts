import type Database from 'better-sqlite3'

export interface SeedExercise {
  name: string
  spanish_name: string
  description: string
  instructions: string
  primary_muscle_group: string
  secondary_muscle_group?: string
  equipment_type: string
  difficulty_level: string
  exercise_type: string
}

// Ejercicios de ejemplo deshabilitados - Base de datos vacía para que el usuario agregue sus propios ejercicios
export const seedExercises: SeedExercise[] = []

/*
// EJERCICIOS COMENTADOS - Descomentar si se desea inicializar con datos de ejemplo
export const seedExercises: SeedExercise[] = [
  // PECHO
  {
    name: 'Barbell Bench Press',
    spanish_name: 'Press de Banca con Barra',
    description: 'Ejercicio compuesto para desarrollo de pecho',
    instructions: 'Acostado en banco plano, bajar barra al pecho y empujar hacia arriba',
    primary_muscle_group: 'Pecho',
    secondary_muscle_group: 'Triceps',
    equipment_type: 'Barra',
    difficulty_level: 'Medio',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Dumbbell Bench Press',
    spanish_name: 'Press de Banca con Mancuernas',
    description: 'Variación con mancuernas para mayor rango de movimiento',
    instructions: 'Similar al press de banca pero con mancuernas',
    primary_muscle_group: 'Pecho',
    secondary_muscle_group: 'Hombros',
    equipment_type: 'Mancuernas',
    difficulty_level: 'Medio',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Incline Dumbbell Press',
    spanish_name: 'Press Inclinado con Mancuernas',
    description: 'Enfoque en pecho superior',
    instructions: 'En banco inclinado 30-45 grados, presionar mancuernas hacia arriba',
    primary_muscle_group: 'Pecho',
    secondary_muscle_group: 'Hombros',
    equipment_type: 'Mancuernas',
    difficulty_level: 'Medio',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Chest Fly',
    spanish_name: 'Aperturas con Mancuernas',
    description: 'Ejercicio de aislamiento para pecho',
    instructions: 'Brazos semi-extendidos, abrir y cerrar en movimiento de abrazo',
    primary_muscle_group: 'Pecho',
    equipment_type: 'Mancuernas',
    difficulty_level: 'Fácil',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Push-ups',
    spanish_name: 'Flexiones',
    description: 'Ejercicio básico de peso corporal',
    instructions: 'Posición de plancha, bajar pecho al suelo y empujar hacia arriba',
    primary_muscle_group: 'Pecho',
    secondary_muscle_group: 'Triceps',
    equipment_type: 'Peso Corporal',
    difficulty_level: 'Fácil',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Cable Crossover',
    spanish_name: 'Cruces en Polea',
    description: 'Ejercicio de aislamiento con tensión constante',
    instructions: 'De pie entre poleas, cruzar cables al frente',
    primary_muscle_group: 'Pecho',
    equipment_type: 'Polea',
    difficulty_level: 'Medio',
    exercise_type: 'Fuerza',
  },

  // ESPALDA
  {
    name: 'Pull-ups',
    spanish_name: 'Dominadas',
    description: 'Ejercicio compuesto para espalda',
    instructions: 'Colgado de barra, tirar cuerpo hacia arriba hasta barbilla sobre barra',
    primary_muscle_group: 'Espalda',
    secondary_muscle_group: 'Biceps',
    equipment_type: 'Peso Corporal',
    difficulty_level: 'Difícil',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Barbell Row',
    spanish_name: 'Remo con Barra',
    description: 'Ejercicio compuesto para espalda media',
    instructions: 'Inclinado 45 grados, tirar barra hacia abdomen',
    primary_muscle_group: 'Espalda',
    secondary_muscle_group: 'Biceps',
    equipment_type: 'Barra',
    difficulty_level: 'Medio',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Lat Pulldown',
    spanish_name: 'Jalón al Pecho',
    description: 'Ejercicio para dorsales en máquina',
    instructions: 'Sentado, tirar barra hacia pecho',
    primary_muscle_group: 'Espalda',
    secondary_muscle_group: 'Biceps',
    equipment_type: 'Máquina',
    difficulty_level: 'Fácil',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Seated Cable Row',
    spanish_name: 'Remo en Polea Sentado',
    description: 'Ejercicio para espalda media y baja',
    instructions: 'Sentado, tirar cable hacia abdomen',
    primary_muscle_group: 'Espalda',
    equipment_type: 'Polea',
    difficulty_level: 'Fácil',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Deadlift',
    spanish_name: 'Peso Muerto',
    description: 'Ejercicio compuesto de cuerpo completo',
    instructions: 'Levantar barra desde suelo extendiendo caderas y rodillas',
    primary_muscle_group: 'Espalda',
    secondary_muscle_group: 'Piernas',
    equipment_type: 'Barra',
    difficulty_level: 'Difícil',
    exercise_type: 'Fuerza',
  },
  {
    name: 'T-Bar Row',
    spanish_name: 'Remo en T',
    description: 'Variación de remo para espalda gruesa',
    instructions: 'Inclinado, tirar barra en T hacia pecho',
    primary_muscle_group: 'Espalda',
    equipment_type: 'Barra',
    difficulty_level: 'Medio',
    exercise_type: 'Fuerza',
  },

  // HOMBROS
  {
    name: 'Military Press',
    spanish_name: 'Press Militar',
    description: 'Ejercicio compuesto para hombros',
    instructions: 'De pie o sentado, presionar barra desde hombros hacia arriba',
    primary_muscle_group: 'Hombros',
    secondary_muscle_group: 'Triceps',
    equipment_type: 'Barra',
    difficulty_level: 'Medio',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Dumbbell Shoulder Press',
    spanish_name: 'Press de Hombros con Mancuernas',
    description: 'Press de hombros con mancuernas',
    instructions: 'Sentado, presionar mancuernas desde hombros hacia arriba',
    primary_muscle_group: 'Hombros',
    equipment_type: 'Mancuernas',
    difficulty_level: 'Fácil',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Lateral Raise',
    spanish_name: 'Elevaciones Laterales',
    description: 'Aislamiento de hombro lateral',
    instructions: 'Brazos a los lados, elevar mancuernas lateralmente',
    primary_muscle_group: 'Hombros',
    equipment_type: 'Mancuernas',
    difficulty_level: 'Fácil',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Front Raise',
    spanish_name: 'Elevaciones Frontales',
    description: 'Aislamiento de hombro anterior',
    instructions: 'Elevar mancuernas al frente hasta altura de hombros',
    primary_muscle_group: 'Hombros',
    equipment_type: 'Mancuernas',
    difficulty_level: 'Fácil',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Face Pull',
    spanish_name: 'Jalón a la Cara',
    description: 'Ejercicio para hombro posterior',
    instructions: 'Tirar cuerda hacia cara separando manos',
    primary_muscle_group: 'Hombros',
    secondary_muscle_group: 'Espalda',
    equipment_type: 'Polea',
    difficulty_level: 'Fácil',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Arnold Press',
    spanish_name: 'Press Arnold',
    description: 'Variación de press con rotación',
    instructions: 'Press con rotación de muñecas de pronación a supinación',
    primary_muscle_group: 'Hombros',
    equipment_type: 'Mancuernas',
    difficulty_level: 'Medio',
    exercise_type: 'Fuerza',
  },

  // BRAZOS - BÍCEPS
  {
    name: 'Barbell Curl',
    spanish_name: 'Curl con Barra',
    description: 'Ejercicio básico de bíceps',
    instructions: 'De pie, flexionar codos elevando barra',
    primary_muscle_group: 'Brazos',
    equipment_type: 'Barra',
    difficulty_level: 'Fácil',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Dumbbell Curl',
    spanish_name: 'Curl con Mancuernas',
    description: 'Curl alterno con mancuernas',
    instructions: 'Alternar flexión de brazos con mancuernas',
    primary_muscle_group: 'Brazos',
    equipment_type: 'Mancuernas',
    difficulty_level: 'Fácil',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Hammer Curl',
    spanish_name: 'Curl Martillo',
    description: 'Curl con agarre neutro',
    instructions: 'Flexionar brazos con mancuernas en posición de martillo',
    primary_muscle_group: 'Brazos',
    equipment_type: 'Mancuernas',
    difficulty_level: 'Fácil',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Preacher Curl',
    spanish_name: 'Curl en Banco Scott',
    description: 'Curl aislado en banco',
    instructions: 'Brazos apoyados en banco, flexionar elevando peso',
    primary_muscle_group: 'Brazos',
    equipment_type: 'Barra',
    difficulty_level: 'Fácil',
    exercise_type: 'Fuerza',
  },

  // BRAZOS - TRÍCEPS
  {
    name: 'Triceps Dip',
    spanish_name: 'Fondos en Paralelas',
    description: 'Ejercicio compuesto para tríceps',
    instructions: 'En paralelas, bajar y subir cuerpo flexionando codos',
    primary_muscle_group: 'Brazos',
    secondary_muscle_group: 'Pecho',
    equipment_type: 'Peso Corporal',
    difficulty_level: 'Medio',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Triceps Pushdown',
    spanish_name: 'Extensión de Tríceps en Polea',
    description: 'Aislamiento de tríceps',
    instructions: 'Empujar cuerda o barra hacia abajo extendiendo codos',
    primary_muscle_group: 'Brazos',
    equipment_type: 'Polea',
    difficulty_level: 'Fácil',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Overhead Triceps Extension',
    spanish_name: 'Extensión de Tríceps sobre Cabeza',
    description: 'Extensión con brazos elevados',
    instructions: 'Brazos sobre cabeza, extender codos elevando peso',
    primary_muscle_group: 'Brazos',
    equipment_type: 'Mancuernas',
    difficulty_level: 'Fácil',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Close-Grip Bench Press',
    spanish_name: 'Press de Banca Agarre Cerrado',
    description: 'Press enfocado en tríceps',
    instructions: 'Press de banca con manos cerca del centro',
    primary_muscle_group: 'Brazos',
    secondary_muscle_group: 'Pecho',
    equipment_type: 'Barra',
    difficulty_level: 'Medio',
    exercise_type: 'Fuerza',
  },

  // PIERNAS
  {
    name: 'Barbell Squat',
    spanish_name: 'Sentadilla con Barra',
    description: 'Ejercicio rey para piernas',
    instructions: 'Barra en espalda, bajar flexionando rodillas y caderas',
    primary_muscle_group: 'Piernas',
    secondary_muscle_group: 'Glúteos',
    equipment_type: 'Barra',
    difficulty_level: 'Medio',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Front Squat',
    spanish_name: 'Sentadilla Frontal',
    description: 'Variación con barra al frente',
    instructions: 'Barra en hombros frontales, realizar sentadilla',
    primary_muscle_group: 'Piernas',
    equipment_type: 'Barra',
    difficulty_level: 'Difícil',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Leg Press',
    spanish_name: 'Prensa de Piernas',
    description: 'Ejercicio en máquina para piernas',
    instructions: 'Empujar plataforma con piernas',
    primary_muscle_group: 'Piernas',
    equipment_type: 'Máquina',
    difficulty_level: 'Fácil',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Lunges',
    spanish_name: 'Zancadas',
    description: 'Ejercicio unilateral para piernas',
    instructions: 'Dar pasos largos hacia adelante flexionando rodillas',
    primary_muscle_group: 'Piernas',
    secondary_muscle_group: 'Glúteos',
    equipment_type: 'Mancuernas',
    difficulty_level: 'Fácil',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Leg Extension',
    spanish_name: 'Extensión de Cuádriceps',
    description: 'Aislamiento de cuádriceps',
    instructions: 'Sentado, extender piernas contra resistencia',
    primary_muscle_group: 'Piernas',
    equipment_type: 'Máquina',
    difficulty_level: 'Fácil',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Leg Curl',
    spanish_name: 'Curl de Isquiotibiales',
    description: 'Aislamiento de isquiotibiales',
    instructions: 'Acostado o sentado, flexionar piernas contra resistencia',
    primary_muscle_group: 'Piernas',
    equipment_type: 'Máquina',
    difficulty_level: 'Fácil',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Romanian Deadlift',
    spanish_name: 'Peso Muerto Rumano',
    description: 'Enfoque en isquiotibiales y glúteos',
    instructions: 'Con ligera flexión de rodillas, bajar barra deslizando por piernas',
    primary_muscle_group: 'Piernas',
    secondary_muscle_group: 'Glúteos',
    equipment_type: 'Barra',
    difficulty_level: 'Medio',
    exercise_type: 'Fuerza',
  },

  // GLÚTEOS
  {
    name: 'Hip Thrust',
    spanish_name: 'Empuje de Cadera',
    description: 'Ejercicio principal para glúteos',
    instructions: 'Espalda en banco, empujar cadera hacia arriba con barra',
    primary_muscle_group: 'Glúteos',
    equipment_type: 'Barra',
    difficulty_level: 'Fácil',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Bulgarian Split Squat',
    spanish_name: 'Sentadilla Búlgara',
    description: 'Sentadilla unilateral elevada',
    instructions: 'Pie trasero elevado en banco, realizar sentadilla',
    primary_muscle_group: 'Glúteos',
    secondary_muscle_group: 'Piernas',
    equipment_type: 'Mancuernas',
    difficulty_level: 'Medio',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Glute Bridge',
    spanish_name: 'Puente de Glúteos',
    description: 'Ejercicio básico de glúteos',
    instructions: 'Acostado, elevar cadera contrayendo glúteos',
    primary_muscle_group: 'Glúteos',
    equipment_type: 'Peso Corporal',
    difficulty_level: 'Fácil',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Cable Kickback',
    spanish_name: 'Patada de Glúteo en Polea',
    description: 'Aislamiento de glúteos',
    instructions: 'De pie frente a polea, patear pierna hacia atrás',
    primary_muscle_group: 'Glúteos',
    equipment_type: 'Polea',
    difficulty_level: 'Fácil',
    exercise_type: 'Fuerza',
  },

  // CORE
  {
    name: 'Plank',
    spanish_name: 'Plancha',
    description: 'Ejercicio isométrico de core',
    instructions: 'Posición de flexión en antebrazos, mantener cuerpo recto',
    primary_muscle_group: 'Core',
    equipment_type: 'Peso Corporal',
    difficulty_level: 'Fácil',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Crunches',
    spanish_name: 'Abdominales',
    description: 'Ejercicio básico de abdomen',
    instructions: 'Acostado, flexionar tronco hacia rodillas',
    primary_muscle_group: 'Core',
    equipment_type: 'Peso Corporal',
    difficulty_level: 'Fácil',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Russian Twist',
    spanish_name: 'Giro Ruso',
    description: 'Ejercicio para oblicuos',
    instructions: 'Sentado semi-inclinado, girar torso de lado a lado',
    primary_muscle_group: 'Core',
    equipment_type: 'Peso Corporal',
    difficulty_level: 'Fácil',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Leg Raise',
    spanish_name: 'Elevación de Piernas',
    description: 'Ejercicio para abdomen bajo',
    instructions: 'Acostado o colgado, elevar piernas manteniendo rectas',
    primary_muscle_group: 'Core',
    equipment_type: 'Peso Corporal',
    difficulty_level: 'Medio',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Ab Wheel Rollout',
    spanish_name: 'Rueda Abdominal',
    description: 'Ejercicio avanzado de core',
    instructions: 'De rodillas, rodar rueda hacia adelante y regresar',
    primary_muscle_group: 'Core',
    equipment_type: 'Sin Equipo',
    difficulty_level: 'Difícil',
    exercise_type: 'Fuerza',
  },
  {
    name: 'Mountain Climbers',
    spanish_name: 'Escaladores',
    description: 'Ejercicio dinámico de core y cardio',
    instructions: 'Posición de plancha, alternar rodillas hacia pecho',
    primary_muscle_group: 'Core',
    equipment_type: 'Peso Corporal',
    difficulty_level: 'Medio',
    exercise_type: 'Cardio',
  },
]
*/

/**
 * Inserta los ejercicios seed en la base de datos
 */
export function seedDatabase(db: Database.Database): number {
  let insertedCount = 0

  // Verificar si ya hay ejercicios
  const count = db.prepare('SELECT COUNT(*) as count FROM exercises').get() as { count: number }

  if (count.count > 0) {
    console.log('Database already has exercises, skipping seed')
    return count.count
  }

  console.log('No se cargarán ejercicios de ejemplo - Base de datos vacía')

  const insert = db.prepare(`
    INSERT INTO exercises (
      name,
      spanish_name,
      description,
      instructions,
      primary_muscle_group,
      secondary_muscle_group,
      equipment_type,
      difficulty_level,
      exercise_type,
      is_active
    ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, 1)
  `)

  const insertMany = db.transaction((exercises: SeedExercise[]) => {
    for (const exercise of exercises) {
      insert.run(
        exercise.name,
        exercise.spanish_name,
        exercise.description,
        exercise.instructions,
        exercise.primary_muscle_group,
        exercise.secondary_muscle_group || null,
        exercise.equipment_type,
        exercise.difficulty_level,
        exercise.exercise_type
      )
      insertedCount++
    }
  })

  insertMany(seedExercises)

  console.log(`Successfully seeded ${insertedCount} exercises`)
  return insertedCount
}
