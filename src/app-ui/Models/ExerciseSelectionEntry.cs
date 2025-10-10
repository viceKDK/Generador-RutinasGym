using System;
using System.Collections.Generic;
using System.Linq;

namespace GymRoutineGenerator.UI.Models
{
    /// <summary>
    /// Represents an element stored in the manual selection panel of the gallery.
    /// </summary>
    public class ExerciseSelectionEntry
    {
        public ExerciseSelectionEntry(
            string exerciseId,
            string name,
            string imagePath,
            IReadOnlyList<string>? muscleGroups,
            DateTime addedAtUtc,
            string source)
        {
            ExerciseId = string.IsNullOrWhiteSpace(exerciseId)
                ? throw new ArgumentException("Exercise id cannot be empty.", nameof(exerciseId))
                : exerciseId;

            Name = name?.Trim() ?? string.Empty;
            ImagePath = imagePath?.Trim() ?? string.Empty;
            MuscleGroups = (muscleGroups ?? Array.Empty<string>()).Where(g => !string.IsNullOrWhiteSpace(g)).ToArray();
            AddedAtUtc = addedAtUtc;
            Source = source?.Trim() ?? string.Empty;
        }

        public string ExerciseId { get; }

        public string Name { get; }

        public string ImagePath { get; }

        public IReadOnlyList<string> MuscleGroups { get; }

        public DateTime AddedAtUtc { get; }

        public string Source { get; }

        public string DisplayName => string.IsNullOrWhiteSpace(Name) ? ExerciseId : Name;

        public static ExerciseSelectionEntry FromGalleryItem(ExerciseGalleryItem item, DateTime? timestampUtc = null)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return new ExerciseSelectionEntry(
                item.Id,
                item.DisplayName,
                item.ImagePath,
                item.MuscleGroups,
                timestampUtc ?? DateTime.UtcNow,
                item.Source);
        }
    }
}
