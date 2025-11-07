import { Document, Packer, Paragraph, TextRun, Table, TableRow, TableCell, HeadingLevel, AlignmentType, WidthType, BorderStyle, ImageRun } from 'docx'
import type { WorkoutPlan } from '../models/types'
import { format } from 'date-fns'

export class ExportService {
  /**
   * Exporta una rutina a formato Word
   */
  async exportToWord(plan: WorkoutPlan): Promise<Blob> {
    const doc = new Document({
      sections: [
        {
          properties: {},
          children: [
            // Encabezado principal
            new Paragraph({
              text: 'RUTINA DE ENTRENAMIENTO PERSONALIZADA',
              heading: HeadingLevel.HEADING_1,
              alignment: AlignmentType.CENTER,
              spacing: { after: 400 },
            }),

            // Información del cliente
            new Paragraph({
              text: 'Información del Cliente',
              heading: HeadingLevel.HEADING_2,
              spacing: { before: 200, after: 200 },
            }),

            this.createInfoParagraph('Nombre', plan.userName),
            this.createInfoParagraph('Edad', plan.userAge?.toString() || 'No especificada'),
            this.createInfoParagraph('Nivel de Fitness', plan.fitnessLevel),
            this.createInfoParagraph('Días de entrenamiento', plan.trainingDays.toString()),
            this.createInfoParagraph('Objetivos', plan.goals.join(', ')),

            new Paragraph({
              text: `Fecha de generación: ${format(new Date(), 'dd/MM/yyyy')}`,
              spacing: { before: 100, after: 400 },
            }),

            // Rutinas por día
            ...this.createRoutineSections(plan),

            // Notas finales
            new Paragraph({
              text: 'Notas Importantes',
              heading: HeadingLevel.HEADING_2,
              spacing: { before: 400, after: 200 },
            }),

            new Paragraph({
              children: [
                new TextRun({
                  text: '• Realiza un calentamiento de 5-10 minutos antes de comenzar\n',
                }),
                new TextRun({
                  text: '• Mantén una técnica correcta en todos los ejercicios\n',
                }),
                new TextRun({
                  text: '• Ajusta el peso según tu capacidad\n',
                }),
                new TextRun({
                  text: '• Descansa lo necesario entre series\n',
                }),
                new TextRun({
                  text: '• Consulta con un profesional si tienes dudas\n',
                }),
              ],
            }),
          ],
        },
      ],
    })

    return await Packer.toBlob(doc)
  }

  /**
   * Crea un párrafo de información
   */
  private createInfoParagraph(label: string, value: string): Paragraph {
    return new Paragraph({
      children: [
        new TextRun({
          text: `${label}: `,
          bold: true,
        }),
        new TextRun({
          text: value,
        }),
      ],
      spacing: { after: 100 },
    })
  }

  /**
   * Crea las secciones de rutinas
   */
  private createRoutineSections(plan: WorkoutPlan): Paragraph[] {
    const sections: Paragraph[] = []

    for (const routine of plan.routines) {
      // Título del día
      sections.push(
        new Paragraph({
          text: routine.dayName,
          heading: HeadingLevel.HEADING_2,
          spacing: { before: 400, after: 200 },
        })
      )

      // Enfoque
      sections.push(
        new Paragraph({
          children: [
            new TextRun({
              text: 'Enfoque: ',
              bold: true,
            }),
            new TextRun({
              text: routine.focus,
            }),
          ],
          spacing: { after: 200 },
        })
      )

      // Tabla de ejercicios
      sections.push(
        new Paragraph({
          text: '', // Spacer
          spacing: { after: 100 },
        })
      )

      // Ejercicios
      for (const exercise of routine.exercises) {
        const exerciseName = exercise.exercise?.spanish_name || 'Ejercicio'
        const muscleGroup = exercise.exercise?.primary_muscle_group || ''

        sections.push(
          new Paragraph({
            children: [
              new TextRun({
                text: `${exercise.orderIndex + 1}. ${exerciseName}`,
                bold: true,
                size: 24,
              }),
            ],
            spacing: { before: 200, after: 100 },
          })
        )

        sections.push(
          new Paragraph({
            children: [
              new TextRun({
                text: `   Grupo muscular: ${muscleGroup}`,
              }),
            ],
            spacing: { after: 50 },
          })
        )

        sections.push(
          new Paragraph({
            children: [
              new TextRun({
                text: `   Series: ${exercise.sets} | Repeticiones: ${exercise.reps} | Descanso: ${exercise.restSeconds}s`,
              }),
            ],
            spacing: { after: 100 },
          })
        )

        if (exercise.notes) {
          sections.push(
            new Paragraph({
              children: [
                new TextRun({
                  text: `   Notas: ${exercise.notes}`,
                  italics: true,
                }),
              ],
              spacing: { after: 100 },
            })
          )
        }
      }
    }

    return sections
  }

  /**
   * Exporta a HTML (para PDF)
   */
  async exportToHTML(plan: WorkoutPlan): Promise<string> {
    const html = `
<!DOCTYPE html>
<html lang="es">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>Rutina de ${plan.userName}</title>
  <style>
    body {
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      max-width: 800px;
      margin: 0 auto;
      padding: 40px 20px;
      color: #333;
      line-height: 1.6;
    }
    h1 {
      text-align: center;
      color: #2c3e50;
      border-bottom: 3px solid #3498db;
      padding-bottom: 15px;
      margin-bottom: 30px;
    }
    h2 {
      color: #3498db;
      margin-top: 30px;
      margin-bottom: 15px;
      border-left: 5px solid #3498db;
      padding-left: 10px;
    }
    .info-section {
      background-color: #f8f9fa;
      padding: 20px;
      border-radius: 8px;
      margin-bottom: 30px;
    }
    .info-item {
      margin-bottom: 10px;
    }
    .info-label {
      font-weight: bold;
      color: #2c3e50;
    }
    .routine-day {
      margin-bottom: 40px;
      page-break-inside: avoid;
    }
    .day-title {
      background-color: #3498db;
      color: white;
      padding: 12px 20px;
      border-radius: 8px;
      margin-bottom: 20px;
    }
    .exercise {
      background-color: #ffffff;
      border: 1px solid #e0e0e0;
      border-radius: 8px;
      padding: 15px;
      margin-bottom: 15px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }
    .exercise-name {
      font-size: 18px;
      font-weight: bold;
      color: #2c3e50;
      margin-bottom: 8px;
    }
    .exercise-details {
      display: flex;
      gap: 20px;
      flex-wrap: wrap;
      margin-bottom: 8px;
    }
    .detail-item {
      display: flex;
      align-items: center;
      gap: 5px;
    }
    .detail-label {
      font-weight: 600;
      color: #7f8c8d;
    }
    .detail-value {
      color: #2c3e50;
    }
    .notes {
      background-color: #fff3cd;
      padding: 10px;
      border-left: 3px solid #ffc107;
      margin-top: 10px;
      font-style: italic;
    }
    .footer {
      margin-top: 50px;
      padding-top: 20px;
      border-top: 2px solid #e0e0e0;
      text-align: center;
      color: #7f8c8d;
      font-size: 14px;
    }
    @media print {
      body {
        padding: 20px;
      }
      .routine-day {
        page-break-inside: avoid;
      }
    }
  </style>
</head>
<body>
  <h1>RUTINA DE ENTRENAMIENTO PERSONALIZADA</h1>

  <div class="info-section">
    <h2>Información del Cliente</h2>
    <div class="info-item">
      <span class="info-label">Nombre:</span> ${plan.userName}
    </div>
    <div class="info-item">
      <span class="info-label">Edad:</span> ${plan.userAge || 'No especificada'}
    </div>
    <div class="info-item">
      <span class="info-label">Nivel de Fitness:</span> ${plan.fitnessLevel}
    </div>
    <div class="info-item">
      <span class="info-label">Días de entrenamiento:</span> ${plan.trainingDays}
    </div>
    <div class="info-item">
      <span class="info-label">Objetivos:</span> ${plan.goals.join(', ')}
    </div>
    <div class="info-item">
      <span class="info-label">Fecha:</span> ${format(new Date(), 'dd/MM/yyyy')}
    </div>
  </div>

  ${plan.routines
    .map(
      (routine) => `
    <div class="routine-day">
      <div class="day-title">
        <h2 style="margin: 0; color: white;">${routine.dayName}</h2>
        <div style="font-size: 14px; margin-top: 5px;">Enfoque: ${routine.focus}</div>
      </div>

      ${routine.exercises
        .map(
          (exercise, index) => `
        <div class="exercise">
          <div class="exercise-name">
            ${index + 1}. ${exercise.exercise?.spanish_name || 'Ejercicio'}
          </div>
          <div style="color: #7f8c8d; margin-bottom: 10px;">
            ${exercise.exercise?.primary_muscle_group || ''}
          </div>
          <div class="exercise-details">
            <div class="detail-item">
              <span class="detail-label">Series:</span>
              <span class="detail-value">${exercise.sets}</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Repeticiones:</span>
              <span class="detail-value">${exercise.reps}</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Descanso:</span>
              <span class="detail-value">${exercise.restSeconds}s</span>
            </div>
          </div>
          ${
            exercise.notes
              ? `<div class="notes">Nota: ${exercise.notes}</div>`
              : ''
          }
        </div>
      `
        )
        .join('')}
    </div>
  `
    )
    .join('')}

  <div class="footer">
    <p>Generado el ${format(new Date(), "dd/MM/yyyy 'a las' HH:mm")}</p>
    <p style="margin-top: 20px; font-size: 12px;">
      Recuerda: Realiza un calentamiento adecuado antes de comenzar y consulta con un profesional si tienes dudas.
    </p>
  </div>
</body>
</html>
    `

    return html
  }
}
