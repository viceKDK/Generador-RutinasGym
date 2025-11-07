# Generador de Rutinas de Gimnasio - Electron

Aplicación de escritorio multiplataforma para generar rutinas de gimnasio personalizadas con inteligencia artificial.

## Características

- Interfaz moderna y responsive con React + TypeScript
- Integración con Ollama para generación de rutinas con IA
- Base de datos SQLite local para almacenar ejercicios y rutinas
- Exportación a Word y PDF
- Biblioteca completa de ejercicios con filtros
- Multiplataforma (Windows, macOS, Linux)

## Tecnologías

- **Frontend**: React 18, TypeScript, TailwindCSS
- **Desktop**: Electron
- **Build**: Vite
- **Database**: better-sqlite3
- **Icons**: Tabler Icons
- **Export**: docx library
- **AI**: Ollama (local)

## Instalación

```bash
# Instalar dependencias
npm install

# Desarrollo
npm run electron:dev

# Build de producción
npm run electron:build
```

## Requisitos

- Node.js 18+
- Ollama instalado localmente (para funcionalidad de IA)

## Estructura del Proyecto

```
electron-app/
├── electron/          # Procesos de Electron
│   ├── main.ts       # Proceso principal
│   └── preload.ts    # Script de preload
├── src/              # Código React
│   ├── components/   # Componentes UI
│   ├── models/       # TypeScript types
│   ├── services/     # Lógica de negocio
│   ├── hooks/        # Custom hooks
│   ├── utils/        # Utilidades
│   └── styles/       # Estilos globales
├── public/           # Assets públicos
└── dist/             # Build output
```

## Scripts Disponibles

- `npm run dev` - Inicia Vite dev server
- `npm run electron:dev` - Inicia app Electron en modo desarrollo
- `npm run build` - Build de producción
- `npm run electron:build` - Build y empaqueta app Electron
- `npm run type-check` - Verificación de tipos TypeScript

## Configuración de Ollama

1. Instalar Ollama: https://ollama.ai/
2. Descargar modelo Mistral: `ollama pull mistral`
3. Iniciar servicio: `ollama serve`
4. La app se conectará automáticamente a `http://localhost:11434`

## Licencia

MIT
