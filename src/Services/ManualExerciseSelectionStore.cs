using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using GymRoutineGenerator.Domain.Models;

namespace GymRoutineGenerator.Services
{
    public class ManualExerciseSelectionStore
    {
        private readonly string _storePath;
        private readonly List<ExerciseSelectionEntry> _currentSelection = new();

        public ManualExerciseSelectionStore(string storePath)
        {
            _storePath = storePath;
            // Cargar estado inicial si existe
            try
            {
                var loaded = Load();
                _currentSelection.Clear();
                _currentSelection.AddRange(loaded);
            }
            catch
            {
                // Ignorar errores de carga: iniciar con lista vacía
            }
        }

        public IReadOnlyList<ExerciseSelectionEntry> CurrentSelection => _currentSelection.AsReadOnly();

        public event EventHandler<ManualExerciseSelectionChangedEventArgs>? SelectionChanged;

        public List<ExerciseSelectionEntry> Load()
        {
            if (!File.Exists(_storePath)) return new List<ExerciseSelectionEntry>();
            var json = File.ReadAllText(_storePath);
            try
            {
                return JsonSerializer.Deserialize<List<ExerciseSelectionEntry>>(json) ?? new List<ExerciseSelectionEntry>();
            }
            catch
            {
                return new List<ExerciseSelectionEntry>();
            }
        }

        public void Save(IEnumerable<ExerciseSelectionEntry> entries)
        {
            var json = JsonSerializer.Serialize(entries);
            File.WriteAllText(_storePath, json);
        }

        /// <summary>
        /// Actualiza el snapshot interno (llamado desde la UI) y persiste el cambio,
        /// además notifica a los suscriptores.
        /// </summary>
        public void UpdateSelectionSnapshot(IEnumerable<ExerciseSelectionEntry> entries)
        {
            _currentSelection.Clear();
            _currentSelection.AddRange(entries ?? Enumerable.Empty<ExerciseSelectionEntry>());
            try
            {
                Save(_currentSelection);
            }
            catch
            {
                // Ignorar errores de persistencia para no romper la UI
            }

            SelectionChanged?.Invoke(this, new ManualExerciseSelectionChangedEventArgs(_currentSelection));
        }
    }

    public sealed class ManualExerciseSelectionChangedEventArgs : EventArgs
    {
        public ManualExerciseSelectionChangedEventArgs(IReadOnlyList<ExerciseSelectionEntry> items)
        {
            Items = items ?? Array.Empty<ExerciseSelectionEntry>();
            Count = Items.Count;
        }

        public IReadOnlyList<ExerciseSelectionEntry> Items { get; }
        public int Count { get; }
    }
}
