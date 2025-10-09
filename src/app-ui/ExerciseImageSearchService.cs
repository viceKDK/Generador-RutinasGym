using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GymRoutineGenerator.UI
{
    /// <summary>
    /// Servicio para buscar ejercicios con imagenes
    /// Prioridad: 1) BD Principal SQLite, 2) BD Secundaria, 3) Carpeta docs/ejercicios
    /// </summary>
    public class ExerciseImageSearchService
    {
        private readonly SQLiteExerciseImageDatabase _database;
        private readonly SecondaryExerciseDatabase _secondaryDatabase;
        private readonly string _exerciseFolderPath;
        private Dictionary<string, string> _exerciseImageCache;

        public ExerciseImageSearchService()
        {
            _database = new SQLiteExerciseImageDatabase();
            _secondaryDatabase = new SecondaryExerciseDatabase();

            // Ruta a la carpeta de ejercicios
            _exerciseFolderPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..", "..",
                "docs", "ejercicios"
            );
            _exerciseFolderPath = Path.GetFullPath(_exerciseFolderPath);

            _exerciseImageCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            BuildExerciseImageCache();
        }

        /// <summary>
        /// Construye cache de imagenes desde docs/ejercicios
        /// </summary>
        private void BuildExerciseImageCache()
        {
            if (!Directory.Exists(_exerciseFolderPath))
            {
                return;
            }

            try
            {
                // Recorrer todas las carpetas de grupos musculares
                var muscleGroupFolders = Directory.GetDirectories(_exerciseFolderPath);

                foreach (var folder in muscleGroupFolders)
                {
                    // Recorrer ejercicios dentro de cada grupo
                    var exerciseFolders = Directory.GetDirectories(folder);

                    foreach (var exerciseFolder in exerciseFolders)
                    {
                        var exerciseName = Path.GetFileName(exerciseFolder);

                        // Buscar la primera imagen en la carpeta
                        var imageFiles = Directory.GetFiles(exerciseFolder, "*.*")
                            .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                       f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                       f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                       f.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                                       f.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
                            .OrderBy(f => f)
                            .ToList();

                        if (imageFiles.Any())
                        {
                            _exerciseImageCache[exerciseName] = imageFiles.First();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error building exercise cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Busca ejercicio con imagen
        /// Prioridad: 1) BD Principal SQLite, 2) BD Secundaria, 3) Cache de docs/ejercicios
        /// </summary>
        public ExerciseWithImage FindExerciseWithImage(string exerciseName)
        {
            System.Diagnostics.Debug.WriteLine($"Buscando ejercicio: '{exerciseName}'");

            // 1. Intentar desde base de datos principal
            var dbExercise = _database.FindExerciseImage(exerciseName);
            if (dbExercise != null && dbExercise.ImageData != null && dbExercise.ImageData.Length > 0)
            {
                System.Diagnostics.Debug.WriteLine($"  ✓ Encontrado en BD Principal");
                return new ExerciseWithImage
                {
                    Name = exerciseName,
                    ImageData = dbExercise.ImageData,
                    ImagePath = dbExercise.ImagePath,
                    Source = "BD Principal",
                    Description = dbExercise.Description,
                    MuscleGroups = dbExercise.MuscleGroups
                };
            }

            // 2. Intentar desde base de datos secundaria
            var secondaryExercise = _secondaryDatabase.FindExerciseImage(exerciseName);
            if (secondaryExercise != null && !string.IsNullOrWhiteSpace(secondaryExercise.ImagePath))
            {
                try
                {
                    var imageData = File.ReadAllBytes(secondaryExercise.ImagePath);
                    System.Diagnostics.Debug.WriteLine($"  ✓ Encontrado en BD Secundaria: {secondaryExercise.Source}");
                    return new ExerciseWithImage
                    {
                        Name = secondaryExercise.Name,
                        ImageData = imageData,
                        ImagePath = secondaryExercise.ImagePath,
                        Source = secondaryExercise.Source,
                        Description = "",
                        MuscleGroups = new string[0]
                    };
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"  ✗ Error leyendo imagen de BD secundaria: {ex.Message}");
                }
            }

            // 3. Intentar desde cache de docs/ejercicios
            if (_exerciseImageCache.TryGetValue(exerciseName, out var imagePath))
            {
                try
                {
                    var imageData = File.ReadAllBytes(imagePath);
                    return new ExerciseWithImage
                    {
                        Name = exerciseName,
                        ImageData = imageData,
                        ImagePath = imagePath,
                        Source = "DocsFolder",
                        Description = "",
                        MuscleGroups = new string[0]
                    };
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading image from docs: {ex.Message}");
                }
            }

            // 3. Busqueda fuzzy en cache (coincidencia parcial)
            var partialMatch = _exerciseImageCache.Keys
                .FirstOrDefault(k => k.Contains(exerciseName, StringComparison.OrdinalIgnoreCase) ||
                                    exerciseName.Contains(k, StringComparison.OrdinalIgnoreCase));

            if (partialMatch != null)
            {
                try
                {
                    var imagePath2 = _exerciseImageCache[partialMatch];
                    var imageData = File.ReadAllBytes(imagePath2);
                    return new ExerciseWithImage
                    {
                        Name = partialMatch,
                        ImageData = imageData,
                        ImagePath = imagePath2,
                        Source = "DocsFolder (Partial Match)",
                        Description = "",
                        MuscleGroups = new string[0]
                    };
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading partial match image: {ex.Message}");
                }
            }

            return null;
        }

        /// <summary>
        /// Mapea nombres de grupos musculares en español a inglés
        /// </summary>
        private string MapMuscleGroupToEnglish(string muscleGroup)
        {
            var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Español -> Inglés
                { "Pecho", "Chest" },
                { "Espalda", "Back" },
                { "Piernas", "Legs" },
                { "Hombros", "Shoulders" },
                { "Bíceps", "Biceps" },
                { "Tríceps", "Triceps" },
                { "Glúteos", "Glutes" },
                { "Core", "Core" },
                { "Abdomen", "Abs" },
                { "Pantorrillas", "Calves" },
                { "Brazos", "Arms" },
                { "Antebrazos", "Forearms" },
                { "Cuerpo Completo", "Full Body" },
                // Ya en inglés (no cambiar)
                { "Chest", "Chest" },
                { "Back", "Back" },
                { "Legs", "Legs" },
                { "Shoulders", "Shoulders" },
                { "Biceps", "Biceps" },
                { "Triceps", "Triceps" },
                { "Glutes", "Glutes" },
                { "Abs", "Abs" },
                { "Calves", "Calves" },
                { "Arms", "Arms" },
                { "Forearms", "Forearms" },
                { "Full Body", "Full Body" }
            };

            return mapping.TryGetValue(muscleGroup, out var mapped) ? mapped : muscleGroup;
        }

        /// <summary>
        /// Obtiene todos los ejercicios disponibles por grupo muscular
        /// </summary>
        public List<ExerciseWithImage> GetExercisesByMuscleGroup(string muscleGroup)
        {
            var exercises = new List<ExerciseWithImage>();

            // Convertir a inglés si es necesario
            var muscleGroupEn = MapMuscleGroupToEnglish(muscleGroup);

            System.Diagnostics.Debug.WriteLine($"📋 GetExercisesByMuscleGroup: '{muscleGroup}' -> '{muscleGroupEn}'");

            // 1. Desde base de datos principal
            var allDbExercises = _database.GetAllExercises();
            System.Diagnostics.Debug.WriteLine($"   Total ejercicios en BD: {allDbExercises.Count}");

            var dbFiltered = allDbExercises
                .Where(e => e.MuscleGroups != null &&
                           (e.MuscleGroups.Any(mg => mg.Equals(muscleGroup, StringComparison.OrdinalIgnoreCase)) ||
                            e.MuscleGroups.Any(mg => mg.Equals(muscleGroupEn, StringComparison.OrdinalIgnoreCase))));

            foreach (var ex in dbFiltered)
            {
                exercises.Add(new ExerciseWithImage
                {
                    Name = ex.ExerciseName,
                    ImageData = ex.ImageData,
                    ImagePath = ex.ImagePath,
                    Source = "BD Principal",
                    Description = ex.Description,
                    MuscleGroups = ex.MuscleGroups
                });
            }

            System.Diagnostics.Debug.WriteLine($"   Ejercicios de BD Principal: {exercises.Count}");

            // 2. Desde base de datos secundaria si no hay suficientes
            if (exercises.Count < 5)
            {
                var secondaryExercises = _secondaryDatabase.GetExercisesByMuscleGroup(muscleGroupEn);
                System.Diagnostics.Debug.WriteLine($"   Ejercicios de BD Secundaria: {secondaryExercises.Count}");

                foreach (var ex in secondaryExercises.Take(10 - exercises.Count))
                {
                    try
                    {
                        var imageData = File.ReadAllBytes(ex.ImagePath);
                        exercises.Add(new ExerciseWithImage
                        {
                            Name = ex.Name,
                            ImageData = imageData,
                            ImagePath = ex.ImagePath,
                            Source = "BD Secundaria",
                            Description = "",
                            MuscleGroups = new[] { muscleGroup }
                        });
                    }
                    catch { }
                }
            }

            // 3. Desde docs/ejercicios si aún no hay suficientes
            if (exercises.Count < 5)
            {
                var muscleGroupFolder = Path.Combine(_exerciseFolderPath, muscleGroupEn);
                System.Diagnostics.Debug.WriteLine($"   Buscando en carpeta: {muscleGroupFolder}");

                if (Directory.Exists(muscleGroupFolder))
                {
                    var exerciseFolders = Directory.GetDirectories(muscleGroupFolder);
                    foreach (var folder in exerciseFolders.Take(10))
                    {
                        var exerciseName = Path.GetFileName(folder);
                        var imageFiles = Directory.GetFiles(folder, "*.*")
                            .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                       f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                       f.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                            .OrderBy(f => f)
                            .FirstOrDefault();

                        if (imageFiles != null)
                        {
                            try
                            {
                                var imageData = File.ReadAllBytes(imageFiles);
                                exercises.Add(new ExerciseWithImage
                                {
                                    Name = exerciseName,
                                    ImageData = imageData,
                                    ImagePath = imageFiles,
                                    Source = "DocsFolder",
                                    Description = "",
                                    MuscleGroups = new[] { muscleGroup }
                                });
                            }
                            catch { }
                        }
                    }
                }
            }

            return exercises;
        }

        /// <summary>
        /// Cuenta total de ejercicios disponibles con imagenes
        /// </summary>
        public int GetTotalExercisesWithImages()
        {
            var dbCount = _database.GetAllExercises()
                .Count(e => e.ImageData != null && e.ImageData.Length > 0);

            var docsCount = _exerciseImageCache.Count;

            return dbCount + docsCount;
        }

        /// <summary>
        /// Obtiene grupos musculares disponibles
        /// </summary>
        public List<string> GetAvailableMuscleGroups()
        {
            var groups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Desde BD
            var dbExercises = _database.GetAllExercises();
            foreach (var ex in dbExercises)
            {
                if (ex.MuscleGroups != null)
                {
                    foreach (var mg in ex.MuscleGroups)
                    {
                        groups.Add(mg);
                    }
                }
            }

            // Desde docs/ejercicios
            if (Directory.Exists(_exerciseFolderPath))
            {
                var folders = Directory.GetDirectories(_exerciseFolderPath);
                foreach (var folder in folders)
                {
                    groups.Add(Path.GetFileName(folder));
                }
            }

            return groups.OrderBy(g => g).ToList();
        }
    }

    /// <summary>
    /// Ejercicio con imagen asociada
    /// </summary>
    public class ExerciseWithImage
    {
        public string Name { get; set; }
        public byte[] ImageData { get; set; }
        public string ImagePath { get; set; }
        public string Source { get; set; }
        public string Description { get; set; }
        public string[] MuscleGroups { get; set; }
    }
}
