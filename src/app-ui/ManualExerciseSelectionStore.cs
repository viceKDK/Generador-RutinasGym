using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GymRoutineGenerator.UI.Models;

namespace GymRoutineGenerator.UI
{
    /// <summary>
    /// Notifies listeners when the manual gallery selection changes and provides
    /// a read-only snapshot that other components in the app can consume.
    /// </summary>
    public sealed class ManualExerciseSelectionStore
    {
        private readonly object _syncRoot = new();
        private IReadOnlyList<ExerciseSelectionEntry> _selection = Array.Empty<ExerciseSelectionEntry>();

        public event EventHandler<ManualExerciseSelectionChangedEventArgs>? SelectionChanged;

        public IReadOnlyList<ExerciseSelectionEntry> CurrentSelection
        {
            get
            {
                lock (_syncRoot)
                {
                    return _selection;
                }
            }
        }

        public void UpdateSelectionSnapshot(IEnumerable<ExerciseSelectionEntry> entries)
        {
            if (entries == null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            var snapshot = entries
                .Select(entry => entry ?? throw new ArgumentException("Selection cannot contain null items.", nameof(entries)))
                .ToArray();

            var readOnly = new ReadOnlyCollection<ExerciseSelectionEntry>(snapshot);

            lock (_syncRoot)
            {
                _selection = readOnly;
            }

            SelectionChanged?.Invoke(this, new ManualExerciseSelectionChangedEventArgs(readOnly));
        }
    }

    public sealed class ManualExerciseSelectionChangedEventArgs : EventArgs
    {
        public ManualExerciseSelectionChangedEventArgs(IReadOnlyList<ExerciseSelectionEntry> selection)
        {
            Selection = selection ?? throw new ArgumentNullException(nameof(selection));
        }

        public IReadOnlyList<ExerciseSelectionEntry> Selection { get; }

        public int Count => Selection.Count;
    }
}
