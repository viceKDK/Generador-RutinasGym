import { contextBridge, ipcRenderer } from 'electron'

// Expose protected methods that allow the renderer process to use
// the ipcRenderer without exposing the entire object
contextBridge.exposeInMainWorld('electronAPI', {
  // Database operations
  db: {
    getExercises: (filter?: any) => ipcRenderer.invoke('db:getExercises', filter),
    getExercise: (id: number) => ipcRenderer.invoke('db:getExercise', id),
    createExercise: (exercise: any) => ipcRenderer.invoke('db:createExercise', exercise),
    updateExercise: (id: number, exercise: any) => ipcRenderer.invoke('db:updateExercise', id, exercise),
    deleteExercise: (id: number) => ipcRenderer.invoke('db:deleteExercise', id),
    getExerciseImages: (exerciseId: number) => ipcRenderer.invoke('db:getExerciseImages', exerciseId),
    saveExerciseImage: (exerciseId: number, imagePath: string, isPrimary?: boolean) =>
      ipcRenderer.invoke('db:saveExerciseImage', exerciseId, imagePath, isPrimary),
    deleteExerciseImage: (imageId: number) => ipcRenderer.invoke('db:deleteExerciseImage', imageId),
    saveWorkoutPlan: (plan: any) => ipcRenderer.invoke('db:saveWorkoutPlan', plan),
    saveRoutine: (workoutPlanId: number, routine: any) =>
      ipcRenderer.invoke('db:saveRoutine', workoutPlanId, routine),
    saveRoutineExercise: (routineId: number, exercise: any) =>
      ipcRenderer.invoke('db:saveRoutineExercise', routineId, exercise),
  },

  // File operations
  file: {
    uploadImage: (imageData: string, exerciseId: number) =>
      ipcRenderer.invoke('file:uploadImage', imageData, exerciseId),
    uploadVideo: (videoData: string, exerciseId: number) =>
      ipcRenderer.invoke('file:uploadVideo', videoData, exerciseId),
    getPath: (relativePath: string) => ipcRenderer.invoke('file:getPath', relativePath),
    openFolder: (relativePath: string) => ipcRenderer.invoke('file:openFolder', relativePath),
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
    createExercise: (exercise: any) => Promise<number>
    updateExercise: (id: number, exercise: any) => Promise<boolean>
    deleteExercise: (id: number) => Promise<boolean>
    getExerciseImages: (exerciseId: number) => Promise<any[]>
    saveExerciseImage: (exerciseId: number, imagePath: string, isPrimary?: boolean) => Promise<number>
    deleteExerciseImage: (imageId: number) => Promise<boolean>
    saveWorkoutPlan: (plan: any) => Promise<number>
    saveRoutine: (workoutPlanId: number, routine: any) => Promise<number>
    saveRoutineExercise: (routineId: number, exercise: any) => Promise<number>
  }
  file: {
    uploadImage: (imageData: string, exerciseId: number) => Promise<string | null>
    uploadVideo: (videoData: string, exerciseId: number) => Promise<string | null>
    getPath: (relativePath: string) => Promise<string>
    openFolder: (relativePath: string) => Promise<{ success: boolean; error?: string }>
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
