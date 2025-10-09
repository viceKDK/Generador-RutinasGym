using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GymRoutineGenerator.UI
{
    public class AutomaticImageFinder
    {
        private readonly string _exercisesBasePath;
        private readonly Dictionary<string, string> _imageCache;
        private readonly Dictionary<string, string> _exerciseNameMapping;

        public AutomaticImageFinder()
        {
            // Ruta base donde están las imágenes - intentar múltiples ubicaciones
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // Opción 1: Ruta relativa desde el ejecutable (Debug/Release)
            var option1 = Path.Combine(baseDir, "..", "..", "..", "..", "..", "docs", "ejercicios");

            // Opción 2: Ruta desde el directorio raíz del proyecto
            var option2 = Path.Combine(baseDir, "..", "..", "..", "..", "..", "..", "docs", "ejercicios");

            // Opción 3: Buscar hacia arriba hasta encontrar docs/ejercicios
            var option3 = FindDocsEjerciciosPath(baseDir);

            // Usar la primera que exista
            if (Directory.Exists(Path.GetFullPath(option1)))
            {
                _exercisesBasePath = Path.GetFullPath(option1);
            }
            else if (Directory.Exists(Path.GetFullPath(option2)))
            {
                _exercisesBasePath = Path.GetFullPath(option2);
            }
            else if (!string.IsNullOrEmpty(option3) && Directory.Exists(option3))
            {
                _exercisesBasePath = option3;
            }
            else
            {
                // Fallback: asumir que está en el directorio del proyecto
                _exercisesBasePath = Path.Combine(baseDir, "docs", "ejercicios");
            }

            _imageCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _exerciseNameMapping = InitializeExerciseMapping();

            // Pre-cargar cache si existe el directorio
            if (Directory.Exists(_exercisesBasePath))
            {
                LoadImageCache();
            }
        }

        private string? FindDocsEjerciciosPath(string startPath)
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

            return null;
        }

        private Dictionary<string, string> InitializeExerciseMapping()
        {
            // Mapeo de nombres comunes de ejercicios en español a nombres en carpetas
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Pecho
                { "Press de Banca", "Bench Press" },
                { "Press Banca", "Bench Press" },
                { "Press de pecho", "Chest Press" },
                { "Flexiones", "Push Up" },
                { "Aperturas", "Fly" },

                // Espalda
                { "Remo con Barra", "Barbell Row" },
                { "Remo", "Row" },
                { "Dominadas", "Pull Up" },
                { "Jalones", "Lat Pulldown" },
                { "Peso Muerto", "Deadlift" },

                // Piernas
                { "Sentadilla", "Squat" },
                { "Sentadillas", "Squat" },
                { "Prensa", "Leg Press" },
                { "Zancadas", "Lunge" },
                { "Curl Femoral", "Leg Curl" },
                { "Extensión de Cuádriceps", "Leg Extension" },
                { "Elevaciones de Pantorrilla", "Calf Raise" },

                // Hombros
                { "Press Militar", "Military Press" },
                { "Press de Hombro", "Shoulder Press" },
                { "Elevaciones Laterales", "Lateral Raise" },
                { "Elevaciones Frontales", "Front Raise" },
                { "Remo al Mentón", "Upright Row" },

                // Brazos
                { "Curl de Bíceps", "Bicep Curl" },
                { "Curl con Barra", "Barbell Curl" },
                { "Extensiones de Tríceps", "Tricep Extension" },
                { "Press Francés", "Skull Crusher" },
                { "Fondos", "Dip" },

                // Abdominales
                { "Abdominales", "Crunch" },
                { "Plancha", "Plank" },
                { "Elevación de Piernas", "Leg Raise" },
                { "Bicicleta", "Bicycle Crunch" }
            };
        }

        private void LoadImageCache()
        {
            try
            {
                // Buscar recursivamente todas las imágenes en docs/ejercicios
                var imageExtensions = new[] { "*.jpg", "*.jpeg", "*.png", "*.webp" };

                foreach (var extension in imageExtensions)
                {
                    var files = Directory.GetFiles(_exercisesBasePath, extension, SearchOption.AllDirectories);

                    foreach (var file in files)
                    {
                        // Obtener el nombre de la carpeta (que es el nombre del ejercicio)
                        var directoryName = Path.GetFileName(Path.GetDirectoryName(file));

                        if (!string.IsNullOrWhiteSpace(directoryName))
                        {
                            // Guardar en cache con el nombre de la carpeta como clave
                            if (!_imageCache.ContainsKey(directoryName))
                            {
                                _imageCache[directoryName] = file;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Si hay error, el cache queda vacío
            }
        }

        public string? FindImageForExercise(string exerciseName)
        {
            if (string.IsNullOrWhiteSpace(exerciseName))
                return null;

            // 1. Buscar en cache con nombre exacto
            if (_imageCache.TryGetValue(exerciseName, out var cachedPath))
            {
                if (File.Exists(cachedPath))
                    return cachedPath;
            }

            // 2. Buscar con nombre traducido/mapeado
            if (_exerciseNameMapping.TryGetValue(exerciseName, out var mappedName))
            {
                if (_imageCache.TryGetValue(mappedName, out var mappedPath))
                {
                    if (File.Exists(mappedPath))
                        return mappedPath;
                }
            }

            // 3. Búsqueda fuzzy - buscar por palabras clave
            var normalizedExerciseName = NormalizeString(exerciseName);
            var keywords = normalizedExerciseName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Buscar en cache por coincidencia parcial
            foreach (var cacheKey in _imageCache.Keys)
            {
                var normalizedKey = NormalizeString(cacheKey);

                // Si contiene todas las palabras clave importantes
                var matchCount = keywords.Count(keyword => normalizedKey.Contains(keyword, StringComparison.OrdinalIgnoreCase));

                if (matchCount >= Math.Min(keywords.Length, 2)) // Al menos 2 palabras coinciden
                {
                    var imagePath = _imageCache[cacheKey];
                    if (File.Exists(imagePath))
                        return imagePath;
                }
            }

            // 4. Búsqueda en tiempo real en el sistema de archivos
            if (Directory.Exists(_exercisesBasePath))
            {
                try
                {
                    var directories = Directory.GetDirectories(_exercisesBasePath, "*", SearchOption.AllDirectories);

                    foreach (var dir in directories)
                    {
                        var dirName = Path.GetFileName(dir);
                        var normalizedDirName = NormalizeString(dirName);

                        // Coincidencia por palabras clave
                        var matchCount = keywords.Count(keyword => normalizedDirName.Contains(keyword, StringComparison.OrdinalIgnoreCase));

                        if (matchCount >= Math.Min(keywords.Length, 2))
                        {
                            // Buscar primera imagen en esta carpeta
                            var images = Directory.GetFiles(dir, "*.*")
                                .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                           f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                           f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                           f.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
                                .FirstOrDefault();

                            if (!string.IsNullOrEmpty(images))
                            {
                                // Agregar al cache para futuras búsquedas
                                _imageCache[dirName] = images;
                                return images;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignorar errores de búsqueda
                }
            }

            // 5. No se encontró imagen
            return null;
        }

        private string NormalizeString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Remover acentos y convertir a minúsculas
            var normalized = input.ToLowerInvariant()
                .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
                .Replace("ñ", "n")
                .Replace("ü", "u");

            // Remover caracteres especiales
            normalized = Regex.Replace(normalized, @"[^a-z0-9\s]", " ");

            // Remover espacios múltiples
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

            return normalized;
        }

        public bool HasImages()
        {
            return _imageCache.Count > 0;
        }

        public int GetCachedImageCount()
        {
            return _imageCache.Count;
        }

        public List<string> GetAvailableExercises()
        {
            return _imageCache.Keys.ToList();
        }
    }
}
