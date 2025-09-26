# Prompts para IA - GymRoutine Generator

---

## 🚀 PROMPT 1 - PANTALLA PRINCIPAL COMPLETA
*Usa este primero para la estructura base*

```
# GymRoutine Generator - Aplicación de Escritorio WinUI 3

## CONTEXTO DEL PROYECTO
Estás construyendo una aplicación de escritorio WinUI 3 llamada "GymRoutine Generator" que ayuda a propietarios de gimnasios y entrenadores personales a crear rutinas de ejercicio profesionales rápidamente. Los usuarios objetivo son principalmente personas de 50+ años con experiencia técnica mínima, requiriendo una interfaz "amigable para la abuela".

**Stack Tecnológico:**
- WinUI 3 (aplicación nativa de Windows)
- .NET 8 Runtime
- XAML para marcado de UI
- C# para lógica backend
- Componentes Fluent Design System
- Interfaz en idioma español

**Principios de Diseño Clave:**
- Flujo de trabajo de una sola pantalla (sin complejidad de navegación)
- Objetivos táctiles mínimos de 44px para todos los elementos interactivos
- Colores de alto contraste (mínimo 4.5:1 de ratio)
- Etiquetas grandes y claras en español
- Retroalimentación visual inmediata para todas las acciones
- Divulgación progresiva (opciones avanzadas ocultas inicialmente)

**Paleta de Colores:**
- Primary: #2E7D32 (verde fitness)
- Secondary: #1976D2 (azul confianza)
- Success: #4CAF50
- Warning: #FF9800
- Error: #F44336
- Neutral: #424242, #757575, #E0E0E0

**Tipografía:**
- Fuente: Segoe UI (nativo de Windows)
- Headers: 24-28px Semibold
- Body: 16px Regular (más grande que lo típico para usuarios 50+)
- Altura de línea mínima: 1.4

## OBJETIVO DE ALTO NIVEL
Crear el componente de pantalla principal de la aplicación que permita a los usuarios ingresar información del cliente, seleccionar preferencias de entrenamiento, generar rutinas impulsadas por IA, y exportar a documentos Word - todo dentro de una interfaz única e intuitiva optimizada para accesibilidad y facilidad de uso.

## INSTRUCCIONES DETALLADAS PASO A PASO

1. **Crear el archivo XAML principal** llamado `MainWindow.xaml` con un contenedor ScrollViewer para manejar desbordamiento de contenido en pantallas más pequeñas.

2. **Implementar la sección de encabezado** con:
   - Título grande de la app "Generador de Rutinas de Gym" (28px, Semibold, centrado)
   - Área sutil de placeholder para logo
   - Icono de engranaje de configuración (arriba-derecha, mínimo 32px)

3. **Construir la Tarjeta de Información del Cliente** con:
   - Encabezado de sección "Datos del Cliente" (24px, Semibold)
   - Selección de género: Tres botones de radio grandes "Hombre", "Mujer", "Otro" (mínimo 60px de altura)
   - Entrada de edad: Campo numérico con etiqueta "Edad" y validación (16-100 años)
   - Días de entrenamiento: Deslizador horizontal con etiquetas grandes "Días por Semana" (rango 1-7)
   - Indicador visual de completación mostrando campos llenos vs vacíos

4. **Crear la Sección de Preferencias con Pestañas** con:
   - Encabezados de pestañas: "Equipamiento", "Músculos", "Limitaciones", "Intensidad" (grandes, 48px de altura)
   - Pestaña de equipamiento: Cuadrícula de checkboxes grandes con iconos y etiquetas en español
   - Pestaña de enfoque muscular: Diagrama del cuerpo con regiones clickeables o checkboxes grandes
   - Pestaña de limitaciones: Checkboxes de limitaciones comunes más área de texto para notas personalizadas
   - Pestaña de intensidad: Tres botones de radio grandes "Principiante", "Intermedio", "Avanzado"

5. **Implementar la Sección de Generación** con:
   - Botón extra-grande "Generar Rutina" (color primario, mínimo 80px de altura, ancho completo)
   - Área de indicador de progreso (oculto por defecto, se muestra durante procesamiento de IA)
   - Área de visualización de resultados con vista previa de rutina generada
   - Sección de exportación con botón "Exportar a Word" y retroalimentación de estado

6. **Agregar atributos de accesibilidad XAML apropiados** incluyendo:
   - AutomationProperties.Name para todos los elementos interactivos
   - Estructura de encabezados apropiada usando AutomationProperties.HeadingLevel
   - Soporte para modo de alto contraste
   - Orden de navegación de teclado usando TabIndex

7. **Implementar diseño responsivo** usando contenedores Grid y StackPanel que se adapten de diseño de una columna (800px) a tres columnas (2400px+).

8. **Crear estados de carga y éxito** con animaciones apropiadas y mensajes de retroalimentación en español.

## EJEMPLOS DE CÓDIGO Y RESTRICCIONES

**Ejemplo de Botón:**
```xml
<Button x:Name="GenerateButton"
        Content="Generar Rutina"
        Background="#2E7D32"
        Foreground="White"
        FontSize="18"
        FontWeight="Semibold"
        Height="80"
        Margin="0,24,0,0"
        AutomationProperties.Name="Generar rutina de ejercicios"
        Click="GenerateButton_Click"/>
```

**Ejemplo de Campo de Entrada:**
```xml
<TextBox x:Name="AgeInput"
         PlaceholderText="Ejemplo: 35"
         Header="Edad del Cliente"
         FontSize="16"
         Height="48"
         AutomationProperties.Name="Edad del cliente en años"
         InputScope="Number"/>
```

**Restricciones:**
- TODO el texto debe estar en español
- Usar solo componentes Fluent Design (no controles personalizados a menos que se especifique)
- Mantener espaciado de cuadrícula de 8px en todo
- NO incluir elementos de navegación (aplicación de una sola pantalla)
- NO agregar animaciones complejas (respetar accesibilidad)
- Asegurar que todos los elementos interactivos sean objetivos táctiles mínimos de 44px

## DEFINICIÓN DE ALCANCE ESTRICTO

**Debes crear:**
- MainWindow.xaml (interfaz principal de la aplicación)
- MainWindow.xaml.cs (manejadores de eventos y lógica básica)
- Controles de usuario de apoyo para secciones complejas si es necesario

**NO debes modificar:**
- App.xaml (configuraciones globales de la aplicación)
- Cualquier modelo de datos existente o lógica de negocio
- Código de integración de API externa
- Funcionalidad de exportación de archivos (solo placeholder)

**Áreas de Enfoque:**
- Diseño y jerarquía visual optimizada para usuarios 50+
- Cumplimiento de accesibilidad (WCAG AA)
- Etiquetado claro en español en todo
- Patrones de divulgación progresiva
- Diseño visual de alto contraste
- Elementos interactivos grandes y fáciles de hacer clic

El código generado debe crear una interfaz profesional y accesible que una abuela podría usar sin entrenamiento mientras mantiene la eficiencia necesaria para usuarios de negocio.
```

---

## 🎯 PROMPT 2 - BOTÓN GRANDE
*Para crear botones accesibles reutilizables*

```
# Componente: Botón de Acción Grande - GymRoutine Generator

## CONTEXTO
Crear un componente de botón reutilizable optimizado para usuarios de 50+ años con experiencia técnica limitada. Debe ser parte del sistema de diseño Fluent para WinUI 3.

## OBJETIVO
Crear un UserControl llamado `LargeActionButton.xaml` que sea altamente visible, accesible, y proporcione retroalimentación clara para acciones primarias.

## ESPECIFICACIONES DETALLADAS

1. **Crear UserControl** con las siguientes propiedades dependientes:
   - `ButtonText` (string): Texto del botón en español
   - `ButtonType` (enum): Primary, Secondary, Success, Warning, Error
   - `IsLoading` (bool): Estado de carga con spinner
   - `IsEnabled` (bool): Estado habilitado/deshabilitado

2. **Implementar estados visuales:**
   - Normal: Color de fondo según ButtonType
   - Hover: Ligero oscurecimiento (10%)
   - Pressed: Escala 0.98x con animación de retorno
   - Loading: Spinner centrado con texto opcional
   - Disabled: 50% opacidad con cursor no permitido

3. **Especificaciones de tamaño:**
   - Altura mínima: 60px (80px para acciones críticas)
   - Ancho: Adaptable al contenido con mínimo 120px
   - Padding interno: 16px horizontal, 12px vertical
   - Border radius: 4px para suavidad visual

4. **Accesibilidad:**
   - AutomationProperties.Name descriptivo
   - Soporte para navegación de teclado
   - Indicador de enfoque de alto contraste (3px border)
   - Anuncio de cambios de estado para lectores de pantalla

## EJEMPLO DE USO
```xml
<local:LargeActionButton
    ButtonText="Generar Rutina"
    ButtonType="Primary"
    IsLoading="{Binding IsGenerating}"
    Click="OnGenerateClick"/>
```

## PALETA DE COLORES
- Primary: #2E7D32
- Secondary: #1976D2
- Success: #4CAF50
- Warning: #FF9800
- Error: #F44336

Crear código XAML y C# completo con todas las propiedades dependientes y estados visuales implementados.
```

---

## 📋 PROMPT 3 - TARJETA CLIENTE
*Para la sección de datos del cliente*

```
# Componente: Tarjeta de Información del Cliente - GymRoutine Generator

## CONTEXTO
Crear una tarjeta que agrupe los campos de entrada de información básica del cliente de manera visual y lógica, con validación en tiempo real y indicadores de progreso.

## OBJETIVO
Desarrollar un UserControl `ClientInfoCard.xaml` que recopile género, edad, y días de entrenamiento con validación inteligente y retroalimentación visual clara.

## ESPECIFICACIONES DETALLADAS

1. **Estructura de la tarjeta:**
   - Header con título "Datos del Cliente" e indicador de completación
   - Sección de género con RadioButtons grandes y claramente etiquetados
   - Campo de edad con validación numérica (16-100 años)
   - Selector de días de entrenamiento con controles grandes
   - Barra de progreso visual mostrando campos completados

2. **Validación en tiempo real:**
   - Edad: Validar rango 16-100, mostrar mensaje de error claro
   - Género: Requerido, resaltar si no seleccionado
   - Días: Validar rango 1-7, valores predeterminados inteligentes
   - Indicador visual de completación (verde = completo, amarillo = parcial, rojo = error)

3. **Diseño responsivo:**
   - Pantalla pequeña: Layout vertical con márgenes generosos
   - Pantalla grande: Layout de dos columnas optimizado
   - Todos los controles mantienen tamaño mínimo de 44px

4. **Propiedades expuestas:**
   - `SelectedGender` (enum): Masculino, Femenino, Otro
   - `ClientAge` (int): Edad validada del cliente
   - `TrainingDays` (int): Días por semana seleccionados
   - `IsValid` (bool): Estado de validación general
   - `CompletionPercentage` (double): Porcentaje de campos completados

## EJEMPLO DE CONTROLES
```xml
<!-- Selector de Género -->
<StackPanel Orientation="Horizontal" Spacing="16">
    <RadioButton Content="Hombre" FontSize="16" Height="48"
                 AutomationProperties.Name="Seleccionar género masculino"/>
    <RadioButton Content="Mujer" FontSize="16" Height="48"
                 AutomationProperties.Name="Seleccionar género femenino"/>
    <RadioButton Content="Otro" FontSize="16" Height="48"
                 AutomationProperties.Name="Seleccionar otro género"/>
</StackPanel>

<!-- Campo de Edad -->
<NumberBox Header="Edad del Cliente"
           PlaceholderText="Ejemplo: 35"
           Minimum="16" Maximum="100"
           FontSize="16" Height="48"
           AutomationProperties.Name="Ingresar edad del cliente"/>
```

Implementar con validación completa, estados visuales, y todas las propiedades de enlace de datos necesarias.
```

---

## 🏋️ PROMPT 4 - SELECTOR EQUIPAMIENTO
*Para elegir equipamiento disponible*

```
# Componente: Selector de Equipamiento - GymRoutine Generator

## CONTEXTO
Crear un selector visual e intuitivo para equipamiento de gimnasio disponible, optimizado para usuarios no técnicos con iconografía clara y etiquetas en español.

## OBJETIVO
Desarrollar un UserControl `EquipmentSelector.xaml` que permita selección múltiple de equipamiento con categorización inteligente y opciones de conveniencia.

## ESPECIFICACIONES DETALLADAS

1. **Categorías de equipamiento:**
   - Pesas Libres: Mancuernas, Barras, Discos
   - Máquinas: Prensa, Poleas, Multiestación
   - Peso Corporal: Sin equipamiento
   - Cardio: Cinta, Bicicleta, Elíptica
   - Accesorios: Bandas, TRX, Balones

2. **Interfaz visual:**
   - Grid layout con tarjetas grandes para cada categoría
   - Iconos de 32px mínimo con etiquetas claras en español
   - Estados: No seleccionado, Seleccionado, Hover
   - Checkboxes grandes (mínimo 24px) con áreas de click ampliadas

3. **Funcionalidad de conveniencia:**
   - Botón "Seleccionar Todo" para marcar todas las categorías
   - Botón "Limpiar Todo" para desmarcar todo
   - Presets comunes: "Gimnasio Completo", "Gimnasio Casa", "Solo Peso Corporal"
   - Indicador de cantidad seleccionada

4. **Propiedades del componente:**
   - `SelectedEquipment` (List<EquipmentType>): Equipamiento seleccionado
   - `ShowPresets` (bool): Mostrar botones de preset
   - `AllowMultipleSelection` (bool): Permitir selección múltiple
   - `EquipmentChanged` (event): Evento de cambio de selección

## EJEMPLO DE ESTRUCTURA
```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/> <!-- Header con controles -->
        <RowDefinition Height="*"/>    <!-- Grid de equipamiento -->
    </Grid.RowDefinitions>

    <!-- Header con botones de conveniencia -->
    <StackPanel Grid.Row="0" Orientation="Horizontal" Spacing="8">
        <Button Content="Seleccionar Todo" Height="40"/>
        <Button Content="Limpiar Todo" Height="40"/>
        <TextBlock Text="{Binding SelectedCount}" FontWeight="Semibold"/>
    </StackPanel>

    <!-- Grid de equipamiento -->
    <ItemsControl Grid.Row="1" ItemsSource="{Binding EquipmentCategories}">
        <ItemsControl.ItemsPanel>
            <ItemsPanelTemplate>
                <UniformGrid Columns="2" />
            </ItemsPanelTemplate>
        </ItemsControl.ItemsPanel>
    </ItemsControl>
</Grid>
```

Implementar con iconografía clara, estados visuales atractivos, y lógica de selección completa.
```

---

## ⏳ PROMPT 5 - INDICADOR PROGRESO
*Para mostrar estado de generación IA*

```
# Componente: Indicador de Progreso - GymRoutine Generator

## CONTEXTO
Crear un indicador de progreso que maneje la generación de rutinas con IA, proporcionando retroalimentación clara sobre el estado del procesamiento y tiempo estimado.

## OBJETIVO
Desarrollar un UserControl `ProgressIndicator.xaml` que muestre progreso de operaciones largas con mensajes descriptivos en español y estimaciones de tiempo realistas.

## ESPECIFICACIONES DETALLADAS

1. **Estados del indicador:**
   - Hidden: Completamente oculto cuando no hay operaciones
   - Indeterminate: Barra de progreso animada sin porcentaje específico
   - Determinate: Barra con porcentaje conocido (0-100%)
   - Success: Estado de éxito con checkmark animado
   - Error: Estado de error con mensaje descriptivo y opciones de reintento

2. **Información mostrada:**
   - Mensaje de estado principal en español claro
   - Submensaje con detalles técnicos (opcional)
   - Tiempo estimado restante
   - Botón de cancelación para operaciones largas
   - Indicador visual de modalidad (AI Local vs Modo Básico)

3. **Diseño visual:**
   - Barra de progreso de altura generosa (8px mínimo)
   - Colores que reflejan el estado (azul = procesando, verde = éxito, rojo = error)
   - Animaciones suaves que no distraigan
   - Iconografía clara para cada estado

4. **Propiedades del componente:**
   - `ProgressValue` (double): Valor de 0-100 para progreso determinado
   - `IsIndeterminate` (bool): Modo indeterminado
   - `StatusMessage` (string): Mensaje principal de estado
   - `SubMessage` (string): Mensaje secundario opcional
   - `ShowCancelButton` (bool): Mostrar opción de cancelar
   - `EstimatedTimeRemaining` (TimeSpan): Tiempo estimado restante

## EJEMPLO DE MENSAJES
```csharp
// Estados típicos en español
"Iniciando generación de rutina..."
"Procesando con IA Local (Ollama)..."
"Seleccionando ejercicios personalizados..."
"Optimizando rutina para cliente..."
"Finalizando y preparando documento..."
"¡Rutina generada exitosamente!"
"Error: IA no disponible, usando modo básico..."
```

## ESTRUCTURA XAML EJEMPLO
```xml
<Border Background="#F5F5F5" CornerRadius="8" Padding="24">
    <StackPanel Spacing="12">
        <TextBlock Text="{Binding StatusMessage}"
                   FontSize="16" FontWeight="Semibold"/>
        <ProgressBar Value="{Binding ProgressValue}"
                     IsIndeterminate="{Binding IsIndeterminate}"
                     Height="8"/>
        <TextBlock Text="{Binding SubMessage}"
                   FontSize="14" Opacity="0.7"/>
        <Button Content="Cancelar" Visibility="{Binding ShowCancel}"/>
    </StackPanel>
</Border>
```

Implementar con todas las transiciones de estado suaves y manejo de errores robusto.
```

---

## ♿ PROMPT 6 - ACCESIBILIDAD
*Para revisar y mejorar accesibilidad final*

```
# Refinamiento de Accesibilidad - GymRoutine Generator

## CONTEXTO
Revisar y mejorar todos los componentes generados para asegurar cumplimiento WCAG 2.1 AA y optimización para usuarios de 50+ años con posibles limitaciones visuales o motoras.

## OBJETIVO
Auditar y refinar la accesibilidad de todos los componentes, asegurando que la aplicación sea verdaderamente "amigable para la abuela".

## ÁREAS DE REVISIÓN CRÍTICAS

1. **Contraste de Color:**
   - Verificar ratio mínimo 4.5:1 para texto normal
   - Verificar ratio mínimo 3:1 para texto grande
   - Asegurar visibilidad en modo de alto contraste de Windows
   - Proporcionar alternativas para usuarios daltónicos

2. **Tamaños y Espaciado:**
   - Confirmar objetivos táctiles mínimos de 44px x 44px
   - Verificar espaciado de 8px entre elementos interactivos
   - Asegurar que el texto sea escalable hasta 200% sin scroll horizontal
   - Confirmar que todos los botones tengan altura mínima de 48px

3. **Navegación de Teclado:**
   - Implementar orden lógico de tabulación
   - Asegurar que todos los elementos interactivos sean accesibles por teclado
   - Proporcionar indicadores de enfoque visibles (border de 3px)
   - Implementar atajos de teclado para acciones principales

4. **Lectores de Pantalla:**
   - Agregar AutomationProperties.Name descriptivos en español
   - Implementar AutomationProperties.HelpText para elementos complejos
   - Usar AutomationProperties.HeadingLevel para estructura semántica
   - Configurar live regions para cambios dinámicos de estado

## MEJORAS ESPECÍFICAS REQUERIDAS

**Para todos los botones:**
```xml
<Button AutomationProperties.Name="Generar rutina de ejercicios para el cliente"
        AutomationProperties.HelpText="Crea una rutina personalizada basada en la información ingresada"
        ToolTipService.ToolTip="Generar Rutina"/>
```

**Para campos de entrada:**
```xml
<TextBox AutomationProperties.LabeledBy="{Binding ElementName=AgeLabel}"
         AutomationProperties.Name="Edad del cliente"
         AutomationProperties.HelpText="Ingrese la edad entre 16 y 100 años"/>
```

**Para elementos dinámicos:**
```xml
<TextBlock AutomationProperties.LiveSetting="Polite"
           Text="{Binding StatusMessage}"/>
```

## PRUEBAS REQUERIDAS

1. **Prueba con Narrador de Windows:**
   - Verificar que todos los elementos se anuncien correctamente
   - Confirmar navegación lógica con Tab/Shift+Tab
   - Probar funcionalidad completa solo con teclado

2. **Prueba de Alto Contraste:**
   - Activar modo de alto contraste en Windows
   - Verificar visibilidad de todos los elementos
   - Confirmar que los iconos permanezcan claros

3. **Prueba de Escalado:**
   - Configurar escalado de texto al 150% y 200%
   - Verificar que la interfaz permanezca funcional
   - Confirmar que no aparezca scroll horizontal

4. **Prueba con Usuarios Reales:**
   - Probar con al menos 3 usuarios de 50+ años
   - Incluir al menos 1 usuario con limitaciones visuales
   - Documentar dificultades y áreas de mejora

## IMPLEMENTACIÓN

Revisar cada componente generado y aplicar estas mejoras de accesibilidad, proporcionando código actualizado que cumpla con todos los requisitos WCAG 2.1 AA.
```

---

## 📝 ORDEN DE USO RECOMENDADO

**Con 3 usos de v0 y 3 de Lovable:**

1. **v0 #1**: PROMPT 1 (Pantalla Principal)
2. **v0 #2**: PROMPT 2 (Botón Grande)
3. **v0 #3**: PROMPT 3 (Tarjeta Cliente)

4. **Lovable #1**: PROMPT 4 (Selector Equipamiento)
5. **Lovable #2**: PROMPT 5 (Indicador Progreso)
6. **Lovable #3**: PROMPT 6 (Accesibilidad)

⚠️ **Importante**: Todo código de IA necesita revisión humana antes de usar en producción.