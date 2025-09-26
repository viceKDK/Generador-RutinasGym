# Convención de Mensajes de Commit

Vamos a seguir esta convención para que los commits sean claros, organizados y fáciles de entender por todo el equipo, respetando buenas prácticas.

---

## 📌 Formato general

Tipo(Alcance): descripción breve en minúscula y sin punto final

### Ejemplo:

feat(login): agregar validación de campos vacíos  
fix(rutaCritica): corregir error en el cálculo de fechas  
docs(readme): actualizar instrucciones de instalación

---

## 🧩 Tipos permitidos

| Tipo        | ¿Cuándo usarlo?                                                                 |
|-------------|----------------------------------------------------------------------------------|
| feat        | Nueva funcionalidad                                                             |
| fix         | Corrección de errores o bugs                                                    |
| docs        | Cambios en documentación (README, comentarios, etc.)                           |
| refactor    | Reorganización del código sin cambiar su comportamiento                        |
| test        | Agregado o modificación de pruebas                  |
| estructura  | Creación de carpetas base, solución inicial o arquitectura de carpetas principales |


---

## 🏷️ Alcance

Indica la parte del proyecto que modificaste. Algunos ejemplos comunes:

- login
- tareas
- rutaCritica
- readme
- proyecto
- config

---

## ✅ Reglas para escribir commits

- Usar verbos en presente: agregar, corregir, actualizar, crear, eliminar…
- Escribir en minúsculas.
- No usar punto final.
- Un commit debe reflejar un solo cambio claro (atómico).
- Evitar mensajes genéricos como “cambios varios”.

---

## 🧪 Ejemplos completos

feat(tareas): permitir mover tareas entre columnas  
fix(fecha): corregir error en fechas con dependencias  
docs(readme): agregar instrucciones para correr la app  
refactor(login): dividir lógica de validación  
test(usuario): agregar test para el login  
estructura(proyecto): crear carpetas iniciales y solución base  
config(config): agregar .editorconfig y .gitattributes para normalizar estilo y saltos de línea

---

💬 Si tenés dudas con el mensaje, mejor preguntar en el grupo antes de hacer push.
