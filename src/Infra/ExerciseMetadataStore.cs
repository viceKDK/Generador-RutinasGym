using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using GymRoutineGenerator.Domain;

namespace GymRoutineGenerator.Infrastructure
{
    public sealed class ExerciseMetadataStore
    {
        private readonly string _filePath;
        private readonly Dictionary<string, ExerciseMetadataRecord> _cache;
        private readonly object _lock = new object();

        public ExerciseMetadataStore(string baseDirectory)
        {
            var dataDirectory = Path.Combine(baseDirectory, "Data");
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }

            _filePath = Path.Combine(dataDirectory, "exercise-metadata.json");
            _cache = LoadFromDisk();
        }

        public ExerciseMetadataRecord? Get(string exerciseName)
        {
            if (string.IsNullOrWhiteSpace(exerciseName))
            {
                return null;
            }

            lock (_lock)
            {
                _cache.TryGetValue(exerciseName, out var record);
                return record?.Clone();
            }
        }

        public IReadOnlyCollection<ExerciseMetadataRecord> GetAll()
        {
            lock (_lock)
            {
                return _cache.Values.Select(record => record.Clone()).ToArray();
            }
        }

        public void Upsert(ExerciseMetadataRecord record, string? originalName = null)
        {
            if (record == null || string.IsNullOrWhiteSpace(record.Name))
            {
                return;
            }

            lock (_lock)
            {
                if (!string.IsNullOrWhiteSpace(originalName) && !originalName.Equals(record.Name, StringComparison.OrdinalIgnoreCase))
                {
                    _cache.Remove(originalName);
                }

                _cache[record.Name] = record.Clone();
                Persist();
            }
        }

        public void Delete(string exerciseName)
        {
            if (string.IsNullOrWhiteSpace(exerciseName))
            {
                return;
            }

            lock (_lock)
            {
                if (_cache.Remove(exerciseName))
                {
                    Persist();
                }
            }
        }

        private Dictionary<string, ExerciseMetadataRecord> LoadFromDisk()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    return new Dictionary<string, ExerciseMetadataRecord>(StringComparer.OrdinalIgnoreCase);
                }

                var json = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new Dictionary<string, ExerciseMetadataRecord>(StringComparer.OrdinalIgnoreCase);
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var records = JsonSerializer.Deserialize<List<ExerciseMetadataRecord>>(json, options);
                if (records == null)
                {
                    return new Dictionary<string, ExerciseMetadataRecord>(StringComparer.OrdinalIgnoreCase);
                }

                return records
                    .Where(r => !string.IsNullOrWhiteSpace(r.Name))
                    .ToDictionary(r => r.Name, r => r, StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                return new Dictionary<string, ExerciseMetadataRecord>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private void Persist()
        {
            try
            {
                var ordered = _cache.Values
                    .OrderBy(record => record.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(record => record.Clone())
                    .ToList();

                var json = JsonSerializer.Serialize(ordered, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(_filePath, json);
            }
            catch
            {
                // Swallow IO exceptions silently; the cache stays in memory.
            }
        }
    }

    public sealed class ExerciseMetadataRecord
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string[] Keywords { get; set; } = Array.Empty<string>();
        public string[] MuscleGroups { get; set; } = Array.Empty<string>();
        public string Source { get; set; } = string.Empty;

        public ExerciseMetadataRecord Clone()
        {
            return new ExerciseMetadataRecord
            {
                Name = Name,
                Description = Description,
                Keywords = Keywords?.ToArray() ?? Array.Empty<string>(),
                MuscleGroups = MuscleGroups?.ToArray() ?? Array.Empty<string>(),
                Source = Source
            };
        }
    }
}
