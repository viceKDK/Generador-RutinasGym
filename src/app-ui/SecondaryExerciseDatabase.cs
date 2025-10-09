using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace GymRoutineGenerator.UI
{
    /// <summary>
    /// Base de datos secundaria de ejercicios poblada desde docs/ejercicios
    /// Se usa como fallback cuando no se encuentra el ejercicio en la BD principal
    /// </summary>
    public class SecondaryExerciseDatabase
    {
        private readonly string _dbPath;
        private readonly string _docsEjerciciosPath;
        private readonly Dictionary<string, string> _muscleGroupMapping;

        public SecondaryExerciseDatabase()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _dbPath = Path.Combine(baseDir, "ejercicios_secundaria.db");

            // Buscar la carpeta docs/ejercicios
            _docsEjerciciosPath = FindDocsEjerciciosPath(baseDir);

            // Mapeo de nombres de carpetas a nombres de grupos musculares
            _muscleGroupMapping = InitializeMuscleGroupMapping();

            InitializeDatabase();
        }

        private string FindDocsEjerciciosPath(string startPath)
        {
            var current = new DirectoryInfo(startPath);

            // Buscar hacia arriba hasta 10 niveles
            for (int i = 0; i < 10 && current != null; i++)
            {
                var docsPath = Path.Combine(current.FullName, "docs", "ejercicios");
                if (Directory.Exists(docsPath))
                {
                    return docsPath;
                }
                current = current.Parent;
            }

            return "";
        }

        private Dictionary<string, string> InitializeMuscleGroupMapping()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Mapeo de carpetas en docs/ejercicios a nombres estándar
                { "Pecho", "Pecho" },
                { "Espalda", "Espalda" },
                { "Atrás", "Espalda" },
                { "Latissimus dorsi", "Espalda" },
                { "Piernas", "Piernas" },
                { "Cuadríceps", "Piernas" },
                { "Isquiotibiales", "Piernas" },
                { "Glúteos", "Glúteos" },
                { "Hombros", "Hombros" },
                { "Deltoide delantero", "Hombros" },
                { "Deltoide trasero", "Hombros" },
                { "Deltoides laterales", "Hombros" },
                { "Brazos", "Brazos" },
                { "Bíceps", "Brazos" },
                { "Tríceps", "Brazos" },
                { "Antebrazos", "Brazos" },
                { "Abdominales", "Core" },
                { "Core", "Core" },
                { "Cintura", "Core" },
                { "Cardio", "Cardio" },
                { "Pantorrillas", "Pantorrillas" },
                { "Cuello", "Cuello" },
                { "Caderas", "Caderas" },
                { "Columna vertebral", "Espalda" }
            };
        }

        private void InitializeDatabase()
        {
            bool dbExists = File.Exists(_dbPath);

            using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            // Crear tabla si no existe
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Ejercicios (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Nombre TEXT NOT NULL,
                        GrupoMuscular TEXT NOT NULL,
                        RutaImagen TEXT,
                        LinkVideo TEXT,
                        Descripcion TEXT,
                        UNIQUE(Nombre, GrupoMuscular)
                    )";
                cmd.ExecuteNonQuery();
            }

            // Si la BD es nueva o está vacía, poblarla
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM Ejercicios";
                var count = Convert.ToInt32(cmd.ExecuteScalar());

                if (count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("BD secundaria vacía. Poblando desde docs/ejercicios...");
                    PopulateFromDocsEjercicios(connection);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"BD secundaria ya poblada con {count} ejercicios");
                }
            }
        }

        private void PopulateFromDocsEjercicios(SQLiteConnection connection)
        {
            if (string.IsNullOrEmpty(_docsEjerciciosPath) || !Directory.Exists(_docsEjerciciosPath))
            {
                System.Diagnostics.Debug.WriteLine($"WARN: No se encontró docs/ejercicios en: {_docsEjerciciosPath}");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Escaneando: {_docsEjerciciosPath}");

            int ejerciciosAgregados = 0;

            // Escanear carpetas de grupos musculares
            var muscleGroupDirs = Directory.GetDirectories(_docsEjerciciosPath);

            foreach (var muscleGroupDir in muscleGroupDirs)
            {
                var muscleGroupName = Path.GetFileName(muscleGroupDir);

                // Obtener nombre estándar del grupo muscular
                if (!_muscleGroupMapping.TryGetValue(muscleGroupName, out var standardMuscleGroup))
                {
                    standardMuscleGroup = muscleGroupName; // Usar el nombre original si no hay mapeo
                }

                // Escanear carpetas de ejercicios dentro del grupo muscular
                var exerciseDirs = Directory.GetDirectories(muscleGroupDir);

                foreach (var exerciseDir in exerciseDirs)
                {
                    var exerciseName = Path.GetFileName(exerciseDir);

                    // Buscar la primera imagen en la carpeta
                    var imageFiles = Directory.GetFiles(exerciseDir, "*.*")
                        .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                   f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                   f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                   f.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
                        .ToArray();

                    if (imageFiles.Length > 0)
                    {
                        var imagePath = imageFiles[0];

                        // Insertar en BD
                        using var cmd = connection.CreateCommand();
                        cmd.CommandText = @"
                            INSERT OR IGNORE INTO Ejercicios (Nombre, GrupoMuscular, RutaImagen)
                            VALUES (@nombre, @grupo, @imagen)";

                        cmd.Parameters.AddWithValue("@nombre", exerciseName);
                        cmd.Parameters.AddWithValue("@grupo", standardMuscleGroup);
                        cmd.Parameters.AddWithValue("@imagen", imagePath);

                        if (cmd.ExecuteNonQuery() > 0)
                        {
                            ejerciciosAgregados++;
                        }
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"✓ BD secundaria poblada con {ejerciciosAgregados} ejercicios");
        }

        /// <summary>
        /// Busca un ejercicio por nombre en la BD secundaria
        /// </summary>
        public ExerciseImageInfo? FindExerciseImage(string exerciseName)
        {
            if (string.IsNullOrWhiteSpace(exerciseName))
                return null;

            try
            {
                using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
                connection.Open();

                // Búsqueda exacta primero
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Nombre, GrupoMuscular, RutaImagen
                        FROM Ejercicios
                        WHERE Nombre = @nombre
                        COLLATE NOCASE
                        LIMIT 1";

                    cmd.Parameters.AddWithValue("@nombre", exerciseName);

                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        var imagePath = reader.GetString(2);
                        if (File.Exists(imagePath))
                        {
                            return new ExerciseImageInfo
                            {
                                Name = reader.GetString(0),
                                ImagePath = imagePath,
                                Source = "BD Secundaria"
                            };
                        }
                    }
                }

                // Búsqueda parcial (LIKE)
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Nombre, GrupoMuscular, RutaImagen
                        FROM Ejercicios
                        WHERE Nombre LIKE @nombre
                        COLLATE NOCASE
                        LIMIT 1";

                    cmd.Parameters.AddWithValue("@nombre", $"%{exerciseName}%");

                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        var imagePath = reader.GetString(2);
                        if (File.Exists(imagePath))
                        {
                            return new ExerciseImageInfo
                            {
                                Name = reader.GetString(0),
                                ImagePath = imagePath,
                                Source = "BD Secundaria (coincidencia parcial)"
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en BD secundaria: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Obtiene todos los ejercicios de un grupo muscular
        /// </summary>
        public List<ExerciseImageInfo> GetExercisesByMuscleGroup(string muscleGroup)
        {
            var exercises = new List<ExerciseImageInfo>();

            if (string.IsNullOrWhiteSpace(muscleGroup))
                return exercises;

            try
            {
                using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
                connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    SELECT Nombre, GrupoMuscular, RutaImagen
                    FROM Ejercicios
                    WHERE GrupoMuscular = @grupo
                    COLLATE NOCASE";

                cmd.Parameters.AddWithValue("@grupo", muscleGroup);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var imagePath = reader.GetString(2);
                    if (File.Exists(imagePath))
                    {
                        exercises.Add(new ExerciseImageInfo
                        {
                            Name = reader.GetString(0),
                            ImagePath = imagePath,
                            Source = "BD Secundaria"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo ejercicios por grupo: {ex.Message}");
            }

            return exercises;
        }

        /// <summary>
        /// Obtiene estadísticas de la BD secundaria
        /// </summary>
        public (int totalExercicios, int gruposMusculares) GetStatistics()
        {
            try
            {
                using var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
                connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    SELECT
                        COUNT(*) as Total,
                        COUNT(DISTINCT GrupoMuscular) as Grupos
                    FROM Ejercicios";

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return (reader.GetInt32(0), reader.GetInt32(1));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo estadísticas: {ex.Message}");
            }

            return (0, 0);
        }
    }
}
