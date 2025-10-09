# 🔗 Crear Acceso Directo en el Escritorio

## ✅ Acceso Directo Ya Creado

El acceso directo **"Rutina Gym"** ya fue creado en tu escritorio.

**Ubicación**: `C:\Users\vicen\OneDrive\Escritorio\Rutina Gym.lnk`

---

## 🚀 Cómo Usar el Acceso Directo

1. **Hacer doble clic** en el icono "Rutina Gym" en el escritorio
2. La aplicación se abrirá automáticamente
3. ¡Listo para generar rutinas!

---

## 🔄 Si Necesitas Recrear el Acceso Directo

### Opción 1: Script PowerShell (Automático)

```bash
powershell -ExecutionPolicy Bypass -File crear_acceso_directo_mejorado.ps1
```

Este script:
- ✅ Elimina el acceso directo anterior (si existe)
- ✅ Crea uno nuevo con icono personalizado
- ✅ Configura la ruta correcta al ejecutable

---

### Opción 2: Manual

1. **Navega a la carpeta del ejecutable:**
   ```
   src\app-ui\bin\x64\Debug\net8.0-windows\
   ```

2. **Encuentra el archivo:**
   ```
   GeneradorRutinasGimnasio.exe
   ```

3. **Clic derecho** en `GeneradorRutinasGimnasio.exe`

4. **Enviar a → Escritorio (crear acceso directo)**

5. **Renombrar** el acceso directo a "Rutina Gym"

6. **(Opcional) Cambiar icono:**
   - Clic derecho en el acceso directo → Propiedades
   - Botón "Cambiar icono..."
   - Buscar: `gym_icon.ico` en la raíz del proyecto

---

## 📁 Archivos Relacionados

### Scripts Disponibles:

| Archivo | Descripción |
|---------|-------------|
| `ejecutar_rutina_gym.vbs` | Script VBS para ejecutar desde Debug |
| `ejecutar_rutina_gym_release.vbs` | Script VBS inteligente (prueba Release, luego Debug) |
| `crear_acceso_directo_mejorado.ps1` | Script PowerShell para crear acceso directo |
| `gym_icon.ico` | Icono de la aplicación |

---

## 🔧 Solución de Problemas

### El acceso directo no funciona

**Síntoma**: Al hacer doble clic, no pasa nada o sale error

**Solución**:

1. **Verifica que el ejecutable existe:**
   ```bash
   ls src/app-ui/bin/x64/Debug/net8.0-windows/GeneradorRutinasGimnasio.exe
   ```

2. **Si no existe, compila la aplicación:**
   ```bash
   cd src/app-ui
   dotnet build -c Debug
   ```

3. **Recrea el acceso directo:**
   ```bash
   powershell -ExecutionPolicy Bypass -File crear_acceso_directo_mejorado.ps1
   ```

---

### El icono no aparece

**Síntoma**: El acceso directo tiene icono genérico en lugar del icono personalizado

**Solución**:

1. **Verifica que existe `gym_icon.ico`:**
   ```bash
   ls gym_icon.ico
   ```

2. **Refresca el cache de iconos de Windows:**

   **Opción A - Reiniciar Explorer:**
   ```bash
   powershell "Stop-Process -Name explorer -Force; Start-Process explorer"
   ```

   **Opción B - Script incluido:**
   ```bash
   refrescar_iconos.bat
   ```

3. **O simplemente reinicia tu computadora**

---

### El acceso directo apunta a la versión Debug

**Síntoma**: Quieres usar la versión Release (más rápida)

**Solución**:

1. **Compila versión Release:**
   ```bash
   cd src/app-ui
   dotnet build -c Release
   ```

2. **Usa el script VBS inteligente:**
   - Edita `crear_acceso_directo_mejorado.ps1`
   - Cambia la línea del target:
     ```powershell
     $scriptPath = "$projectDir\ejecutar_rutina_gym_release.vbs"
     ```

3. **Recrea el acceso directo:**
   ```bash
   powershell -ExecutionPolicy Bypass -File crear_acceso_directo_mejorado.ps1
   ```

---

## 🎨 Personalizar el Icono

Si quieres cambiar el icono del acceso directo:

### Usando otro archivo .ico

1. Coloca tu archivo `.ico` en la raíz del proyecto
2. Edita `crear_acceso_directo_mejorado.ps1`:
   ```powershell
   $iconPath = "$projectDir\tu_icono.ico"
   ```
3. Ejecuta el script

### Usando icono del ejecutable

1. Edita `crear_acceso_directo_mejorado.ps1`:
   ```powershell
   $iconPath = "$exePath,0"  # Usa el icono del .exe
   ```
2. Ejecuta el script

---

## 📝 Notas Técnicas

### Script VBS vs Acceso Directo .exe

**Por qué usar VBS:**
- ✅ No muestra ventana de consola negra
- ✅ Inicio más limpio y profesional
- ✅ Puede manejar rutas relativas dinámicamente

**Alternativa directa al .exe:**
- ⚠️ Muestra ventana de consola brevemente
- ✅ Más simple
- ✅ Más rápido (microsegundos de diferencia)

### Rutas Usadas

El script VBS busca el ejecutable en:
```
<Directorio del Script>\src\app-ui\bin\x64\Debug\net8.0-windows\GeneradorRutinasGimnasio.exe
```

Si moviste el proyecto a otra ubicación, el acceso directo seguirá funcionando porque usa rutas relativas.

---

## ✨ Resultado Final

Deberías tener en tu escritorio:

```
📁 Escritorio
├── 🏋️ Rutina Gym.lnk  ← Acceso directo con icono personalizado
└── ... (otros archivos)
```

Al hacer **doble clic** en "Rutina Gym":
1. Se ejecuta el script VBS
2. El script busca el ejecutable
3. La aplicación se abre sin consola
4. ¡Listo para usar!

---

**🎯 Disfruta de tu aplicación con acceso directo personalizado!**
