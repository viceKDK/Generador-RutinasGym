import { useState } from 'react'
import type { WorkoutPlan } from '../models/types'
import { ExportService } from '../services/ExportService'

export function useExport() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const exportService = new ExportService()

  /**
   * Exporta a Word
   */
  const exportToWord = async (plan: WorkoutPlan): Promise<boolean> => {
    setLoading(true)
    setError(null)

    try {
      const result = await window.electronAPI.export.toWord(plan)

      if (result.success) {
        console.log('Exported to:', result.path)
        return true
      } else {
        setError('Error exporting to Word')
        return false
      }
    } catch (err: any) {
      setError(err.message || 'Error exporting')
      console.error('Error exporting:', err)
      return false
    } finally {
      setLoading(false)
    }
  }

  /**
   * Exporta a HTML (para imprimir o PDF)
   */
  const exportToHTML = async (plan: WorkoutPlan): Promise<string | null> => {
    setLoading(true)
    setError(null)

    try {
      const html = await exportService.exportToHTML(plan)
      return html
    } catch (err: any) {
      setError(err.message || 'Error exporting to HTML')
      console.error('Error exporting to HTML:', err)
      return null
    } finally {
      setLoading(false)
    }
  }

  /**
   * Descarga HTML como archivo
   */
  const downloadHTML = async (plan: WorkoutPlan): Promise<void> => {
    const html = await exportToHTML(plan)

    if (html) {
      const blob = new Blob([html], { type: 'text/html' })
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `rutina-${plan.userName}-${Date.now()}.html`
      a.click()
      URL.revokeObjectURL(url)
    }
  }

  return {
    loading,
    error,
    exportToWord,
    exportToHTML,
    downloadHTML,
  }
}
