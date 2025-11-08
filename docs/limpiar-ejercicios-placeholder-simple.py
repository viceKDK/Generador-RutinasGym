#!/usr/bin/env python3
"""
Script simple para eliminar ejercicios predeterminados/placeholder de la base de datos
"""

import sqlite3
import os
from datetime import datetime

def main():
    db_path = 'gymroutine.db'

    if not os.path.exists(db_path):
        print(f"ERROR: No se encontro la base de datos: {db_path}")
        return

    # Crear backup
    backup_path = f"{db_path}.backup_{datetime.now().strftime('%Y%m%d_%H%M%S')}"
    print(f"Creando backup: {backup_path}")

    with open(db_path, 'rb') as source:
        with open(backup_path, 'wb') as backup:
            backup.write(source.read())

    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()

    try:
        # Buscar ejercicios placeholder
        cursor.execute("""
            SELECT Id, Name, SpanishName
            FROM Exercises
            WHERE Name LIKE '%sin imagen%'
               OR SpanishName LIKE '%sin imagen%'
            ORDER BY Id;
        """)

        exercises = cursor.fetchall()

        if not exercises:
            print("No se encontraron ejercicios placeholder para eliminar.")
            return

        print(f"\nEjercicios placeholder encontrados: {len(exercises)}")
        for ex in exercises:
            print(f"  ID: {ex[0]}, Nombre: {ex[1]}")

        exercise_ids = [ex[0] for ex in exercises]
        ids_str = ','.join(map(str, exercise_ids))

        # Buscar datos relacionados
        cursor.execute(f"SELECT COUNT(*) FROM ExerciseImages WHERE ExerciseId IN ({ids_str});")
        image_count = cursor.fetchone()[0]

        cursor.execute(f"SELECT COUNT(*) FROM ExerciseSecondaryMuscles WHERE ExerciseId IN ({ids_str});")
        secondary_count = cursor.fetchone()[0]

        print(f"\nDatos relacionados:")
        print(f"  - Imagenes: {image_count}")
        print(f"  - Musculos secundarios: {secondary_count}")

        # Confirmar eliminacion
        print(f"\nADVERTENCIA: Se eliminaran {len(exercises)} ejercicios y sus datos relacionados")
        response = input("Continuar? (escriba 'SI' para confirmar): ")

        if response != 'SI':
            print("Operacion cancelada.")
            return

        print("\nEliminando datos...")

        # Eliminar en orden correcto (relaciones primero)
        cursor.execute(f"DELETE FROM ExerciseImages WHERE ExerciseId IN ({ids_str});")
        print(f"  Eliminadas {image_count} imagenes")

        cursor.execute(f"DELETE FROM ExerciseSecondaryMuscles WHERE ExerciseId IN ({ids_str});")
        print(f"  Eliminadas {secondary_count} relaciones de musculos secundarios")

        cursor.execute(f"DELETE FROM Exercises WHERE Id IN ({ids_str});")
        print(f"  Eliminados {len(exercises)} ejercicios placeholder")

        conn.commit()
        print(f"\nLimpieza completada exitosamente!")
        print(f"Backup guardado en: {backup_path}")

        # Estadisticas finales
        cursor.execute("SELECT COUNT(*) FROM Exercises;")
        total_exercises = cursor.fetchone()[0]
        print(f"Ejercicios restantes: {total_exercises}")

    except Exception as e:
        print(f"Error: {e}")
        conn.rollback()
        print("Cambios revertidos")

    finally:
        conn.close()

if __name__ == "__main__":
    main()