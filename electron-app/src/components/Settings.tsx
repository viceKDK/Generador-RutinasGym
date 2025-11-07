import { useState } from 'react'
import { IconSettings, IconDatabase, IconBrain, IconInfoCircle } from '@tabler/icons-react'

export default function Settings() {
  const [ollamaUrl, setOllamaUrl] = useState('http://localhost:11434')
  const [ollamaModel, setOllamaModel] = useState('mistral')
  const [testingOllama, setTestingOllama] = useState(false)
  const [ollamaStatus, setOllamaStatus] = useState<'unknown' | 'connected' | 'error'>('unknown')

  const testOllamaConnection = async () => {
    setTestingOllama(true)
    try {
      const response = await fetch(`${ollamaUrl}/api/tags`)
      if (response.ok) {
        setOllamaStatus('connected')
      } else {
        setOllamaStatus('error')
      }
    } catch (error) {
      setOllamaStatus('error')
    } finally {
      setTestingOllama(false)
    }
  }

  return (
    <div className="max-w-4xl mx-auto animate-fade-in">
      <h1 className="text-3xl font-bold mb-8">Configuración</h1>

      <div className="space-y-6">
        {/* Ollama Settings */}
        <div className="card">
          <div className="flex items-center gap-3 mb-6">
            <IconBrain size={32} className="text-primary" />
            <h2 className="text-2xl font-bold">Configuración de IA (Ollama)</h2>
          </div>

          <div className="space-y-4">
            <div>
              <label className="label">URL de Ollama</label>
              <input
                type="text"
                className="input"
                value={ollamaUrl}
                onChange={(e) => setOllamaUrl(e.target.value)}
                placeholder="http://localhost:11434"
              />
              <p className="text-sm text-text-muted mt-2">
                URL donde está corriendo el servicio de Ollama
              </p>
            </div>

            <div>
              <label className="label">Modelo</label>
              <select
                className="input"
                value={ollamaModel}
                onChange={(e) => setOllamaModel(e.target.value)}
              >
                <option value="mistral">Mistral</option>
                <option value="llama2">Llama 2</option>
                <option value="codellama">Code Llama</option>
                <option value="neural-chat">Neural Chat</option>
              </select>
            </div>

            <div className="flex items-center gap-4">
              <button
                className="btn-primary"
                onClick={testOllamaConnection}
                disabled={testingOllama}
              >
                {testingOllama ? 'Probando...' : 'Probar Conexión'}
              </button>

              {ollamaStatus === 'connected' && (
                <span className="text-green-400 flex items-center gap-2">
                  <span className="w-2 h-2 bg-green-400 rounded-full"></span>
                  Conectado
                </span>
              )}

              {ollamaStatus === 'error' && (
                <span className="text-red-400 flex items-center gap-2">
                  <span className="w-2 h-2 bg-red-400 rounded-full"></span>
                  Error de conexión
                </span>
              )}
            </div>
          </div>
        </div>

        {/* Database Settings */}
        <div className="card">
          <div className="flex items-center gap-3 mb-6">
            <IconDatabase size={32} className="text-primary" />
            <h2 className="text-2xl font-bold">Base de Datos</h2>
          </div>

          <div className="space-y-4">
            <div className="bg-surface-light p-4 rounded-lg">
              <p className="text-sm text-text-muted">
                La base de datos se encuentra en la carpeta de datos de la aplicación
              </p>
              <p className="text-sm text-text-muted mt-2">
                Ruta: <code className="text-primary">~/AppData/Roaming/gym-routine-generator/gymroutine.db</code>
              </p>
            </div>

            <button className="btn-outline">
              Exportar Base de Datos
            </button>
          </div>
        </div>

        {/* About */}
        <div className="card">
          <div className="flex items-center gap-3 mb-6">
            <IconInfoCircle size={32} className="text-primary" />
            <h2 className="text-2xl font-bold">Acerca de</h2>
          </div>

          <div className="space-y-2 text-text-muted">
            <p><strong className="text-white">Versión:</strong> 1.0.0</p>
            <p><strong className="text-white">Autor:</strong> Gym Routine Generator</p>
            <p><strong className="text-white">Licencia:</strong> MIT</p>
            <p className="mt-4">
              Aplicación de escritorio para generar rutinas de gimnasio personalizadas
              utilizando inteligencia artificial local.
            </p>
          </div>
        </div>
      </div>
    </div>
  )
}
