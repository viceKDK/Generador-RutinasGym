#!/usr/bin/env python3
"""
Script para eliminar ejercicios predeterminados/placeholder de la base de datos
Estos son ejercicios que no fueron agregados por el usuario y tienen nombres como "sin imagen"
"""

import sqlite3
import os
from datetime import datetime

def backup_database(db_path):
    """Crear backup de la base de datos antes de hacer cambios"""
    backup_path = f"{db_path}.backup_{datetime.now().strftime('%Y%m%d_%H%M%S')}"
    print(f"Creando backup: {backup_path}")

    with open(db_path, 'rb') as source:
        with open(backup_path, 'wb') as backup:
            backup.write(source.read())

    return backup_path

def get_placeholder_exercises(cursor):
    """Obtener lista de ejercicios placeholder"""
    query = """
        SELECT Id, Name, SpanishName, Description
        FROM Exercises
        WHERE Name LIKE '%sin imagen%'
           OR Name LIKE '%placeholder%'
           OR Name LIKE '%sin foto%'
           OR Name LIKE '%default%'
           OR SpanishName LIKE '%sin imagen%'
           OR SpanishName LIKE '%placeholder%'
        ORDER BY Id;
    """

    cursor.execute(query)
    return cursor.fetchall()

def get_related_data(cursor, exercise_ids):
    """Obtener datos relacionados que también deben eliminarse"""
    if not exercise_ids:
        return [], []

    ids_str = ','.join(map(str, exercise_ids))

    # Imágenes relacionadas
    cursor.execute(f"""
        SELECT Id, ExerciseId, ImagePath
        FROM ExerciseImages
        WHERE ExerciseId IN ({ids_str});
    """)
    images = cursor.fetchall()

    # Músculos secundarios relacionados
    cursor.execute(f"""
        SELECT ExerciseId, MuscleGroupId
        FROM ExerciseSecondaryMuscles
        WHERE ExerciseId IN ({ids_str});
    """)
    secondary_muscles = cursor.fetchall()

    return images, secondary_muscles

def delete_placeholder_exercises(db_path, dry_run=True):
    """Eliminar ejercicios placeholder y sus datos relacionados"""

    # Crear backup
    backup_path = backup_database(db_path)

    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()

    try:
        # Obtener ejercicios placeholder
        placeholder_exercises = get_placeholder_exercises(cursor)

        if not placeholder_exercises:
            print("No se encontraron ejercicios placeholder para eliminar.")
            return

        print(f"\nEjercicios placeholder encontrados: {len(placeholder_exercises)}")
        for ex in placeholder_exercises:
            print(f"  ID: {ex[0]}, Nombre: {ex[1]}, Español: {ex[2]}")

        exercise_ids = [ex[0] for ex in placeholder_exercises]

        # Obtener datos relacionados
        images, secondary_muscles = get_related_data(cursor, exercise_ids)

        print(f"\nDatos relacionados:")
        print(f"  - Imágenes: {len(images)}")
        print(f"  - Músculos secundarios: {len(secondary_muscles)}")

        if dry_run:
            print(f"\n🔍 MODO DRY RUN - No se realizarán cambios")
            print(f"Para ejecutar los cambios, ejecuta: python {__file__} --execute")
            return

        # Confirmar eliminación
        print(f"\n⚠️  ADVERTENCIA: Se eliminarán {len(placeholder_exercises)} ejercicios y sus datos relacionados")
        response = input("¿Continuar? (escriba 'SI' para confirmar): ")

        if response != 'SI':
            print("Operación cancelada.")
            return

        # Ejecutar eliminaciones
        print("\n🗑️  Eliminando datos...")

        # 1. Eliminar imágenes relacionadas
        if images:
            ids_str = ','.join(map(str, exercise_ids))
            cursor.execute(f"DELETE FROM ExerciseImages WHERE ExerciseId IN ({ids_str});")
            print(f"  ✅ Eliminadas {len(images)} imágenes")

        # 2. Eliminar músculos secundarios relacionados
        if secondary_muscles:
            ids_str = ','.join(map(str, exercise_ids))
            cursor.execute(f"DELETE FROM ExerciseSecondaryMuscles WHERE ExerciseId IN ({ids_str});")
            print(f"  ✅ Eliminadas {len(secondary_muscles)} relaciones de músculos secundarios")

        # 3. Eliminar ejercicios
        ids_str = ','.join(map(str, exercise_ids))
        cursor.execute(f"DELETE FROM Exercises WHERE Id IN ({ids_str});")
        print(f"  ✅ Eliminados {len(placeholder_exercises)} ejercicios placeholder")

        # Confirmar cambios
        conn.commit()
        print(f"\n✅ Limpieza completada exitosamente!")
        print(f"📁 Backup guardado en: {backup_path}")

        # Mostrar estadísticas finales
        cursor.execute("SELECT COUNT(*) FROM Exercises;")
        total_exercises = cursor.fetchone()[0]
        print(f"📊 Ejercicios restantes en la base de datos: {total_exercises}")

    except Exception as e:
        print(f"❌ Error durante la eliminación: {e}")
        conn.rollback()
        print("🔄 Cambios revertidos")

    finally:
        conn.close()

def main():
    import sys

    db_path = 'gymroutine.db'

    if not os.path.exists(db_path):
        print(f"❌ No se encontró la base de datos: {db_path}")
        return

    # Verificar si se debe ejecutar o solo hacer dry run
    execute = '--execute' in sys.argv or '-e' in sys.argv
    dry_run = not execute

    print("🧹 Script de Limpieza de Ejercicios Placeholder")
    print("=" * 50)

    delete_placeholder_exercises(db_path, dry_run=dry_run)

if __name__ == "__main__":
    main()