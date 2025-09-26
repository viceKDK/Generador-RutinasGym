# Project Brief: App Generación Rutinas Gimnasio

## Executive Summary

**GymRoutine Generator** es una aplicación de escritorio local que permite a propietarios de gimnasios generar rutinas de ejercicio personalizadas para sus clientes basadas en características específicas (género, edad, días de entrenamiento) y exportarlas directamente a documentos Word con imágenes y texto. La aplicación está diseñada para ser extremadamente simple de usar, permitiendo que personas sin conocimientos técnicos (como la madre del usuario) puedan operarla sin dificultad.

## Problem Statement

**Problema Actual:**
- Los entrenadores y propietarios de gimnasios crean rutinas manualmente, lo que consume mucho tiempo
- El proceso de personalización según características del cliente es repetitivo y propenso a errores
- La creación de documentos con formato profesional (Word con imágenes) requiere conocimientos técnicos
- No existe una solución local y simple que permita generar rutinas basadas en plantillas existentes

**Impacto:**
- Pérdida de tiempo valioso que podría dedicarse a atender clientes
- Inconsistencia en la calidad y formato de las rutinas
- Barrera tecnológica para usuarios no técnicos
- Necesidad de tener conexión a internet para usar herramientas online

## Proposed Solution

**GymRoutine Generator** será una aplicación de escritorio que:

- **Interfaz Ultra-Simple:** Diseño intuitivo con botones grandes y flujo claro
- **Generación Inteligente:** Algoritmo que selecciona ejercicios basado en parámetros del usuario
- **Plantillas Visuales:** Uso de PDFs/Word existentes como referencia para mantener calidad
- **Exportación Automática:** Generación directa de documentos Word con formato profesional
- **Acceso Directo:** Ejecutable en escritorio para inicio inmediato
- **Funcionalidad Offline:** Totalmente local, sin necesidad de internet

**Diferenciadores Clave:**
- Extrema simplicidad de uso para usuarios no técnicos
- Personalización basada en características reales del cliente
- Integración de imágenes de ejercicios en documentos profesionales
- Solución completamente local y privada

## Target Users

### Primary User Segment: Propietario/Entrenador de Gimnasio

**Perfil:**
- Edad: 30-60 años
- Propietario o entrenador personal de gimnasio
- Conocimientos técnicos básicos a intermedios
- Maneja 10-50 clientes regulares

**Necesidades:**
- Crear rutinas personalizadas rápidamente
- Mantener consistencia en calidad y formato
- Herramienta que no requiera aprendizaje complejo
- Solución que funcione sin internet

**Comportamiento Actual:**
- Crea rutinas manualmente en Word/papel
- Reutiliza plantillas existentes modificándolas
- Dedica 15-30 minutos por rutina

### Secondary User Segment: Familiar/Empleado No Técnico

**Perfil:**
- Edad: 50+ años
- Conocimientos técnicos mínimos
- Necesita herramientas extremadamente intuitivas
- Apoyo en tareas administrativas del gimnasio

**Necesidades:**
- Interfaz que no genere confusión
- Instrucciones claras y visibles
- Mínimos pasos para completar tareas

## Goals & Success Metrics

### Business Objectives
- Reducir tiempo de creación de rutinas de 30 min a 5 min (83% reducción)
- Aumentar número de rutinas personalizadas generadas por día en 200%
- Eliminar errores de formato en documentos generados (100% consistencia)
- Facilitar delegación de tareas a personal no técnico

### User Success Metrics
- Tiempo de aprendizaje menor a 15 minutos para usuario básico
- Satisfacción del usuario ≥ 9/10 en facilidad de uso
- 100% de rutinas generadas exportadas exitosamente a Word
- 0% de usuarios que requieren soporte técnico después del primer uso

### Key Performance Indicators (KPIs)
- **Tiempo promedio de generación de rutina**: ≤ 5 minutos
- **Tasa de adopción familiar**: 100% (madre del usuario debe usarla exitosamente)
- **Precisión en personalización**: 100% de rutinas generadas según parámetros seleccionados
- **Estabilidad de aplicación**: 0% de crashes durante operación normal

## MVP Scope

### Core Features (Must Have)
- **Interfaz de Selección Simple:** Formulario con campos para género, edad, días de entrenamiento
- **Motor de Generación:** Algoritmo que selecciona ejercicios basado en parámetros
- **Biblioteca de Ejercicios:** Base de datos local con ejercicios e imágenes
- **Exportación a Word:** Generación automática de documentos con formato profesional
- **Ejecutable de Escritorio:** Acceso directo para inicio rápido
- **Sistema de Plantillas:** Carga de PDFs/Word de referencia en carpeta específica

### Out of Scope for MVP
- Funcionalidades de base de datos de clientes
- Sincronización en la nube
- Reportes y estadísticas
- Múltiples idiomas
- Personalización avanzada de interfaz
- Sistema de usuarios/login

### MVP Success Criteria
La madre del usuario debe poder generar una rutina personalizada completa en menos de 10 minutos sin ayuda, y el documento Word resultante debe tener la misma calidad que las plantillas de referencia.

## Post-MVP Vision

### Phase 2 Features
- Base de datos de clientes con historial de rutinas
- Plantillas de rutinas temáticas (fuerza, cardio, rehabilitación)
- Función de progresión automática (rutinas que evolucionan)
- Backup automático de rutinas generadas

### Long-term Vision
Convertirse en la herramienta estándar para gimnasios pequeños y medianos que buscan profesionalizar la creación de rutinas sin complejidad técnica, expandiendo a funcionalidades de gestión integral del gimnasio.

### Expansion Opportunities
- Versión para tablets/móviles
- Integración con equipos de gimnasio inteligentes
- Marketplace de plantillas de ejercicios
- Versión multi-gimnasio para franquicias

## Technical Considerations

### Platform Requirements
- **Target Platforms:** Windows Desktop (primario)
- **OS Support:** Windows 10/11
- **Performance Requirements:** Arranque < 5 segundos, generación de rutina < 30 segundos

### Technology Preferences
- **Frontend:** Electron + React/Vue o aplicación nativa (C#/WPF)
- **Backend:** Node.js local o .NET Core
- **Database:** SQLite local para ejercicios y configuración
- **Document Generation:** Biblioteca de manipulación de Word (docx)

### Architecture Considerations
- **Repository Structure:** Monorepo con carpetas separadas para frontend, backend, y assets
- **Service Architecture:** Aplicación standalone sin dependencias externas
- **Integration Requirements:** Lectura de PDFs/Word de referencia, generación de documentos Word
- **Security/Compliance:** Datos completamente locales, sin transmisión externa

## Constraints & Assumptions

### Constraints
- **Budget:** Desarrollo interno/personal (sin presupuesto para herramientas premium)
- **Timeline:** Funcionalidad básica en 4-6 semanas
- **Resources:** Desarrollo individual, testing con usuarios familiares
- **Technical:** Debe funcionar offline, instalación simple, mínimas dependencias

### Key Assumptions
- El usuario tiene plantillas de ejercicios existentes en PDF/Word
- Windows será la plataforma principal de uso
- Los usuarios estarán dispuestos a organizar su biblioteca de ejercicios
- La calidad de las imágenes en las plantillas es suficiente
- No se requiere integración con software de gimnasio existente

## Risks & Open Questions

### Key Risks
- **Complejidad de Generación de Word:** La manipulación programática de documentos Word con imágenes puede ser técnicamente desafiante
- **Usabilidad para Usuarios No Técnicos:** El riesgo de que la interfaz no sea suficientemente simple para la madre del usuario
- **Calidad de Personalización:** El algoritmo podría no generar rutinas lo suficientemente variadas o apropiadas

### Open Questions
- ¿Qué formato específico deben tener las plantillas de referencia?
- ¿Cuántos ejercicios diferentes se necesitan en la base de datos inicial?
- ¿Se requiere validación médica o disclaimers en las rutinas generadas?
- ¿Cómo se manejarán las actualizaciones de la aplicación?

### Areas Needing Further Research
- Bibliotecas de generación de documentos Word más eficientes
- Estándares de la industria para rutinas de ejercicio
- Mejores prácticas de UX para usuarios mayores no técnicos
- Formatos de imagen óptimos para documentos Word

## Next Steps

### Immediate Actions
1. Definir estructura exacta de las plantillas de referencia
2. Crear wireframes de la interfaz ultra-simple
3. Investigar bibliotecas de generación de documentos Word
4. Establecer base de datos inicial de ejercicios
5. Crear prototipo mínimo para validar concepto con la madre del usuario

### PM Handoff
Este Project Brief proporciona el contexto completo para **GymRoutine Generator**. El siguiente paso será crear un PRD detallado que especifique cada funcionalidad, flujo de usuario, y requerimiento técnico necesario para desarrollar esta aplicación de generación de rutinas de gimnasio.