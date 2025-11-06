using System;
using System.Collections.Generic;
using System.Linq;

namespace GymRoutineGenerator.Domain.Models
{
    /// <summary>
    /// Lightweight DTO exposed to the gallery UI so it does not depend on the
    /// internal search metadata structures.
    /// </summary>
    public class ExerciseGalleryItem
    {
        public ExerciseGalleryItem(
            string id,
            string name,
            string? englishName,
            IReadOnlyList<string>? muscleGroups,
            string imagePath,
            IReadOnlyList<string>? keywords,
            string source)
        {
            Id = string.IsNullOrWhiteSpace(id)
                ? throw new ArgumentException("An exercise id is required.", nameof(id))
                : id;

            Name = name?.Trim() ?? string.Empty;
            EnglishName = englishName?.Trim();
            MuscleGroups = (muscleGroups ?? Array.Empty<string>()).Where(g => !string.IsNullOrWhiteSpace(g)).ToArray();
            ImagePath = imagePath?.Trim() ?? string.Empty;
            Keywords = (keywords ?? Array.Empty<string>()).Where(k => !string.IsNullOrWhiteSpace(k)).ToArray();
            Source = source?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Identifier used to track selection from the UI; defaults to the normalized name.
        /// </summary>
        public string Id { get; }

        public string Name { get; }

        public string? EnglishName { get; }

        public IReadOnlyList<string> MuscleGroups { get; }

        public string ImagePath { get; }

        public IReadOnlyList<string> Keywords { get; }

        public string Source { get; }

        public bool HasImage => !string.IsNullOrWhiteSpace(ImagePath);

        public string DisplayName => !string.IsNullOrWhiteSpace(Name)
            ? Name
            : !string.IsNullOrWhiteSpace(EnglishName) ? EnglishName! : Id;

        public override string ToString() => DisplayName;
    }
}
