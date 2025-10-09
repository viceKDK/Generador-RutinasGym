import sqlite3
import json
import shutil
from datetime import datetime

def backup_existing_data():
    """Backup existing exercises and related data"""
    print("=== RESPALDANDO DATOS EXISTENTES ===")

    # Try to find and backup from existing database
    db_files = ['gymroutine.db', 'gym_routine.db', 'app-ui/gymroutine.db']
    backup_data = {
        'exercises': [],
        'muscle_groups': [],
        'equipment_types': [],
        'backup_timestamp': datetime.now().isoformat()
    }

    for db_file in db_files:
        try:
            conn = sqlite3.connect(db_file)
            cursor = conn.cursor()

            # Backup exercises
            try:
                cursor.execute("SELECT * FROM Exercises")
                exercises = cursor.fetchall()

                if exercises:
                    # Get column names
                    cursor.execute("PRAGMA table_info(Exercises)")
                    columns = [col[1] for col in cursor.fetchall()]

                    for exercise in exercises:
                        exercise_dict = dict(zip(columns, exercise))
                        backup_data['exercises'].append(exercise_dict)

                    print(f"✅ Respaldados {len(exercises)} ejercicios desde {db_file}")
            except Exception as e:
                print(f"⚠️ No se pudieron respaldar ejercicios desde {db_file}: {e}")

            # Backup muscle groups
            try:
                cursor.execute("SELECT * FROM MuscleGroups")
                muscle_groups = cursor.fetchall()

                if muscle_groups:
                    cursor.execute("PRAGMA table_info(MuscleGroups)")
                    columns = [col[1] for col in cursor.fetchall()]

                    for mg in muscle_groups:
                        mg_dict = dict(zip(columns, mg))
                        backup_data['muscle_groups'].append(mg_dict)

                    print(f"✅ Respaldados {len(muscle_groups)} grupos musculares desde {db_file}")
            except Exception as e:
                print(f"⚠️ No se pudieron respaldar grupos musculares: {e}")

            # Backup equipment types
            try:
                cursor.execute("SELECT * FROM EquipmentTypes")
                equipment_types = cursor.fetchall()

                if equipment_types:
                    cursor.execute("PRAGMA table_info(EquipmentTypes)")
                    columns = [col[1] for col in cursor.fetchall()]

                    for et in equipment_types:
                        et_dict = dict(zip(columns, et))
                        backup_data['equipment_types'].append(et_dict)

                    print(f"✅ Respaldados {len(equipment_types)} tipos de equipamiento desde {db_file}")
            except Exception as e:
                print(f"⚠️ No se pudieron respaldar tipos de equipamiento: {e}")

            conn.close()

            # If we found data, stop looking
            if backup_data['exercises'] or backup_data['muscle_groups'] or backup_data['equipment_types']:
                print(f"✅ Datos encontrados en {db_file}")
                break

        except Exception as e:
            print(f"⚠️ No se pudo acceder a {db_file}: {e}")

    # Save backup to JSON
    with open('data_backup.json', 'w', encoding='utf-8') as f:
        json.dump(backup_data, f, indent=2, ensure_ascii=False)

    print(f"📁 Backup guardado en data_backup.json")
    print(f"📊 Total respaldado: {len(backup_data['exercises'])} ejercicios, "
          f"{len(backup_data['muscle_groups'])} grupos musculares, "
          f"{len(backup_data['equipment_types'])} tipos de equipamiento")

    return backup_data

def create_fresh_database():
    """Create a completely fresh database with correct schema"""
    print("\n=== CREANDO BASE DE DATOS NUEVA ===")

    # Remove old databases
    db_files = ['gymroutine.db', 'gym_routine.db']
    for db_file in db_files:
        try:
            import os
            if os.path.exists(db_file):
                shutil.move(db_file, f"{db_file}.old_backup")
                print(f"📦 Movido {db_file} a {db_file}.old_backup")
        except Exception as e:
            print(f"⚠️ Error moviendo {db_file}: {e}")

    # Create fresh database
    conn = sqlite3.connect('gymroutine.db')
    cursor = conn.cursor()

    # Create tables with correct schema including ImageMetadata
    print("🏗️ Creando tablas...")

    # MuscleGroups table
    cursor.execute("""
        CREATE TABLE MuscleGroups (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            SpanishName TEXT NOT NULL,
            Description TEXT NOT NULL
        )
    """)
    print("✅ Tabla MuscleGroups creada")

    # EquipmentTypes table
    cursor.execute("""
        CREATE TABLE EquipmentTypes (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            SpanishName TEXT NOT NULL,
            Description TEXT NOT NULL
        )
    """)
    print("✅ Tabla EquipmentTypes creada")

    # Exercises table
    cursor.execute("""
        CREATE TABLE Exercises (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            SpanishName TEXT NOT NULL,
            Description TEXT NOT NULL,
            Instructions TEXT NOT NULL,
            PrimaryMuscleGroupId INTEGER NOT NULL,
            EquipmentTypeId INTEGER NOT NULL,
            DifficultyLevel INTEGER NOT NULL,
            ExerciseType INTEGER NOT NULL,
            DurationSeconds INTEGER NULL,
            IsActive INTEGER NOT NULL DEFAULT 1,
            CreatedAt TEXT NOT NULL,
            UpdatedAt TEXT NULL,
            ParentExerciseId INTEGER NULL,
            FOREIGN KEY (PrimaryMuscleGroupId) REFERENCES MuscleGroups(Id),
            FOREIGN KEY (EquipmentTypeId) REFERENCES EquipmentTypes(Id),
            FOREIGN KEY (ParentExerciseId) REFERENCES Exercises(Id)
        )
    """)
    print("✅ Tabla Exercises creada")

    # ExerciseImages table WITH ImageMetadata column
    cursor.execute("""
        CREATE TABLE ExerciseImages (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            ExerciseId INTEGER NOT NULL,
            ImagePath TEXT NULL,
            ImageData BLOB NOT NULL DEFAULT x'',
            ImageMetadata TEXT NOT NULL DEFAULT '',
            ImagePosition TEXT NOT NULL,
            IsPrimary INTEGER NOT NULL DEFAULT 0,
            Description TEXT NOT NULL,
            FOREIGN KEY (ExerciseId) REFERENCES Exercises(Id) ON DELETE CASCADE
        )
    """)
    print("✅ Tabla ExerciseImages creada CON COLUMNA ImageMetadata")

    # ExerciseSecondaryMuscles table
    cursor.execute("""
        CREATE TABLE ExerciseSecondaryMuscles (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            ExerciseId INTEGER NOT NULL,
            MuscleGroupId INTEGER NOT NULL,
            FOREIGN KEY (ExerciseId) REFERENCES Exercises(Id) ON DELETE CASCADE,
            FOREIGN KEY (MuscleGroupId) REFERENCES MuscleGroups(Id)
        )
    """)
    print("✅ Tabla ExerciseSecondaryMuscles creada")

    conn.commit()
    conn.close()

    print("🎉 Base de datos nueva creada exitosamente con schema correcto")
    return True

def populate_basic_data(backup_data):
    """Populate the new database with backed up data plus some basics"""
    print("\n=== POBLANDO BASE DE DATOS NUEVA ===")

    conn = sqlite3.connect('gymroutine.db')
    cursor = conn.cursor()

    # Insert basic muscle groups if none in backup
    if not backup_data['muscle_groups']:
        basic_muscle_groups = [
            (1, "Chest", "Pecho", "Músculos del pecho"),
            (2, "Back", "Espalda", "Músculos de la espalda"),
            (3, "Shoulders", "Hombros", "Músculos de los hombros"),
            (4, "Arms", "Brazos", "Músculos de los brazos"),
            (5, "Legs", "Piernas", "Músculos de las piernas"),
            (6, "Core", "Abdomen", "Músculos del core y abdomen"),
            (7, "Glutes", "Glúteos", "Músculos de los glúteos"),
            (8, "Calves", "Pantorrillas", "Músculos de las pantorrillas")
        ]

        cursor.executemany(
            "INSERT INTO MuscleGroups (Id, Name, SpanishName, Description) VALUES (?, ?, ?, ?)",
            basic_muscle_groups
        )
        print(f"✅ Insertados {len(basic_muscle_groups)} grupos musculares básicos")
    else:
        # Insert backed up muscle groups
        for mg in backup_data['muscle_groups']:
            cursor.execute(
                "INSERT INTO MuscleGroups (Id, Name, SpanishName, Description) VALUES (?, ?, ?, ?)",
                (mg.get('Id'), mg.get('Name'), mg.get('SpanishName'), mg.get('Description'))
            )
        print(f"✅ Restaurados {len(backup_data['muscle_groups'])} grupos musculares")

    # Insert basic equipment types if none in backup
    if not backup_data['equipment_types']:
        basic_equipment = [
            (1, "Bodyweight", "Peso corporal", "Sin equipamiento"),
            (2, "Dumbbells", "Mancuernas", "Ejercicios con mancuernas"),
            (3, "Barbell", "Barra", "Ejercicios con barra"),
            (4, "Resistance Bands", "Bandas elásticas", "Ejercicios con bandas"),
            (5, "Cable Machine", "Máquina de cables", "Ejercicios en máquina de cables"),
            (6, "Machine", "Máquina", "Ejercicios en máquina"),
            (7, "Kettlebell", "Pesa rusa", "Ejercicios con pesa rusa"),
            (8, "Medicine Ball", "Balón medicinal", "Ejercicios con balón")
        ]

        cursor.executemany(
            "INSERT INTO EquipmentTypes (Id, Name, SpanishName, Description) VALUES (?, ?, ?, ?)",
            basic_equipment
        )
        print(f"✅ Insertados {len(basic_equipment)} tipos de equipamiento básicos")
    else:
        # Insert backed up equipment types
        for et in backup_data['equipment_types']:
            cursor.execute(
                "INSERT INTO EquipmentTypes (Id, Name, SpanishName, Description) VALUES (?, ?, ?, ?)",
                (et.get('Id'), et.get('Name'), et.get('SpanishName'), et.get('Description'))
            )
        print(f"✅ Restaurados {len(backup_data['equipment_types'])} tipos de equipamiento")

    # Insert backed up exercises or create basic ones
    if backup_data['exercises']:
        for ex in backup_data['exercises']:
            try:
                cursor.execute("""
                    INSERT INTO Exercises (Id, Name, SpanishName, Description, Instructions,
                                         PrimaryMuscleGroupId, EquipmentTypeId, DifficultyLevel,
                                         ExerciseType, DurationSeconds, IsActive, CreatedAt, UpdatedAt, ParentExerciseId)
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
                """, (
                    ex.get('Id'), ex.get('Name'), ex.get('SpanishName'), ex.get('Description'),
                    ex.get('Instructions'), ex.get('PrimaryMuscleGroupId'), ex.get('EquipmentTypeId'),
                    ex.get('DifficultyLevel'), ex.get('ExerciseType'), ex.get('DurationSeconds'),
                    ex.get('IsActive', 1), ex.get('CreatedAt'), ex.get('UpdatedAt'), ex.get('ParentExerciseId')
                ))
            except Exception as e:
                print(f"⚠️ Error insertando ejercicio {ex.get('SpanishName', 'N/A')}: {e}")

        print(f"✅ Restaurados {len(backup_data['exercises'])} ejercicios")
    else:
        # Create basic exercises
        basic_exercises = [
            (1, "Push-up", "Flexiones", "Ejercicio básico de empuje", "Baja el cuerpo hasta que el pecho casi toque el suelo, luego empuja hacia arriba", 1, 1, 0, 0, None, 1, datetime.now().isoformat(), None, None),
            (2, "Pull-up", "Dominadas", "Ejercicio de tracción", "Cuelga de una barra y tira hacia arriba hasta que la barbilla pase la barra", 2, 1, 1, 0, None, 1, datetime.now().isoformat(), None, None),
            (3, "Squat", "Sentadillas", "Ejercicio básico de piernas", "Baja como si te fueras a sentar manteniendo la espalda recta, luego sube", 5, 1, 0, 0, None, 1, datetime.now().isoformat(), None, None),
            (4, "Plank", "Plancha", "Ejercicio de core", "Mantén el cuerpo recto apoyado en antebrazos y pies", 6, 1, 0, 0, 60, 1, datetime.now().isoformat(), None, None),
            (5, "Dumbbell Curl", "Curl con mancuernas", "Ejercicio de bíceps", "Flexiona el codo levantando la mancuerna hacia el hombro", 4, 2, 0, 0, None, 1, datetime.now().isoformat(), None, None),
            (6, "Deadlift", "Peso muerto", "Ejercicio compuesto", "Levanta la barra desde el suelo manteniendo la espalda recta", 2, 3, 2, 0, None, 1, datetime.now().isoformat(), None, None),
            (7, "Lunges", "Zancadas", "Ejercicio de piernas", "Da un paso adelante y baja la rodilla trasera hacia el suelo", 5, 1, 1, 0, None, 1, datetime.now().isoformat(), None, None),
            (8, "Shoulder Press", "Press de hombros", "Ejercicio de hombros", "Empuja las mancuernas hacia arriba desde los hombros", 3, 2, 1, 0, None, 1, datetime.now().isoformat(), None, None)
        ]

        cursor.executemany("""
            INSERT INTO Exercises (Id, Name, SpanishName, Description, Instructions,
                                 PrimaryMuscleGroupId, EquipmentTypeId, DifficultyLevel,
                                 ExerciseType, DurationSeconds, IsActive, CreatedAt, UpdatedAt, ParentExerciseId)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        """, basic_exercises)

        print(f"✅ Insertados {len(basic_exercises)} ejercicios básicos")

    conn.commit()
    conn.close()

    print("🎉 Base de datos poblada exitosamente")

def verify_new_database():
    """Verify the new database has correct structure and data"""
    print("\n=== VERIFICANDO BASE DE DATOS NUEVA ===")

    conn = sqlite3.connect('gymroutine.db')
    cursor = conn.cursor()

    # Check table structure
    cursor.execute("PRAGMA table_info(ExerciseImages)")
    columns = cursor.fetchall()

    has_image_metadata = any(col[1] == 'ImageMetadata' for col in columns)
    has_image_data = any(col[1] == 'ImageData' for col in columns)

    print("📋 Columnas en tabla ExerciseImages:")
    for col in columns:
        print(f"  - {col[1]} ({col[2]})")

    print(f"\n✅ ImageMetadata column: {'✅ SÍ' if has_image_metadata else '❌ NO'}")
    print(f"✅ ImageData column: {'✅ SÍ' if has_image_data else '❌ NO'}")

    # Check data counts
    cursor.execute("SELECT COUNT(*) FROM Exercises")
    exercise_count = cursor.fetchone()[0]

    cursor.execute("SELECT COUNT(*) FROM MuscleGroups")
    mg_count = cursor.fetchone()[0]

    cursor.execute("SELECT COUNT(*) FROM EquipmentTypes")
    et_count = cursor.fetchone()[0]

    print(f"\n📊 Datos en nueva BD:")
    print(f"  - Ejercicios: {exercise_count}")
    print(f"  - Grupos musculares: {mg_count}")
    print(f"  - Tipos de equipamiento: {et_count}")

    # Show some sample exercises
    cursor.execute("SELECT SpanishName, Description FROM Exercises LIMIT 5")
    exercises = cursor.fetchall()

    print(f"\n📝 Ejercicios de ejemplo:")
    for ex in exercises:
        print(f"  - {ex[0]}: {ex[1]}")

    conn.close()

    if has_image_metadata and has_image_data and exercise_count > 0:
        print("\n🎉 BASE DE DATOS NUEVA LISTA PARA USAR")
        print("✅ Estructura correcta con columnas ImageMetadata e ImageData")
        print("✅ Datos de ejercicios disponibles")
        print("\n🚀 AHORA PUEDES PROBAR LA APLICACIÓN:")
        print("  1. Cierra y vuelve a abrir la aplicación")
        print("  2. Ve al Gestor de Imágenes")
        print("  3. Selecciona un ejercicio")
        print("  4. Agrega una imagen")
        print("  5. ¡Debería funcionar sin errores!")
        return True
    else:
        print("\n❌ Hay problemas con la nueva BD")
        return False

if __name__ == "__main__":
    try:
        # Step 1: Backup existing data
        backup_data = backup_existing_data()

        # Step 2: Create fresh database
        create_fresh_database()

        # Step 3: Populate with data
        populate_basic_data(backup_data)

        # Step 4: Verify everything is correct
        success = verify_new_database()

        if success:
            print("\n🎯 OPERACIÓN COMPLETADA EXITOSAMENTE")
            print("La aplicación ahora debería funcionar sin errores de ImageMetadata")
        else:
            print("\n⚠️ Hubo problemas. Revisa los errores arriba.")

    except Exception as e:
        print(f"\n❌ ERROR GENERAL: {e}")
        import traceback
        traceback.print_exc()