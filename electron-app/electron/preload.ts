import { contextBridge, ipcRenderer } from 'electron'

// Expose protected methods that allow the renderer process to use
// the ipcRenderer without exposing the entire object
contextBridge.exposeInMainWorld('electronAPI', {
  // Database operations
  db: {
    getExercises: (filter?: any) => ipcRenderer.invoke('db:getExercises', filter),
    getExercise: (id: number) => ipcRenderer.invoke('db:getExercise', id),
    saveWorkoutPlan: (plan: any) => ipcRenderer.invoke('db:saveWorkoutPlan', plan),
    saveRoutine: (workoutPlanId: number, routine: any) =>
      ipcRenderer.invoke('db:saveRoutine', workoutPlanId, routine),
    saveRoutineExercise: (routineId: number, exercise: any) =>
      ipcRenderer.invoke('db:saveRoutineExercise', routineId, exercise),
  },

  // Ollama AI integration
  ollama: {
    generateRoutine: (params: any) => ipcRenderer.invoke('ollama:generateRoutine', params),
  },

  // Export functionality
  export: {
    toWord: (workoutPlan: any) => ipcRenderer.invoke('export:toWord', workoutPlan),
    toPDF: (workoutPlan: any) => ipcRenderer.invoke('export:toPDF', workoutPlan),
  },
})

// Type definitions for TypeScript
export interface ElectronAPI {
  db: {
    getExercises: (filter?: any) => Promise<any[]>
    getExercise: (id: number) => Promise<any>
    saveWorkoutPlan: (plan: any) => Promise<number>
    saveRoutine: (workoutPlanId: number, routine: any) => Promise<number>
    saveRoutineExercise: (routineId: number, exercise: any) => Promise<number>
  }
  ollama: {
    generateRoutine: (params: any) => Promise<{ success: boolean; data?: any; error?: string }>
  }
  export: {
    toWord: (workoutPlan: any) => Promise<{ success: boolean; path?: string }>
    toPDF: (workoutPlan: any) => Promise<{ success: boolean; path?: string }>
  }
}

declare global {
  interface Window {
    electronAPI: ElectronAPI
  }
}
