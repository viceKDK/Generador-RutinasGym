using System;

namespace GymRoutineGenerator.UI
{
    public class ExerciseImageInfo
    {
        public string ExerciseName { get; set; } = "";
        public string Name { get; set; } = "";  // Alias for compatibility
        public string ImagePath { get; set; } = "";
        public byte[]? ImageData { get; set; }
        public string[] Keywords { get; set; } = new string[0];
        public string[] MuscleGroups { get; set; } = new string[0];
        public string Description { get; set; } = "";
        public string Source { get; set; } = "";  // Source of the image (BD Principal, BD Secundaria, etc.)
        public DateTime DateAdded { get; set; } = DateTime.Now;
    }
}
