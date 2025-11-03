using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace GymRoutineGenerator.UI
{
    /// <summary>
    /// Database handler for exercise images using SQLite.
    /// </summary>
    public class SQLiteExerciseImageDatabase
    {
        private readonly string _connectionString;
        private readonly HashSet<string> _exerciseColumns;

        public SQLiteExerciseImageDatabase()
        {
            var dbPath = FindDatabasePath();

            if (string.IsNullOrEmpty(dbPath))
            {
                var fallback = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "gymroutine.db");
                dbPath = Path.GetFullPath(fallback);
            }

            _connectionString = $"Data Source={dbPath};Version=3;";
            _exerciseColumns = LoadExerciseColumns();

            // Asegurar que la columna VideoUrl exista
            EnsureVideoUrlColumnExists();
        }

        private string? FindDatabasePath()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var current = new DirectoryInfo(baseDir);

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

        private HashSet<string> LoadExerciseColumns()
        {
            var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("PRAGMA table_info(Exercises);", connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var name = reader["name"]?.ToString();
                            if (!string.IsNullOrWhiteSpace(name))
                            {
                                columns.Add(name);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLiteExerciseImageDatabase] Column discovery failed: {ex.Message}");
            }

            return columns;
        }

        public ExerciseImageInfo? FindExerciseImage(string exerciseName)
        {
            if (string.IsNullOrWhiteSpace(exerciseName))
            {
                return null;
            }

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"
                        SELECT e.Id, e.Name, e.SpanishName, ei.ImageData, ei.ImagePath, ei.Description, e.VideoUrl
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
                                var videoUrl = SafeGetString(reader, "VideoUrl");

                                if (imageData != null || !string.IsNullOrWhiteSpace(imagePath))
                                {
                                    return new ExerciseImageInfo
                                    {
                                        ExerciseName = exerciseName,
                                        ImagePath = imagePath ?? string.Empty,
                                        ImageData = imageData,
                                        Description = reader["Description"]?.ToString() ?? string.Empty,
                                        VideoUrl = videoUrl ?? string.Empty,
                                        Keywords = Array.Empty<string>(),
                                        MuscleGroups = Array.Empty<string>()
                                    };
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLiteExerciseImageDatabase] FindExerciseImage error: {ex.Message}");
            }

            return null;
        }

        public bool HasImageForExercise(string exerciseName)
        {
            return FindExerciseImage(exerciseName) != null;
        }

        public string? GetImagePath(string exerciseName)
        {
            return FindExerciseImage(exerciseName)?.ImagePath;
        }

        public List<ExerciseImageInfo> GetAllExercises()
        {
            var exercises = new List<ExerciseImageInfo>();

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    var optionalSelect = new List<string>();
                    if (_exerciseColumns.Contains("Instructions"))
                    {
                        optionalSelect.Add("e.Instructions");
                    }

                    if (_exerciseColumns.Contains("Source"))
                    {
                        optionalSelect.Add("e.Source");
                    }

                    if (_exerciseColumns.Contains("VideoUrl"))
                    {
                        optionalSelect.Add("e.VideoUrl");
                    }

                    var query = $@"
                        SELECT e.Id, e.Name, e.SpanishName, e.Description, ei.ImageData, ei.ImagePath,
                               mg.SpanishName AS PrimaryMuscleGroup
                               {(optionalSelect.Count > 0 ? ", " + string.Join(", ", optionalSelect) : string.Empty)}
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
                            var spanishName = reader["SpanishName"]?.ToString() ?? reader["Name"]?.ToString() ?? string.Empty;
                            var imagePath = reader["ImagePath"]?.ToString();
                            var imageData = reader["ImageData"] as byte[];
                            var description = reader["Description"]?.ToString() ?? string.Empty;
                            var primaryMuscleGroup = reader["PrimaryMuscleGroup"]?.ToString();
                            var instructions = SafeGetString(reader, "Instructions");
                            var source = SafeGetString(reader, "Source");
                            var videoUrl = SafeGetString(reader, "VideoUrl");

                            var muscleGroups = new List<string>();
                            if (!string.IsNullOrWhiteSpace(primaryMuscleGroup))
                            {
                                muscleGroups.Add(primaryMuscleGroup);
                            }

                            exercises.Add(new ExerciseImageInfo
                            {
                                ExerciseName = spanishName,
                                ImagePath = imagePath ?? string.Empty,
                                ImageData = imageData,
                                Description = description,
                                VideoUrl = videoUrl ?? string.Empty,
                                Keywords = string.IsNullOrWhiteSpace(instructions)
                                    ? Array.Empty<string>()
                                    : instructions.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(k => k.Trim())
                                        .Where(k => k.Length > 0)
                                        .ToArray(),
                                MuscleGroups = muscleGroups.ToArray(),
                                Source = source ?? string.Empty
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLiteExerciseImageDatabase] GetAllExercises error: {ex.Message}");
            }

            return exercises;
        }

        public bool ImportImageForExercise(string exerciseName, string sourceImagePath)
        {
            try
            {
                if (!File.Exists(sourceImagePath))
                {
                    return false;
                }

                byte[] imageBytes;
                using (var sourceStream = new FileStream(sourceImagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    imageBytes = new byte[sourceStream.Length];
                    sourceStream.Read(imageBytes, 0, imageBytes.Length);
                }

                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        var exerciseId = GetExerciseId(connection, exerciseName);
                        if (exerciseId == null)
                        {
                            exerciseId = CreateExercise(connection, exerciseName);
                        }

                        var existingImageQuery = "SELECT COUNT(*) FROM ExerciseImages WHERE ExerciseId = @exerciseId";
                        using (var checkCommand = new SQLiteCommand(existingImageQuery, connection, transaction))
                        {
                            checkCommand.Parameters.AddWithValue("@exerciseId", exerciseId.Value);
                            var count = Convert.ToInt32(checkCommand.ExecuteScalar());

                            if (count > 0)
                            {
                                var updateQuery = "UPDATE ExerciseImages SET ImageData = @imageData, ImagePath = '' WHERE ExerciseId = @exerciseId";
                                using (var updateCommand = new SQLiteCommand(updateQuery, connection, transaction))
                                {
                                    updateCommand.Parameters.AddWithValue("@imageData", imageBytes);
                                    updateCommand.Parameters.AddWithValue("@exerciseId", exerciseId.Value);
                                    updateCommand.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                var insertQuery = @"
                                    INSERT INTO ExerciseImages (ExerciseId, ImageData, ImagePath, ImagePosition, IsPrimary, Description)
                                    VALUES (@exerciseId, @imageData, '', 'Front', 1, '')";

                                using (var insertCommand = new SQLiteCommand(insertQuery, connection, transaction))
                                {
                                    insertCommand.Parameters.AddWithValue("@exerciseId", exerciseId.Value);
                                    insertCommand.Parameters.AddWithValue("@imageData", imageBytes);
                                    insertCommand.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLiteExerciseImageDatabase] ImportImageForExercise error: {ex.Message}");
                return false;
            }
        }

        public bool AddOrUpdateExercise(string exerciseName, string imagePath, string[]? keywords = null, string[]? muscleGroups = null, string description = "")
        {
            try
            {
                if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    return ImportImageForExercise(exerciseName, imagePath);
                }

                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    var exerciseId = GetExerciseId(connection, exerciseName);
                    if (exerciseId == null)
                    {
                        CreateExercise(connection, exerciseName);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLiteExerciseImageDatabase] AddOrUpdateExercise error: {ex.Message}");
                return false;
            }
        }

        public bool UpdateExerciseDetails(string originalName, string newName, string description, string[] muscleGroups, string[] keywords, string source, string videoUrl)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        var exerciseId = GetExerciseId(connection, originalName) ?? GetExerciseId(connection, newName);
                        if (exerciseId == null)
                        {
                            exerciseId = CreateExercise(connection, string.IsNullOrWhiteSpace(newName) ? originalName : newName);
                        }

                        var updateParts = new List<string>();
                        var needsNameParameter = false;
                        var needsDescriptionParameter = false;
                        var needsInstructionsParameter = false;
                        var needsSourceParameter = false;
                        var needsVideoUrlParameter = false;
                        var needsPrimaryMuscleParameter = false;

                        if (_exerciseColumns.Contains("Name"))
                        {
                            updateParts.Add("Name = @newName");
                            needsNameParameter = true;
                        }

                        if (_exerciseColumns.Contains("SpanishName"))
                        {
                            updateParts.Add("SpanishName = @newName");
                            needsNameParameter = true;
                        }

                        if (_exerciseColumns.Contains("Description"))
                        {
                            updateParts.Add("Description = @description");
                            needsDescriptionParameter = true;
                        }

                        if (_exerciseColumns.Contains("Instructions"))
                        {
                            updateParts.Add("Instructions = @instructions");
                            needsInstructionsParameter = true;
                        }

                        if (_exerciseColumns.Contains("Source"))
                        {
                            updateParts.Add("Source = @source");
                            needsSourceParameter = true;
                        }

                        if (_exerciseColumns.Contains("VideoUrl"))
                        {
                            updateParts.Add("VideoUrl = @videoUrl");
                            needsVideoUrlParameter = true;
                        }

                        int? primaryMuscleId = null;
                        if (_exerciseColumns.Contains("PrimaryMuscleGroupId") && muscleGroups != null && muscleGroups.Length > 0)
                        {
                            primaryMuscleId = ResolvePrimaryMuscleGroupId(connection, muscleGroups[0]);
                            if (primaryMuscleId.HasValue)
                            {
                                updateParts.Add("PrimaryMuscleGroupId = @primaryMuscleGroupId");
                                needsPrimaryMuscleParameter = true;
                            }
                        }

                        if (updateParts.Count > 0)
                        {
                            var updateSql = $"UPDATE Exercises SET {string.Join(", ", updateParts)} WHERE Id = @id";
                            using (var command = new SQLiteCommand(updateSql, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@id", exerciseId.Value);

                                if (needsNameParameter)
                                {
                                    command.Parameters.AddWithValue("@newName", newName);
                                }

                                if (needsDescriptionParameter)
                                {
                                    command.Parameters.AddWithValue("@description", description ?? string.Empty);
                                }

                                if (needsInstructionsParameter)
                                {
                                    var joined = keywords == null || keywords.Length == 0
                                        ? string.Empty
                                        : string.Join(",", keywords.Select(k => k.Trim()).Where(k => k.Length > 0));
                                    command.Parameters.AddWithValue("@instructions", joined);
                                }

                                if (needsSourceParameter)
                                {
                                    command.Parameters.AddWithValue("@source", source ?? string.Empty);
                                }

                                if (needsVideoUrlParameter)
                                {
                                    command.Parameters.AddWithValue("@videoUrl", videoUrl ?? string.Empty);
                                }

                                if (needsPrimaryMuscleParameter && primaryMuscleId.HasValue)
                                {
                                    command.Parameters.AddWithValue("@primaryMuscleGroupId", primaryMuscleId.Value);
                                }

                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLiteExerciseImageDatabase] UpdateExerciseDetails error: {ex.Message}");
                return false;
            }
        }

        public bool RemoveExercise(string exerciseName)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        var exerciseId = GetExerciseId(connection, exerciseName);
                        if (exerciseId == null)
                        {
                            return false;
                        }

                        var deleteImagesQuery = "DELETE FROM ExerciseImages WHERE ExerciseId = @exerciseId";
                        using (var deleteImagesCommand = new SQLiteCommand(deleteImagesQuery, connection, transaction))
                        {
                            deleteImagesCommand.Parameters.AddWithValue("@exerciseId", exerciseId.Value);
                            deleteImagesCommand.ExecuteNonQuery();
                        }

                        if (_exerciseColumns.Contains("IsActive"))
                        {
                            var deactivateQuery = "UPDATE Exercises SET IsActive = 0 WHERE Id = @exerciseId";
                            using (var deactivateCommand = new SQLiteCommand(deactivateQuery, connection, transaction))
                            {
                                deactivateCommand.Parameters.AddWithValue("@exerciseId", exerciseId.Value);
                                deactivateCommand.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            var deleteExerciseQuery = "DELETE FROM Exercises WHERE Id = @exerciseId";
                            using (var deleteExerciseCommand = new SQLiteCommand(deleteExerciseQuery, connection, transaction))
                            {
                                deleteExerciseCommand.Parameters.AddWithValue("@exerciseId", exerciseId.Value);
                                deleteExerciseCommand.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLiteExerciseImageDatabase] RemoveExercise error: {ex.Message}");
                return false;
            }
        }

        private int? GetExerciseId(SQLiteConnection connection, string exerciseName)
        {
            if (string.IsNullOrWhiteSpace(exerciseName))
            {
                return null;
            }

            var query = "SELECT Id FROM Exercises WHERE Name = @name OR SpanishName = @name LIMIT 1";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@name", exerciseName);
                var result = command.ExecuteScalar();
                if (result == null)
                {
                    return null;
                }

                if (int.TryParse(result.ToString(), out var id))
                {
                    return id;
                }
            }

            return null;
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

        private int? ResolvePrimaryMuscleGroupId(SQLiteConnection connection, string? muscleGroupName)
        {
            if (string.IsNullOrWhiteSpace(muscleGroupName))
            {
                return null;
            }

            var query = "SELECT Id FROM MuscleGroups WHERE SpanishName = @name OR Name = @name ORDER BY Id LIMIT 1";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@name", muscleGroupName.Trim());
                var result = command.ExecuteScalar();
                if (result == null)
                {
                    return null;
                }

                if (int.TryParse(result.ToString(), out var id))
                {
                    return id;
                }
            }

            return null;
        }

        private static string? SafeGetString(SQLiteDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal))
                {
                    return null;
                }

                return reader.GetString(ordinal);
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }

        /// <summary>
        /// Actualiza el link de video de un ejercicio
        /// </summary>
        public bool UpdateVideoUrl(string exerciseName, string videoUrl)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = @"
                            UPDATE Exercises
                            SET VideoUrl = @videoUrl
                            WHERE Name = @name OR SpanishName = @name";

                        cmd.Parameters.AddWithValue("@videoUrl", videoUrl ?? string.Empty);
                        cmd.Parameters.AddWithValue("@name", exerciseName);

                        var rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLiteExerciseImageDatabase] Error actualizando VideoUrl: {ex.Message}");
                return false;
            }
        }

        private void EnsureVideoUrlColumnExists()
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"
                        CREATE TABLE IF NOT EXISTS Exercises (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT NOT NULL,
                            SpanishName TEXT,
                            Description TEXT,
                            Instructions TEXT,
                            PrimaryMuscleGroupId INTEGER,
                            EquipmentTypeId INTEGER,
                            DifficultyLevel INTEGER,
                            ExerciseType INTEGER,
                            IsActive INTEGER DEFAULT 1,
                            CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                            VideoUrl TEXT
                        );";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLiteExerciseImageDatabase] EnsureVideoUrlColumnExists error: {ex.Message}");
            }
        }
    }
}
