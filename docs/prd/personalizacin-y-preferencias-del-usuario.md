# Personalización y Preferencias del Usuario

## Functional Requirements - Personalización

11. **FR11:** El sistema debe permitir seleccionar preferencias de equipamiento (Pesas libres, Máquinas, Peso corporal, Combinado)

12. **FR12:** El sistema debe permitir especificar áreas de enfoque muscular (Abdominales, Brazos, Piernas, Espalda, Pecho, Glúteos, Cuerpo completo)

13. **FR13:** La aplicación debe incluir opciones de intensidad (Principiante, Intermedio, Avanzado) que afecten la selección de ejercicios

14. **FR14:** El sistema debe permitir especificar limitaciones físicas o ejercicios a evitar mediante checkboxes o lista de exclusiones

15. **FR15:** La aplicación debe generar rutinas que consideren el número específico de días por semana (1-7) ajustando la distribución muscular accordingly

16. **FR16:** El sistema debe incluir una base de datos completa de ejercicios con imágenes de alta calidad para cada movimiento

17. **FR17:** La aplicación debe permitir cargar y modificar plantillas base que se adapten según las preferencias seleccionadas

18. **FR18:** El sistema debe integrar Ollama con modelo Mistral 7B local para generar variaciones inteligentes de rutinas basadas en las preferencias

## AI-Powered Personalization Engine (Local)

19. **FR19:** El sistema debe ejecutar Ollama localmente con modelo Mistral 7B para generar rutinas personalizadas inteligentes sin conexión a internet

20. **FR20:** El motor de AI debe considerar todos los parámetros (edad, género, días, preferencias, limitaciones) para crear rutinas óptimas

21. **FR21:** La aplicación debe mantener plantillas base que el AI pueda modificar dinámicamente según necesidades específicas

22. **FR22:** El sistema debe generar explicaciones breves de por qué se seleccionaron ciertos ejercicios para cada usuario

## Database and Template Requirements

23. **FR23:** La aplicación debe incluir una biblioteca de al menos 200+ ejercicios con imágenes profesionales

24. **FR24:** Cada ejercicio debe tener metadatos (grupo muscular, equipamiento, dificultad, variaciones)

25. **FR25:** El sistema debe permitir agregar nuevos ejercicios e imágenes a la base de datos local

26. **FR26:** La aplicación debe incluir plantillas base modificables para diferentes tipos de rutinas (Fuerza, Cardio, Híbrido)

27. **FR27:** La aplicación debe estar completamente en idioma español, incluyendo interfaz, mensajes de error, y contenido generado por AI

28. **FR28:** El modelo Mistral 7B debe recibir prompts en español y generar respuestas en español para rutinas de ejercicio

## Non-Functional Requirements - Arquitectura Local AI

9. **NFR9:** El sistema debe funcionar completamente offline una vez instalado Ollama y descargado el modelo Mistral 7B

10. **NFR10:** La aplicación debe instalar automáticamente Ollama, descargar Mistral 7B, y cachear localmente la base de ejercicios e imágenes

11. **NFR11:** El sistema debe mostrar claramente si está funcionando en modo "AI Local" (Ollama activo) o "Modo Básico" (algoritmo simple)

12. **NFR12:** La aplicación debe requerir mínimo 8GB RAM y recomendar 16GB para óptimo rendimiento del modelo Mistral 7B

13. **NFR13:** El sistema debe manejar la instalación de Ollama automáticamente o proporcionar instrucciones claras si falla la instalación automática
