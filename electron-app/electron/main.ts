import { app, BrowserWindow, ipcMain, dialog } from 'electron'
import path from 'path'
import { fileURLToPath } from 'url'
import Database from 'better-sqlite3'
import { seedDatabase } from './seed-data'

const __filename = fileURLToPath(import.meta.url)
const __dirname = path.dirname(__filename)

// Database instance
let db: Database.Database | null = null

function createWindow() {
  const mainWindow = new BrowserWindow({
    width: 1400,
    height: 900,
    minWidth: 1200,
    minHeight: 800,
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      nodeIntegration: false,
      contextIsolation: true,
    },
    icon: path.join(__dirname, '../../gym_icon_512.png'),
    title: 'Generador de Rutinas de Gimnasio',
    backgroundColor: '#1a1a1a',
  })

  // Development mode
  if (process.env.VITE_DEV_SERVER_URL) {
    mainWindow.loadURL(process.env.VITE_DEV_SERVER_URL)
    mainWindow.webContents.openDevTools()
  } else {
    // Production mode
    mainWindow.loadFile(path.join(__dirname, '../dist/index.html'))
  }

  // Initialize database
  initializeDatabase()
}

function initializeDatabase() {
  const dbPath = path.join(app.getPath('userData'), 'gymroutine.db')
  db = new Database(dbPath)

  // Create tables if they don't exist
  db.exec(`
    CREATE TABLE IF NOT EXISTS exercises (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      name TEXT NOT NULL,
      spanish_name TEXT NOT NULL,
      description TEXT,
      instructions TEXT,
      primary_muscle_group TEXT,
      secondary_muscle_group TEXT,
      equipment_type TEXT,
      difficulty_level TEXT,
      exercise_type TEXT,
      is_active BOOLEAN DEFAULT 1,
      created_at DATETIME DEFAULT CURRENT_TIMESTAMP
    );

    CREATE TABLE IF NOT EXISTS workout_plans (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      user_name TEXT NOT NULL,
      user_age INTEGER,
      fitness_level TEXT,
      training_days INTEGER,
      goals TEXT,
      created_at DATETIME DEFAULT CURRENT_TIMESTAMP
    );

    CREATE TABLE IF NOT EXISTS routines (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      workout_plan_id INTEGER,
      day_number INTEGER,
      day_name TEXT,
      focus TEXT,
      FOREIGN KEY (workout_plan_id) REFERENCES workout_plans(id)
    );

    CREATE TABLE IF NOT EXISTS routine_exercises (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      routine_id INTEGER,
      exercise_id INTEGER,
      sets INTEGER,
      reps TEXT,
      rest_seconds INTEGER,
      notes TEXT,
      order_index INTEGER,
      FOREIGN KEY (routine_id) REFERENCES routines(id),
      FOREIGN KEY (exercise_id) REFERENCES exercises(id)
    );

    CREATE TABLE IF NOT EXISTS exercise_images (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      exercise_id INTEGER,
      image_path TEXT,
      image_data BLOB,
      is_primary BOOLEAN DEFAULT 0,
      FOREIGN KEY (exercise_id) REFERENCES exercises(id)
    );
  `)

  // Seed database with exercises
  seedDatabase(db)

  console.log('Database initialized at:', dbPath)
}

// IPC Handlers for database operations
ipcMain.handle('db:getExercises', async (_, filter?: any) => {
  if (!db) return []

  let query = 'SELECT * FROM exercises WHERE is_active = 1'
  const params: any[] = []

  if (filter?.muscleGroup) {
    query += ' AND (primary_muscle_group = ? OR secondary_muscle_group = ?)'
    params.push(filter.muscleGroup, filter.muscleGroup)
  }

  if (filter?.equipment) {
    query += ' AND equipment_type = ?'
    params.push(filter.equipment)
  }

  const stmt = db.prepare(query)
  return stmt.all(...params)
})

ipcMain.handle('db:getExercise', async (_, id: number) => {
  if (!db) return null
  const stmt = db.prepare('SELECT * FROM exercises WHERE id = ?')
  return stmt.get(id)
})

ipcMain.handle('db:saveWorkoutPlan', async (_, plan: any) => {
  if (!db) return null

  const insert = db.prepare(`
    INSERT INTO workout_plans (user_name, user_age, fitness_level, training_days, goals)
    VALUES (?, ?, ?, ?, ?)
  `)

  const result = insert.run(
    plan.userName,
    plan.userAge,
    plan.fitnessLevel,
    plan.trainingDays,
    JSON.stringify(plan.goals)
  )

  return result.lastInsertRowid
})

ipcMain.handle('db:saveRoutine', async (_, workoutPlanId: number, routine: any) => {
  if (!db) return null

  const insert = db.prepare(`
    INSERT INTO routines (workout_plan_id, day_number, day_name, focus)
    VALUES (?, ?, ?, ?)
  `)

  const result = insert.run(
    workoutPlanId,
    routine.dayNumber,
    routine.dayName,
    routine.focus
  )

  return result.lastInsertRowid
})

ipcMain.handle('db:saveRoutineExercise', async (_, routineId: number, exercise: any) => {
  if (!db) return null

  const insert = db.prepare(`
    INSERT INTO routine_exercises (routine_id, exercise_id, sets, reps, rest_seconds, notes, order_index)
    VALUES (?, ?, ?, ?, ?, ?, ?)
  `)

  const result = insert.run(
    routineId,
    exercise.exerciseId,
    exercise.sets,
    exercise.reps,
    exercise.restSeconds,
    exercise.notes,
    exercise.orderIndex
  )

  return result.lastInsertRowid
})

// Ollama integration
ipcMain.handle('ollama:generateRoutine', async (_, params: any) => {
  try {
    const response = await fetch('http://localhost:11434/api/generate', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        model: 'mistral',
        prompt: `Genera una rutina de gimnasio personalizada para:
- Nombre: ${params.userName}
- Edad: ${params.userAge}
- Nivel: ${params.fitnessLevel}
- Días de entrenamiento: ${params.trainingDays}
- Objetivos: ${params.goals.join(', ')}

Proporciona una rutina detallada con ejercicios, series, repeticiones y descansos.`,
        stream: false,
      }),
    })

    if (!response.ok) {
      throw new Error('Ollama service not available')
    }

    const data = await response.json()
    return { success: true, data: data.response }
  } catch (error: any) {
    return { success: false, error: error.message }
  }
})

// Export to Word
ipcMain.handle('export:toWord', async (_, workoutPlan: any) => {
  const { filePath } = await dialog.showSaveDialog({
    title: 'Guardar rutina',
    defaultPath: `rutina-${workoutPlan.userName}-${Date.now()}.docx`,
    filters: [
      { name: 'Word Document', extensions: ['docx'] }
    ]
  })

  if (!filePath) return { success: false }

  // Implementation using docx library will go here
  return { success: true, path: filePath }
})

// App lifecycle
app.whenReady().then(() => {
  createWindow()

  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) {
      createWindow()
    }
  })
})

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    if (db) db.close()
    app.quit()
  }
})

app.on('before-quit', () => {
  if (db) db.close()
})
