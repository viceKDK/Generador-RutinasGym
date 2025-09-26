using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Drawing;

namespace GymRoutineGenerator.UI
{
    public class ExerciseImageDatabase
    {
        private readonly string _databasePath;
        private readonly string _imagesDirectory;
        private Dictionary<string, ExerciseImageInfo> _exerciseImages;

        public ExerciseImageDatabase()
        {
            _databasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "exercise_images.json");
            _imagesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Exercises");
            _exerciseImages = new Dictionary<string, ExerciseImageInfo>();

            // Crear directorios si no existen
            var dbDir = Path.GetDirectoryName(_databasePath);
            if (string.IsNullOrWhiteSpace(dbDir))
            {
                dbDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            }
            Directory.CreateDirectory(dbDir);
            Directory.CreateDirectory(_imagesDirectory);

            LoadDatabase();
        }

        public void LoadDatabase()
        {
            try
            {
                if (File.Exists(_databasePath))
                {
                    var json = File.ReadAllText(_databasePath);
                    var data = JsonSerializer.Deserialize<Dictionary<string, ExerciseImageInfo>>(json);
                    _exerciseImages = data ?? new Dictionary<string, ExerciseImageInfo>();
                }
                else
                {
                    // Crear base de datos inicial con algunos ejercicios comunes
                    CreateInitialDatabase();
                }
            }
            catch (Exception)
            {
                _exerciseImages = new Dictionary<string, ExerciseImageInfo>();
                CreateInitialDatabase();
            }
        }

        public void SaveDatabase()
        {
            try
            {
                var json = JsonSerializer.Serialize(_exerciseImages, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
                File.WriteAllText(_databasePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error guardando base de datos: {ex.Message}");
            }
        }

        private void CreateInitialDatabase()
        {
            // Ejercicios de ejemplo con datos iniciales
            var initialExercises = new Dictionary<string, ExerciseImageInfo>
            {
                ["press_banca"] = new ExerciseImageInfo
                {
                    ExerciseName = "Press de banca",
                    Keywords = new[] { "press", "banca", "pecho", "pectoral", "bench press" },
                    ImagePath = "",
                    MuscleGroups = new[] { "Pecho", "Trceps", "Hombros" },
                    Description = "Ejercicio fundamental para el desarrollo del pecho"
                },
                ["sentadillas"] = new ExerciseImageInfo
                {
                    ExerciseName = "Sentadillas",
                    Keywords = new[] { "sentadilla", "squat", "piernas", "cudriceps", "glteos" },
                    ImagePath = "",
                    MuscleGroups = new[] { "Cudriceps", "Glteos", "Core" },
                    Description = "Ejercicio base para el tren inferior"
                },
                ["peso_muerto"] = new ExerciseImageInfo
                {
                    ExerciseName = "Peso muerto",
                    Keywords = new[] { "peso muerto", "deadlift", "espalda", "glteos", "isquiotibiales" },
                    ImagePath = "",
                    MuscleGroups = new[] { "Espalda", "Glteos", "Isquiotibiales" },
                    Description = "Ejercicio complejo que trabaja toda la cadena posterior"
                },
                ["dominadas"] = new ExerciseImageInfo
                {
                    ExerciseName = "Dominadas",
                    Keywords = new[] { "dominada", "pull up", "espalda", "bceps", "dorsal" },
                    ImagePath = "",
                    MuscleGroups = new[] { "Espalda", "Bceps", "Core" },
                    Description = "Ejercicio de traccin vertical para espalda"
                },
                ["press_militar"] = new ExerciseImageInfo
                {
                    ExerciseName = "Press militar",
                    Keywords = new[] { "press militar", "shoulder press", "hombros", "deltoides" },
                    ImagePath = "",
                    MuscleGroups = new[] { "Hombros", "Trceps", "Core" },
                    Description = "Ejercicio para desarrollo de hombros"
                },
                ["remo_barra"] = new ExerciseImageInfo
                {
                    ExerciseName = "Remo con barra",
                    Keywords = new[] { "remo", "barra", "espalda", "dorsal", "rowing" },
                    ImagePath = "",
                    MuscleGroups = new[] { "Espalda", "Bceps", "Core" },
                    Description = "Ejercicio de traccin horizontal para espalda"
                },
                ["curl_biceps"] = new ExerciseImageInfo
                {
                    ExerciseName = "Curl de bceps",
                    Keywords = new[] { "curl", "bceps", "brazos", "bicep curl" },
                    ImagePath = "",
                    MuscleGroups = new[] { "Bceps" },
                    Description = "Ejercicio de aislamiento para bceps"
                },
                ["extensiones_triceps"] = new ExerciseImageInfo
                {
                    ExerciseName = "Extensiones de trceps",
                    Keywords = new[] { "extensin", "trceps", "brazos", "tricep extension" },
                    ImagePath = "",
                    MuscleGroups = new[] { "Trceps" },
                    Description = "Ejercicio de aislamiento para trceps"
                },
                ["plancha"] = new ExerciseImageInfo
                {
                    ExerciseName = "Plancha",
                    Keywords = new[] { "plancha", "plank", "core", "abdomen", "isomtrico" },
                    ImagePath = "",
                    MuscleGroups = new[] { "Core", "Abdomen", "Espalda baja" },
                    Description = "Ejercicio isomtrico para fortalecimiento del core"
                },
                ["zancadas"] = new ExerciseImageInfo
                {
                    ExerciseName = "Zancadas",
                    Keywords = new[] { "zancada", "lunge", "piernas", "cudriceps", "glteos" },
                    ImagePath = "",
                    MuscleGroups = new[] { "Cudriceps", "Glteos", "Core" },
                    Description = "Ejercicio unilateral para piernas"
                },
                ["hip_thrust"] = new ExerciseImageInfo
                {
                    ExerciseName = "Hip thrust",
                    Keywords = new[] { "hip thrust", "glteos", "cadera", "puente" },
                    ImagePath = "",
                    MuscleGroups = new[] { "Glteos", "Isquiotibiales" },
                    Description = "Ejercicio especfico para glteos"
                },
                ["elevaciones_pantorrillas"] = new ExerciseImageInfo
                {
                    ExerciseName = "Elevaciones de pantorrillas",
                    Keywords = new[] { "pantorrilla", "gemelos", "calf raise", "elevacin" },
                    ImagePath = "",
                    MuscleGroups = new[] { "Pantorrillas", "Gemelos" },
                    Description = "Ejercicio para desarrollo de pantorrillas"
                }
            };

            _exerciseImages = initialExercises;
            SaveDatabase();
        }

        public ExerciseImageInfo? FindExerciseImage(string exerciseName)
        {
            // Bsqueda exacta por clave
            var key = GenerateKey(exerciseName);
            if (_exerciseImages.TryGetValue(key, out var exactMatch))
            {
                return exactMatch;
            }

            // Bsqueda por similitud en keywords
            var searchTerms = exerciseName.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var exercise in _exerciseImages.Values)
            {
                foreach (var keyword in exercise.Keywords)
                {
                    foreach (var term in searchTerms)
                    {
                        if (keyword.ToLower().Contains(term) || term.Contains(keyword.ToLower()))
                        {
                            return exercise;
                        }
                    }
                }
            }

            // Bsqueda por nombre del ejercicio
            foreach (var exercise in _exerciseImages.Values)
            {
                if (exercise.ExerciseName.ToLower().Contains(exerciseName.ToLower()) ||
                    exerciseName.ToLower().Contains(exercise.ExerciseName.ToLower()))
                {
                    return exercise;
                }
            }

            return null;
        }

        public void AddOrUpdateExercise(string exerciseName, string imagePath, string[] keywords = null, string[] muscleGroups = null, string description = "")
        {
            var key = GenerateKey(exerciseName);
            var imageInfo = new ExerciseImageInfo
            {
                ExerciseName = exerciseName,
                ImagePath = imagePath,
                Keywords = keywords ?? new[] { exerciseName.ToLower() },
                MuscleGroups = muscleGroups ?? new string[0],
                Description = description,
                DateAdded = DateTime.Now
            };

            _exerciseImages[key] = imageInfo;
            SaveDatabase();
        }

        public bool HasImageForExercise(string exerciseName)
        {
            var imageInfo = FindExerciseImage(exerciseName);
            return imageInfo != null && !string.IsNullOrEmpty(imageInfo.ImagePath) && File.Exists(imageInfo.ImagePath);
        }

        public string GetImagePath(string exerciseName)
        {
            var imageInfo = FindExerciseImage(exerciseName);
            if (imageInfo != null && !string.IsNullOrEmpty(imageInfo.ImagePath) && File.Exists(imageInfo.ImagePath))
            {
                return imageInfo.ImagePath;
            }
            return "";
        }

        public Image? GetExerciseImage(string exerciseName)
        {
            var imagePath = GetImagePath(exerciseName);
            if (!string.IsNullOrEmpty(imagePath))
            {
                try
                {
                    return Image.FromFile(imagePath);
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public List<ExerciseImageInfo> GetAllExercises()
        {
            return _exerciseImages.Values.ToList();
        }

        public List<ExerciseImageInfo> GetExercisesWithoutImages()
        {
            return _exerciseImages.Values
                .Where(e => string.IsNullOrEmpty(e.ImagePath) || !File.Exists(e.ImagePath))
                .ToList();
        }

        public void RemoveExercise(string exerciseName)
        {
            var key = GenerateKey(exerciseName);
            if (_exerciseImages.ContainsKey(key))
            {
                _exerciseImages.Remove(key);
                SaveDatabase();
            }
        }

        private string GenerateKey(string exerciseName)
        {
            if (string.IsNullOrWhiteSpace(exerciseName))
                return string.Empty;

            // Normalize accents and diacritics safely (avoids empty oldValue errors)
            var text = exerciseName.Trim().ToLowerInvariant();
            text = text.Normalize(System.Text.NormalizationForm.FormD);

            var sb = new System.Text.StringBuilder(text.Length);
            foreach (var ch in text)
            {
                var category = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                if (category == System.Globalization.UnicodeCategory.NonSpacingMark)
                    continue; // strip diacritics

                char c = ch;
                if (c == 'ñ') c = 'n';

                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                }
                else if (char.IsWhiteSpace(c))
                {
                    sb.Append('_');
                }
                else if (c == '_' || c == '-')
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append('_');
                }
            }

            var key = sb.ToString();
            // collapse multiple underscores and trim
            key = System.Text.RegularExpressions.Regex.Replace(key, "_+", "_").Trim('_');
            return key;
        }

        public string GetImagesDirectory()
        {
            return _imagesDirectory;
        }

        public void ImportImageForExercise(string exerciseName, string sourceImagePath)
        {
            if (!File.Exists(sourceImagePath))
            {
                throw new FileNotFoundException("La imagen especificada no existe.");
            }

            var extension = Path.GetExtension(sourceImagePath);
            var key = GenerateKey(exerciseName);
            var targetFileName = $"{key}{extension}";
            var targetPath = Path.Combine(_imagesDirectory, targetFileName);

            // Copiar imagen al directorio de la aplicacin
            File.Copy(sourceImagePath, targetPath, true);

            // Actualizar base de datos
            var existingExercise = FindExerciseImage(exerciseName);
            if (existingExercise != null)
            {
                existingExercise.ImagePath = targetPath;
                existingExercise.DateAdded = DateTime.Now;
            }
            else
            {
                AddOrUpdateExercise(exerciseName, targetPath);
            }

            SaveDatabase();
        }
    }

    public class ExerciseImageInfo
    {
        public string ExerciseName { get; set; } = "";
        public string ImagePath { get; set; } = "";
        public string[] Keywords { get; set; } = new string[0];
        public string[] MuscleGroups { get; set; } = new string[0];
        public string Description { get; set; } = "";
        public DateTime DateAdded { get; set; } = DateTime.Now;
    }
}
