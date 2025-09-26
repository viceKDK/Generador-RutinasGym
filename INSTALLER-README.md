# 🏋️ Generador de Rutinas de Gimnasio - Instalador

## Instalación Rápida

### Opción 1: Instalación Automática (Recomendada)
1. Haga doble clic en `build-and-install.cmd`
2. Siga las instrucciones en pantalla
3. La aplicación se instalará automáticamente
4. Busque "Generador de Rutinas de Gimnasio" en el Menú Inicio

### Opción 2: Construcción Manual
```cmd
# Ejecutar PowerShell como Administrador
powershell -ExecutionPolicy Bypass -File "scripts\build-installer.ps1" -Install
```

## Requisitos del Sistema

### Requisitos Mínimos
- **Sistema Operativo:** Windows 10 versión 1809 (17763) o superior
- **RAM:** 4 GB mínimo, 8 GB recomendado
- **Espacio en Disco:** 500 MB libres
- **.NET Runtime:** Se instala automáticamente si no está presente
- **Microsoft Word:** Para abrir documentos generados (opcional)

### Software Requerido para Desarrollo
- **.NET 9.0 SDK** - [Descargar](https://dotnet.microsoft.com/download/dotnet/9.0)
- **PowerShell 5.1 o superior** - Incluido en Windows 10/11

## Características de la Instalación

### ✅ Lo que se Instala
- Aplicación principal "Generador de Rutinas de Gimnasio"
- Acceso directo en el Menú Inicio
- Acceso directo en el Escritorio (opcional)
- Asociación de archivos .docx para rutinas
- Desinstalador automático

### 🔧 Configuración Automática
- Crea carpeta "Rutinas de Gimnasio" en Documentos
- Configura permisos de archivo apropiados
- Registra la aplicación en el sistema
- Configura integración con Windows

## Desinstalación

### Desde Windows 10/11
1. Abra **Configuración** → **Aplicaciones**
2. Busque "Generador de Rutinas de Gimnasio"
3. Haga clic en **Desinstalar**

### Desde Panel de Control
1. Abra **Panel de Control** → **Programas**
2. Busque "Generador de Rutinas de Gimnasio"
3. Haga clic en **Desinstalar**

## Solución de Problemas

### Error: "No se puede ejecutar PowerShell"
**Solución:**
```cmd
# Ejecutar como Administrador en PowerShell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Error: ".NET SDK no encontrado"
**Solución:**
1. Descargue .NET 9.0 SDK desde [Microsoft](https://dotnet.microsoft.com/download)
2. Instale y reinicie el sistema
3. Ejecute nuevamente el instalador

### Error: "Error al crear el paquete MSIX"
**Solución:**
1. Verifique que tiene permisos de administrador
2. Cierre todas las instancias de Visual Studio
3. Ejecute: `dotnet clean` antes de intentar nuevamente

### La aplicación no aparece en el Menú Inicio
**Solución:**
1. Busque manualmente "Gym" en el Menú Inicio
2. Si no aparece, reinstale la aplicación
3. Verifique en **Configuración** → **Aplicaciones**

## Archivos del Instalador

```
📁 Directorio del Proyecto
├── 📄 build-and-install.cmd          # Instalador simple para usuarios
├── 📁 scripts/
│   └── 📄 build-installer.ps1        # Script avanzado de construcción
├── 📁 src/
│   ├── 📄 Package.appxmanifest       # Manifiesto de la aplicación
│   └── 📄 GymRoutineGenerator.UI.csproj  # Configuración del proyecto
└── 📄 INSTALLER-README.md            # Este archivo
```

## Características del Instalador MSIX

### ✅ Ventajas
- **Instalación limpia** - No modifica el registro innecesariamente
- **Desinstalación completa** - Remueve todos los archivos
- **Actualizaciones automáticas** - Soporte para actualizaciones futuras
- **Sandbox de seguridad** - La aplicación se ejecuta en entorno seguro
- **Compatibilidad moderna** - Integración completa con Windows 10/11

### 📦 Contenido del Paquete
- Ejecutable principal (≈15 MB)
- Librerías .NET requeridas
- Recursos de interfaz (iconos, imágenes)
- Base de datos de ejercicios
- Plantillas de documentos Word
- Archivos de configuración

## Soporte

### 🔍 Verificación de Instalación
Después de instalar, debería poder:
1. ✅ Ver la aplicación en el Menú Inicio
2. ✅ Crear una rutina de ejemplo
3. ✅ Exportar a documento Word
4. ✅ Abrir el documento generado

### 📧 Contacto
Para problemas de instalación o soporte técnico:
- Cree un archivo de log ejecutando: `build-installer.ps1 -Verbose`
- Adjunte el log y descripción del problema

---

**Versión del Instalador:** 1.0.0
**Última actualización:** Septiembre 2024
**Compatibilidad:** Windows 10/11 (64-bit)