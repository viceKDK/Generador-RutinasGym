import { useState } from 'react'
import { IconDatabase, IconBrain } from '@tabler/icons-react'

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
    <div className="max-w-4xl mx-auto animate-fade-in-up">
      <div className="mb-8 pb-2 text-center">
        <h1 className="text-5xl font-bold text-text mb-3 pb-1 leading-tight">Configuración</h1>
      </div>

      <div className="space-y-8">
        {/* Ollama Settings */}
        <div className="card animate-scale-in hover-lift">
          <div className="flex items-center gap-4 mb-8">
            <div className="p-3 bg-gradient-to-br from-secondary to-secondary-dark rounded-xl shadow-glow-violet">
              <IconBrain size={36} className="text-white" />
            </div>
            <div>
              <h2 className="text-2xl font-bold text-text">Configuración de IA</h2>
              <p className="text-text-secondary text-sm">Configuración del servicio Ollama</p>
            </div>
          </div>

          <div className="space-y-6">
            <div>
              <label className="label">URL de Ollama</label>
              <input
                type="text"
                className="input"
                value={ollamaUrl}
                onChange={(e) => setOllamaUrl(e.target.value)}
                placeholder="http://localhost:11434"
              />
              <p className="text-sm text-text-muted mt-2 flex items-center gap-2">
                <span className="inline-block w-1.5 h-1.5 bg-primary rounded-full"></span>
                URL donde está corriendo el servicio de Ollama
              </p>
            </div>

            <div>
              <label className="label">Modelo de IA</label>
              <select
                className="input cursor-pointer"
                value={ollamaModel}
                onChange={(e) => setOllamaModel(e.target.value)}
              >
                <option value="mistral">Mistral - Recomendado</option>
                <option value="llama2">Llama 2</option>
                <option value="codellama">Code Llama</option>
                <option value="neural-chat">Neural Chat</option>
              </select>
            </div>

            <div className="flex flex-col sm:flex-row items-start sm:items-center gap-4 pt-4">
              <button
                className="group relative px-6 py-3 bg-gradient-to-r from-secondary via-secondary-dark to-secondary-light text-white font-semibold rounded-xl shadow-lg hover:shadow-glow-violet transition-all duration-300 hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100 overflow-hidden"
                onClick={testOllamaConnection}
                disabled={testingOllama}
              >
                <div className="absolute inset-0 bg-gradient-to-r from-transparent via-white/20 to-transparent -translate-x-full group-hover:translate-x-full transition-transform duration-700"></div>
                {testingOllama ? (
                  <span className="flex items-center gap-2 relative z-10">
                    <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
                    Probando...
                  </span>
                ) : (
                  <span className="relative z-10">Probar Conexión</span>
                )}
              </button>

              {ollamaStatus === 'connected' && (
                <span className="text-success flex items-center gap-2 font-semibold animate-fade-in">
                  <span className="w-3 h-3 bg-success rounded-full animate-pulse-slow shadow-lg shadow-success/50"></span>
                  Conectado correctamente
                </span>
              )}

              {ollamaStatus === 'error' && (
                <span className="text-error flex items-center gap-2 font-semibold animate-fade-in">
                  <span className="w-3 h-3 bg-error rounded-full animate-pulse shadow-lg shadow-error/50"></span>
                  Error de conexión
                </span>
              )}
            </div>
          </div>
        </div>

        {/* Database Settings */}
        <div className="card animate-scale-in hover-lift" style={{ animationDelay: '0.1s' }}>
          <div className="flex items-center gap-4 mb-8">
            <div className="p-3 bg-gradient-to-br from-secondary to-secondary-dark rounded-xl shadow-glow-violet">
              <IconDatabase size={36} className="text-white" />
            </div>
            <div>
              <h2 className="text-2xl font-bold text-text">Base de Datos</h2>
              <p className="text-text-secondary text-sm">Gestión y exportación de datos</p>
            </div>
          </div>

          <div className="space-y-6">
            <div className="glass-effect p-6 rounded-xl border border-border-gold">
              <p className="text-text-secondary mb-3 font-medium">
                📍 Ubicación de la base de datos
              </p>
              <div className="bg-background/50 p-4 rounded-lg border border-border">
                <code className="text-primary font-mono text-sm break-all">
                  ~/AppData/Roaming/gym-routine-generator/gymroutine.db
                </code>
              </div>
            </div>

            <button className="group relative px-6 py-3 bg-gradient-to-r from-secondary via-secondary-dark to-secondary-light text-white font-semibold rounded-xl shadow-lg hover:shadow-glow-violet transition-all duration-300 hover:scale-105 overflow-hidden w-full sm:w-auto">
              <div className="absolute inset-0 bg-gradient-to-r from-transparent via-white/20 to-transparent -translate-x-full group-hover:translate-x-full transition-transform duration-700"></div>
              <span className="relative z-10">Exportar Base de Datos</span>
            </button>
          </div>
        </div>

      </div>
    </div>
  )
}
