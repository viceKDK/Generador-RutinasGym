# Solución al Error de Instalación de Electron

## Problema
```
Error: Electron failed to install correctly, please delete node_modules/electron and try installing again
```

## Causa
El binario de Electron no se descargó correctamente durante la instalación de dependencias, usualmente por:
- Problemas de red/firewall
- Error 403 (Forbidden) al descargar desde los servidores de Electron
- Conexión interrumpida

## Solución Paso a Paso

### Opción 1: Reinstalación Completa (Recomendado)

Abre PowerShell o CMD en la carpeta `electron-app` y ejecuta:

```powershell
# 1. Eliminar instalación corrupta
Remove-Item -Recurse -Force node_modules
Remove-Item -Force package-lock.json

# 2. Limpiar cache de npm
npm cache clean --force

# 3. Configurar mirror alternativo de Electron (opcional pero recomendado)
npm config set electron_mirror https://npmmirror.com/mirrors/electron/

# 4. Reinstalar todas las dependencias
npm install

# 5. Si el paso 4 falla, usar mirror alternativo directamente
$env:ELECTRON_MIRROR="https://npmmirror.com/mirrors/electron/"
npm install
```

### Opción 2: Instalación Sin Scripts (Si Opción 1 falla)

```powershell
# 1. Limpiar
Remove-Item -Recurse -Force node_modules -ErrorAction SilentlyContinue
Remove-Item -Force package-lock.json -ErrorAction SilentlyContinue

# 2. Instalar sin ejecutar scripts post-install
$env:ELECTRON_MIRROR="https://npmmirror.com/mirrors/electron/"
npm install --ignore-scripts

# 3. Instalar Electron manualmente
cd node_modules\electron
node install.js
cd ..\..

# 4. Si falla el paso 3, probar con:
$env:ELECTRON_MIRROR="https://github.com/electron/electron/releases/download/"
cd node_modules\electron
node install.js
cd ..\..
```

### Opción 3: Usar Archivo .npmrc (Configuración Permanente)

1. Crea un archivo `.npmrc` en la carpeta `electron-app` con este contenido:

```
electron_mirror=https://npmmirror.com/mirrors/electron/
registry=https://registry.npmjs.org/
```

2. Luego ejecuta:

```powershell
Remove-Item -Recurse -Force node_modules -ErrorAction SilentlyContinue
Remove-Item -Force package-lock.json -ErrorAction SilentlyContinue
npm install
```

### Opción 4: Actualizar a Electron Más Reciente

Si todas las opciones anteriores fallan, actualiza Electron a la versión más reciente:

```powershell
# 1. Actualizar package.json manualmente o con:
npm install --save-dev electron@latest

# 2. Reinstalar
npm install
```

## Verificar Instalación Correcta

Después de instalar correctamente, verifica que Electron funciona:

```powershell
# Verificar que el binario existe
node -e "console.log(require('electron'))"

# Debería mostrar la ruta del ejecutable de Electron, algo como:
# C:\Users\...\electron-app\node_modules\electron\dist\electron.exe
```

## Ejecutar la Aplicación

Una vez instalado correctamente:

```powershell
# Modo desarrollo
npm run dev

# O modo electron específico
npm run electron:dev
```

## Problemas Comunes

### "Cannot find module 'electron'"
- Ejecuta: `npm install electron --save-dev`

### "Error 403 Forbidden" persiste
- Verifica tu conexión a Internet
- Desactiva temporalmente antivirus/firewall
- Usa un VPN si tu región bloquea descargas de Electron
- Intenta desde otra red (ej: datos móviles)

### "EACCES: permission denied"
- Ejecuta PowerShell/CMD como Administrador
- O cambia los permisos de la carpeta

### Instalación muy lenta
- Es normal, Electron es un paquete grande (~200MB)
- Ten paciencia, puede tardar 5-10 minutos dependiendo de tu conexión

## Solución Rápida (Si todo falla)

Si ninguna opción funciona, descarga Electron manualmente:

1. Ve a: https://github.com/electron/electron/releases
2. Descarga `electron-v28.0.0-win32-x64.zip` (o la versión que necesites)
3. Extrae en `electron-app/node_modules/electron/dist/`
4. Crea el archivo `electron-app/node_modules/electron/path.txt` con el contenido: `electron.exe`

## Notas Adicionales

- **Archivo .npmrc creado**: Ya creé un archivo `.npmrc` en el repositorio con configuraciones optimizadas
- **Version actual**: Electron 28.0.0
- **Tamaño**: ~200-300MB de descarga total
- **Tiempo estimado**: 5-15 minutos dependiendo de la conexión

## Scripts Disponibles Después de Instalar

```json
"dev": "vite"                          // Solo frontend con Vite
"electron:dev": "..."                   // Electron + Vite en desarrollo
"build": "tsc && vite build && electron-builder"  // Build completo
"electron:build": "vite build && electron-builder"  // Build para producción
```

## Ayuda Adicional

Si después de seguir todos estos pasos aún tienes problemas:

1. Revisa los logs completos: `C:\Users\TU_USUARIO\AppData\Roaming\npm-cache\_logs\`
2. Busca el archivo más reciente y revisa el error específico
3. Comparte el error específico para ayuda más detallada

---

**Última actualización**: 2025-11-07
**Versión de Electron**: 28.0.0
**Node.js requerido**: >= 18.x
