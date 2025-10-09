using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GymRoutineGenerator.UI
{
    /// <summary>
    /// Database handler for exercise images using SQLite
    /// </summary>
    public class SQLiteExerciseImageDatabase
    {
        private readonly string _connectionString;

        public SQLiteExerciseImageDatabase()
        {
            // Buscar la base de datos en múltiples ubicaciones posibles
            var dbPath = FindDatabasePath();

            if (string.IsNullOrEmpty(dbPath))
            {
                // Si no se encuentra, usar una en el directorio base
                dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "gymroutine.db");
                dbPath = Path.GetFullPath(dbPath);
            }

            _connectionString = $"Data Source={dbPath};Version=3;";
        }

        private string? FindDatabasePath()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var current = new DirectoryInfo(baseDir);

            // Buscar hacia arriba hasta 10 niveles
            for (int i = 0; i < 10 && current != null; i++)
            {
                var dbPath = Path.Combine(current.FullName, "gymroutine.db");
                if (File.Exists(dbPath))
                {
                    return dbPath;
                }
                current = current.Parent;
            }

            return null;
        }

        public ExerciseImageInfo? FindExerciseImage(string exerciseName)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    // Buscar por nombre exacto en español o inglés, traer ImageData (BLOB)
                    var query = @"
                        SELECT e.Id, e.Name, e.SpanishName, ei.ImageData, ei.ImagePath, ei.Description
                        FROM Exercises e
                        LEFT JOIN ExerciseImages ei ON e.Id = ei.ExerciseId
                        WHERE (e.Name LIKE @name OR e.SpanishName LIKE @name)
                        AND (ei.ImageData IS NOT NULL OR ei.ImagePath IS NOT NULL)
                        ORDER BY ei.IsPrimary DESC
                        LIMIT 1";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@name", $"%{exerciseName}%");

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var imageData = reader["ImageData"] as byte[];
                                var imagePath = reader["ImagePath"]?.ToString();

                                if (imageData != null || !string.IsNullOrWhiteSpace(imagePath))
                                {
                                    return new ExerciseImageInfo
                                    {
                                        ExerciseName = exerciseName,
                                        ImagePath = imagePath ?? "",
                                        ImageData = imageData,
                                        Description = reader["Description"]?.ToString() ?? "",
                                        Keywords = new string[0],
                                        MuscleGroups = new string[0]
                                    };
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en FindExerciseImage: {ex.Message}");
            }

            return null;
        }

        public bool HasImageForExercise(string exerciseName)
        {
            return FindExerciseImage(exerciseName) != null;
        }

        public string? GetImagePath(string exerciseName)
        {
            var imageInfo = FindExerciseImage(exerciseName);
            return imageInfo?.ImagePath;
        }

        public List<ExerciseImageInfo> GetAllExercises()
        {
            var exercises = new List<ExerciseImageInfo>();

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"
                        SELECT e.Id, e.Name, e.SpanishName, e.Description, ei.ImageData, ei.ImagePath,
                               mg.SpanishName as PrimaryMuscleGroup
                        FROM Exercises e
                        LEFT JOIN ExerciseImages ei ON e.Id = ei.ExerciseId
                        LEFT JOIN MuscleGroups mg ON e.PrimaryMuscleGroupId = mg.Id
                        WHERE e.IsActive = 1
                        ORDER BY e.SpanishName, e.Name";

                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var spanishName = reader["SpanishName"]?.ToString() ?? reader["Name"]?.ToString() ?? "";
                            var imagePath = reader["ImagePath"]?.ToString();
                            var imageData = reader["ImageData"] as byte[];
                            var description = reader["Description"]?.ToString();
                            var primaryMuscleGroup = reader["PrimaryMuscleGroup"]?.ToString();

                            var muscleGroups = new List<string>();
                            if (!string.IsNullOrEmpty(primaryMuscleGroup))
                            {
                                muscleGroups.Add(primaryMuscleGroup);
                            }

                            exercises.Add(new ExerciseImageInfo
                            {
                                ExerciseName = spanishName,
                                ImagePath = imagePath ?? "",
                                ImageData = imageData,
                                Description = description ?? "",
                                Keywords = new string[0],
                                MuscleGroups = muscleGroups.ToArray()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en GetAllExercises: {ex.Message}");
            }

            return exercises;
        }

        public bool ImportImageForExercise(string exerciseName, string sourceImagePath)
        {
            try
            {
                if (!File.Exists(sourceImagePath))
                {
                    System.Diagnostics.Debug.WriteLine($"Archivo fuente no existe: {sourceImagePath}");
                    return false;
                }

                // Leer imagen como bytes
                byte[] imageBytes;
                using (var sourceStream = new FileStream(sourceImagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    imageBytes = new byte[sourceStream.Length];
                    sourceStream.Read(imageBytes, 0, imageBytes.Length);
                }

                System.Diagnostics.Debug.WriteLine($"Imagen leida: {imageBytes.Length} bytes");

                // Guardar en base de datos como BLOB
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    // Buscar ID del ejercicio
                    var exerciseId = GetExerciseId(connection, exerciseName);
                    if (exerciseId == null)
                    {
                        // Si no existe, crearlo
                        System.Diagnostics.Debug.WriteLine($"Creando nuevo ejercicio: {exerciseName}");
                        exerciseId = CreateExercise(connection, exerciseName);
                    }

                    System.Diagnostics.Debug.WriteLine($"Exercise ID: {exerciseId}");

                    // Verificar si ya existe una imagen para este ejercicio
                    var existingImageQuery = "SELECT COUNT(*) FROM ExerciseImages WHERE ExerciseId = @exerciseId";
                    using (var checkCommand = new SQLiteCommand(existingImageQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@exerciseId", exerciseId.Value);
                        var count = Convert.ToInt32(checkCommand.ExecuteScalar());

                        if (count > 0)
                        {
                            // Actualizar imagen existente (SOBRESCRIBIR)
                            System.Diagnostics.Debug.WriteLine($"Sobrescribiendo imagen existente para ejercicio {exerciseId}");
                            var updateQuery = "UPDATE ExerciseImages SET ImageData = @imageData, ImagePath = '' WHERE ExerciseId = @exerciseId";
                            using (var updateCommand = new SQLiteCommand(updateQuery, connection))
                            {
                                updateCommand.Parameters.AddWithValue("@imageData", imageBytes);
                                updateCommand.Parameters.AddWithValue("@exerciseId", exerciseId.Value);
                                var rows = updateCommand.ExecuteNonQuery();
                                System.Diagnostics.Debug.WriteLine($"Filas actualizadas: {rows}");
                            }
                        }
                        else
                        {
                            // Insertar nueva imagen como BLOB
                            System.Diagnostics.Debug.WriteLine($"Insertando nueva imagen como BLOB para ejercicio {exerciseId}");
                            var insertQuery = @"
                                INSERT INTO ExerciseImages (ExerciseId, ImageData, ImagePath, ImagePosition, IsPrimary, Description)
                                VALUES (@exerciseId, @imageData, '', 'Front', 1, '')";

                            using (var insertCommand = new SQLiteCommand(insertQuery, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@exerciseId", exerciseId.Value);
                                insertCommand.Parameters.AddWithValue("@imageData", imageBytes);
                                var rows = insertCommand.ExecuteNonQuery();
                                System.Diagnostics.Debug.WriteLine($"Filas insertadas: {rows}");
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"✅ Imagen importada exitosamente para: {exerciseName}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error importando imagen: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        private int? GetExerciseId(SQLiteConnection connection, string exerciseName)
        {
            var query = "SELECT Id FROM Exercises WHERE Name = @name OR SpanishName = @name LIMIT 1";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@name", exerciseName);
                var result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : null;
            }
        }

        private int CreateExercise(SQLiteConnection connection, string exerciseName)
        {
            var insertQuery = @"
                INSERT INTO Exercises (Name, SpanishName, Description, Instructions, PrimaryMuscleGroupId, EquipmentTypeId, DifficultyLevel, ExerciseType, IsActive, CreatedAt)
                VALUES (@name, @spanishName, '', '', 1, 1, 1, 0, 1, @createdAt);
                SELECT last_insert_rowid();";

            using (var command = new SQLiteCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@name", exerciseName);
                command.Parameters.AddWithValue("@spanishName", exerciseName);
                command.Parameters.AddWithValue("@createdAt", DateTime.Now);

                var result = command.ExecuteScalar();
                return Convert.ToInt32(result);
            }
        }

        private string NormalizeFileName(string name)
        {
            // Reemplazar caracteres inválidos
            var invalidChars = Path.GetInvalidFileNameChars();
            var normalized = name;

            foreach (var c in invalidChars)
            {
                normalized = normalized.Replace(c, '_');
            }

            return normalized.Replace(" ", "_").ToLowerInvariant();
        }

        public bool AddOrUpdateExercise(string exerciseName, string imagePath, string[] keywords = null, string[] muscleGroups = null, string description = "")
        {
            try
            {
                if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    return ImportImageForExercise(exerciseName, imagePath);
                }

                // Si no hay imagen, solo crear/actualizar el ejercicio
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    var exerciseId = GetExerciseId(connection, exerciseName);
                    if (exerciseId == null)
                    {
                        // Crear nuevo ejercicio
                        exerciseId = CreateExercise(connection, exerciseName);
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool RemoveExercise(string exerciseName)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🗑️ Intentando eliminar ejercicio: {exerciseName}");

                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    var exerciseId = GetExerciseId(connection, exerciseName);
                    if (exerciseId == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Ejercicio no encontrado: {exerciseName}");
                        return false;
                    }

                    System.Diagnostics.Debug.WriteLine($"Exercise ID encontrado: {exerciseId}");

                    // Eliminar imágenes asociadas
                    var deleteQuery = "DELETE FROM ExerciseImages WHERE ExerciseId = @exerciseId";
                    using (var command = new SQLiteCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@exerciseId", exerciseId.Value);
                        var rowsAffected = command.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"Filas eliminadas de ExerciseImages: {rowsAffected}");
                    }

                    // Verificar eliminación
                    var checkQuery = "SELECT COUNT(*) FROM ExerciseImages WHERE ExerciseId = @exerciseId";
                    using (var checkCommand = new SQLiteCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@exerciseId", exerciseId.Value);
                        var remaining = Convert.ToInt32(checkCommand.ExecuteScalar());
                        System.Diagnostics.Debug.WriteLine($"Registros restantes: {remaining}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"✅ Imagen eliminada exitosamente para: {exerciseName}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error eliminando ejercicio: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }
    }
}
