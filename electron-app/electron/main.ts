import { app, BrowserWindow, ipcMain, dialog, shell } from 'electron'
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
    // Tema premium: fondo oscuro + overlay de barra de título con acento dorado
    backgroundColor: '#121212',
    titleBarStyle: 'hidden',
    titleBarOverlay: {
      color: '#1C1C1E',
      symbolColor: '#D4AF37',
      height: 32,
    },
  })

  // Development mode
  if (process.env.VITE_DEV_SERVER_URL) {
    mainWindow.loadURL(process.env.VITE_DEV_SERVER_URL)
    // DevTools deshabilitado para producción
    // mainWindow.webContents.openDevTools()
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
      video_url TEXT,
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

  // Migration: Add video_url column if it doesn't exist
  try {
    const tableInfo = db.prepare('PRAGMA table_info(exercises)').all()
    const hasVideoUrl = tableInfo.some((col: any) => col.name === 'video_url')

    if (!hasVideoUrl) {
      db.exec('ALTER TABLE exercises ADD COLUMN video_url TEXT')
      console.log('Added video_url column to exercises table')
    }
  } catch (error) {
    console.error('Error adding video_url column:', error)
  }

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

// Create exercise
ipcMain.handle('db:createExercise', async (_, exercise: any) => {
  if (!db) return null

  const insert = db.prepare(`
    INSERT INTO exercises (
      name, spanish_name, description, instructions,
      primary_muscle_group, secondary_muscle_group,
      equipment_type, difficulty_level, exercise_type, video_url, is_active
    ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
  `)

  const result = insert.run(
    exercise.name,
    exercise.spanish_name,
    exercise.description || null,
    exercise.instructions || null,
    exercise.primary_muscle_group,
    exercise.secondary_muscle_group || null,
    exercise.equipment_type,
    exercise.difficulty_level || 'Medio',
    exercise.exercise_type || 'Fuerza',
    exercise.video_url || null,
    1
  )

  return result.lastInsertRowid
})

// Update exercise
ipcMain.handle('db:updateExercise', async (_, id: number, exercise: any) => {
  if (!db) return false

  const update = db.prepare(`
    UPDATE exercises SET
      name = ?,
      spanish_name = ?,
      description = ?,
      instructions = ?,
      primary_muscle_group = ?,
      secondary_muscle_group = ?,
      equipment_type = ?,
      difficulty_level = ?,
      exercise_type = ?,
      video_url = ?
    WHERE id = ?
  `)

  const result = update.run(
    exercise.name,
    exercise.spanish_name,
    exercise.description || null,
    exercise.instructions || null,
    exercise.primary_muscle_group,
    exercise.secondary_muscle_group || null,
    exercise.equipment_type,
    exercise.difficulty_level || 'Medio',
    exercise.exercise_type || 'Fuerza',
    exercise.video_url || null,
    id
  )

  return result.changes > 0
})

// Delete exercise (soft delete)
ipcMain.handle('db:deleteExercise', async (_, id: number) => {
  if (!db) return false

  const update = db.prepare('UPDATE exercises SET is_active = 0 WHERE id = ?')
  const result = update.run(id)

  return result.changes > 0
})

// Get exercise images
ipcMain.handle('db:getExerciseImages', async (_, exerciseId: number) => {
  if (!db) return []

  const stmt = db.prepare(`
    SELECT id, exercise_id, image_path, is_primary
    FROM exercise_images
    WHERE exercise_id = ?
    ORDER BY is_primary DESC, id ASC
  `)

  return stmt.all(exerciseId)
})

// Save exercise image
ipcMain.handle('db:saveExerciseImage', async (_, exerciseId: number, imagePath: string, isPrimary: boolean = false) => {
  if (!db) return null

  // If this is primary, unset other primary images
  if (isPrimary) {
    const unsetPrimary = db.prepare('UPDATE exercise_images SET is_primary = 0 WHERE exercise_id = ?')
    unsetPrimary.run(exerciseId)
  }

  const insert = db.prepare(`
    INSERT INTO exercise_images (exercise_id, image_path, is_primary)
    VALUES (?, ?, ?)
  `)

  const result = insert.run(exerciseId, imagePath, isPrimary ? 1 : 0)
  return result.lastInsertRowid
})

// Delete exercise image
ipcMain.handle('db:deleteExerciseImage', async (_, imageId: number) => {
  if (!db) return false

  const del = db.prepare('DELETE FROM exercise_images WHERE id = ?')
  const result = del.run(imageId)

  return result.changes > 0
})

// Upload image file
ipcMain.handle('file:uploadImage', async (_, imageData: string, exerciseId: number) => {
  try {
    const fs = await import('fs/promises')
    const path = await import('path')

    // Create images directory if it doesn't exist
    const imagesDir = path.join(app.getPath('userData'), 'exercise-images')
    await fs.mkdir(imagesDir, { recursive: true })

    // Generate unique filename
    const timestamp = Date.now()
    const filename = `exercise-${exerciseId}-${timestamp}.png`
    const filepath = path.join(imagesDir, filename)

    // Remove data:image/png;base64, prefix if present
    const base64Data = imageData.replace(/^data:image\/\w+;base64,/, '')
    const buffer = Buffer.from(base64Data, 'base64')

    // Save file
    await fs.writeFile(filepath, buffer)

    // Return the relative path
    return path.join('exercise-images', filename)
  } catch (error: any) {
    console.error('Error uploading image:', error)
    return null
  }
})

// Upload video file
ipcMain.handle('file:uploadVideo', async (_, videoData: string, exerciseId: number) => {
  try {
    const fs = await import('fs/promises')
    const path = await import('path')

    // Create videos directory if it doesn't exist
    const videosDir = path.join(app.getPath('userData'), 'exercise-videos')
    await fs.mkdir(videosDir, { recursive: true })

    // Generate unique filename
    const timestamp = Date.now()
    const filename = `exercise-${exerciseId}-${timestamp}.mp4`
    const filepath = path.join(videosDir, filename)

    // Remove data:video/mp4;base64, prefix if present
    const base64Data = videoData.replace(/^data:video\/\w+;base64,/, '')
    const buffer = Buffer.from(base64Data, 'base64')

    // Save file
    await fs.writeFile(filepath, buffer)

    // Return the relative path
    return path.join('exercise-videos', filename)
  } catch (error: any) {
    console.error('Error uploading video:', error)
    return null
  }
})

// Get file path (for loading images/videos)
ipcMain.handle('file:getPath', async (_, relativePath: string) => {
  const path = await import('path')
  return path.join(app.getPath('userData'), relativePath)
})

// Open folder containing file
ipcMain.handle('file:openFolder', async (_, relativePath: string) => {
  try {
    const path = await import('path')
    const absolutePath = path.join(app.getPath('userData'), relativePath)
    shell.showItemInFolder(absolutePath)
    return { success: true }
  } catch (error: any) {
    console.error('Error opening folder:', error)
    return { success: false, error: error.message }
  }
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
    // Get available exercises from database
    if (!db) {
      throw new Error('Database not available')
    }

    const exercises = db.prepare('SELECT id, spanish_name, name, primary_muscle_group, secondary_muscle_group, equipment_type, exercise_type FROM exercises WHERE is_active = 1').all()

    // Format exercises for prompt
    const exerciseList = exercises.map((ex: any) =>
      `- ID: ${ex.id} | ${ex.spanish_name} (${ex.name}) | Músculo: ${ex.primary_muscle_group}${ex.secondary_muscle_group ? ' / ' + ex.secondary_muscle_group : ''} | Equipo: ${ex.equipment_type} | Tipo: ${ex.exercise_type}`
    ).join('\n')

    const response = await fetch('http://localhost:11434/api/generate', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        model: 'mistral',
        prompt: `Eres un entrenador personal profesional. Genera una rutina de gimnasio personalizada usando EXCLUSIVAMENTE los ejercicios de esta lista.

EJERCICIOS DISPONIBLES EN BASE DE DATOS:
${exerciseList}

INFORMACIÓN DEL CLIENTE:
- Nombre: ${params.userName}
- Edad: ${params.userAge} años
- Nivel de fitness: ${params.fitnessLevel}
- Días de entrenamiento por semana: ${params.trainingDays}
- Objetivos: ${params.goals.join(', ')}

INSTRUCCIONES IMPORTANTES:
1. USA SOLAMENTE los ejercicios de la lista anterior (referencia por ID)
2. NO inventes ejercicios nuevos
3. Distribuye los ejercicios en ${params.trainingDays} días diferentes
4. Para cada ejercicio indica: nombre, series, repeticiones, descanso en segundos
5. Balancea los grupos musculares según el nivel y objetivos
6. Responde en formato JSON con esta estructura:
{
  "routines": [
    {
      "dayNumber": 1,
      "dayName": "Día 1",
      "focus": "Pecho y Tríceps",
      "exercises": [
        {
          "exerciseId": ID_del_ejercicio,
          "sets": número,
          "reps": "8-12",
          "restSeconds": 60,
          "notes": "opcional"
        }
      ]
    }
  ]
}

Genera la rutina AHORA en formato JSON:`,
        stream: false,
      }),
    })

    if (!response.ok) {
      throw new Error('Ollama service not available')
    }

    const data = await response.json()
    return { success: true, data: data.response, exercises }
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

// Export to PDF
ipcMain.handle('export:toPDF', async (_, workoutPlan: any) => {
  const { filePath } = await dialog.showSaveDialog({
    title: 'Guardar rutina como PDF',
    defaultPath: `rutina-${workoutPlan.userName}-${Date.now()}.pdf`,
    filters: [
      { name: 'PDF Document', extensions: ['pdf'] }
    ]
  })

  if (!filePath) return { success: false }

  try {
    // Create an HTML template with the workout plan
    const html = generateWorkoutPlanHTML(workoutPlan)

    // Create a hidden window to render the HTML
    const win = new BrowserWindow({
      show: false,
      webPreferences: {
        nodeIntegration: false
      }
    })

    await win.loadURL(`data:text/html;charset=utf-8,${encodeURIComponent(html)}`)

    // Print to PDF
    const pdfData = await win.webContents.printToPDF({
      marginsType: 1,
      pageSize: 'A4',
      printBackground: true,
      preferCSSPageSize: false,
    })

    // Write PDF to file
    const fs = await import('fs/promises')
    await fs.writeFile(filePath, pdfData)

    win.close()
    return { success: true, path: filePath }
  } catch (error: any) {
    console.error('Error exporting to PDF:', error)
    return { success: false, error: error.message }
  }
})

// Helper function to generate HTML for PDF export
function generateWorkoutPlanHTML(plan: any): string {
  return `
<!DOCTYPE html>
<html lang="es">
<head>
  <meta charset="UTF-8">
  <title>Rutina de ${plan.userName}</title>
  <style>
    body {
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      max-width: 800px;
      margin: 0 auto;
      padding: 40px 20px;
      color: #333;
      line-height: 1.6;
      background: white;
    }
    h1 {
      text-align: center;
      color: #2c3e50;
      border-bottom: 3px solid #6366f1;
      padding-bottom: 15px;
      margin-bottom: 30px;
    }
    h2 {
      color: #6366f1;
      margin-top: 30px;
      margin-bottom: 15px;
      border-left: 5px solid #6366f1;
      padding-left: 10px;
    }
    .info-section {
      background-color: #f8f9fa;
      padding: 20px;
      border-radius: 8px;
      margin-bottom: 30px;
    }
    .info-item {
      margin-bottom: 10px;
    }
    .info-label {
      font-weight: bold;
      color: #2c3e50;
    }
    .routine-day {
      margin-bottom: 40px;
      page-break-inside: avoid;
    }
    .day-title {
      background: linear-gradient(135deg, #6366f1 0%, #ec4899 100%);
      color: white;
      padding: 12px 20px;
      border-radius: 8px;
      margin-bottom: 20px;
    }
    .exercise {
      background-color: #ffffff;
      border: 1px solid #e0e0e0;
      border-radius: 8px;
      padding: 15px;
      margin-bottom: 15px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }
    .exercise-name {
      font-size: 18px;
      font-weight: bold;
      color: #2c3e50;
      margin-bottom: 8px;
    }
    .exercise-details {
      display: flex;
      gap: 20px;
      flex-wrap: wrap;
      margin-bottom: 8px;
    }
    .detail-item {
      display: flex;
      align-items: center;
      gap: 5px;
    }
    .detail-label {
      font-weight: 600;
      color: #7f8c8d;
    }
    .detail-value {
      color: #2c3e50;
    }
    .notes {
      background-color: #fff3cd;
      padding: 10px;
      border-left: 3px solid #ffc107;
      margin-top: 10px;
      font-style: italic;
    }
    .footer {
      margin-top: 50px;
      padding-top: 20px;
      border-top: 2px solid #e0e0e0;
      text-align: center;
      color: #7f8c8d;
      font-size: 14px;
    }
    @media print {
      body {
        padding: 20px;
      }
      .routine-day {
        page-break-inside: avoid;
      }
    }
  </style>
</head>
<body>
  <h1>RUTINA DE ENTRENAMIENTO PERSONALIZADA</h1>

  <div class="info-section">
    <h2>Información del Cliente</h2>
    <div class="info-item">
      <span class="info-label">Nombre:</span> ${plan.userName}
    </div>
    <div class="info-item">
      <span class="info-label">Edad:</span> ${plan.userAge || 'No especificada'}
    </div>
    <div class="info-item">
      <span class="info-label">Nivel de Fitness:</span> ${plan.fitnessLevel}
    </div>
    <div class="info-item">
      <span class="info-label">Días de entrenamiento:</span> ${plan.trainingDays}
    </div>
    <div class="info-item">
      <span class="info-label">Objetivos:</span> ${plan.goals.join(', ')}
    </div>
    <div class="info-item">
      <span class="info-label">Fecha:</span> ${new Date().toLocaleDateString('es-ES')}
    </div>
  </div>

  ${plan.routines
    .map(
      (routine: any) => `
    <div class="routine-day">
      <div class="day-title">
        <h2 style="margin: 0; color: white;">${routine.dayName}</h2>
        <div style="font-size: 14px; margin-top: 5px;">Enfoque: ${routine.focus}</div>
      </div>

      ${routine.exercises
        .map(
          (exercise: any, index: number) => `
        <div class="exercise">
          <div class="exercise-name">
            ${index + 1}. ${exercise.exercise?.spanish_name || 'Ejercicio'}
          </div>
          <div style="color: #7f8c8d; margin-bottom: 10px;">
            ${exercise.exercise?.primary_muscle_group || ''}
          </div>
          <div class="exercise-details">
            <div class="detail-item">
              <span class="detail-label">Series:</span>
              <span class="detail-value">${exercise.sets}</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Repeticiones:</span>
              <span class="detail-value">${exercise.reps}</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Descanso:</span>
              <span class="detail-value">${exercise.restSeconds}s</span>
            </div>
          </div>
          ${
            exercise.notes
              ? `<div class="notes">Nota: ${exercise.notes}</div>`
              : ''
          }
        </div>
      `
        )
        .join('')}
    </div>
  `
    )
    .join('')}

  <div class="footer">
    <p>Generado el ${new Date().toLocaleString('es-ES')}</p>
    <p style="margin-top: 20px; font-size: 12px;">
      Recuerda: Realiza un calentamiento adecuado antes de comenzar y consulta con un profesional si tienes dudas.
    </p>
  </div>
</body>
</html>
  `
}

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
