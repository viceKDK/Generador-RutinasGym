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
   * Exporta a PDF
   */
  const exportToPDF = async (plan: WorkoutPlan): Promise<boolean> => {
    setLoading(true)
    setError(null)

    try {
      const result = await window.electronAPI.export.toPDF(plan)

      if (result.success) {
        console.log('Exported to:', result.path)
        return true
      } else {
        setError('Error exporting to PDF')
        return false
      }
    } catch (err: any) {
      setError(err.message || 'Error exporting to PDF')
      console.error('Error exporting to PDF:', err)
      return false
    } finally {
      setLoading(false)
    }
  }

  /**
   * Exporta a HTML (para imprimir)
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

  return {
    loading,
    error,
    exportToWord,
    exportToPDF,
    exportToHTML,
  }
}
