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

### Instalación Normal

```bash
# Instalar dependencias
npm install

# Desarrollo
npm run electron:dev

# Build de producción
npm run electron:build
```

### ⚠️ Problemas con la Instalación de Electron

Si ves este error:
```
Error: Electron failed to install correctly, please delete node_modules/electron and try installing again
```

**Solución rápida (PowerShell/CMD en Windows):**

```powershell
# 1. Limpiar instalación corrupta
Remove-Item -Recurse -Force node_modules -ErrorAction SilentlyContinue
Remove-Item -Force package-lock.json -ErrorAction SilentlyContinue

# 2. Reinstalar con mirror alternativo
$env:ELECTRON_MIRROR="https://npmmirror.com/mirrors/electron/"
npm install
```

**O usando los scripts incluidos:**

```bash
# Reinstalar desde cero
npm run reinstall

# Verificar instalación correcta
npm run verify
```

**Para más soluciones detalladas, ver: `SOLUCION-INSTALACION-ELECTRON.md`**

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

### Desarrollo
- `npm run dev` - Inicia Vite dev server
- `npm run electron:dev` - Inicia app Electron en modo desarrollo
- `npm run type-check` - Verificación de tipos TypeScript

### Build
- `npm run build` - Build de producción completo
- `npm run electron:build` - Build y empaqueta app Electron

### Utilidades
- `npm run verify` - Verifica que Electron está instalado correctamente
- `npm run clean` - Limpia node_modules y archivos generados
- `npm run reinstall` - Reinstala dependencias desde cero

## Configuración de Ollama

1. Instalar Ollama: https://ollama.ai/
2. Descargar modelo Mistral: `ollama pull mistral`
3. Iniciar servicio: `ollama serve`
4. La app se conectará automáticamente a `http://localhost:11434`

## Licencia

MIT
