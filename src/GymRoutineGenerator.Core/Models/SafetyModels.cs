using GymRoutineGenerator.Core.Enums;

namespace GymRoutineGenerator.Core.Models
{
    public class SafetyValidationResult
    {
        public bool IsSafe { get; set; }
        public SafetyLevel SafetyLevel { get; set; } = SafetyLevel.Safe;
        public List<SafetyWarning> Warnings { get; set; } = new();
        public List<string> SafetyNotes { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
        public float RiskScore { get; set; }
        public List<string> Recommendations { get; set; } = new();
        public DateTime AssessmentDate { get; set; } = DateTime.UtcNow;
        public bool RequiresSupervision { get; set; } = false;
        public List<string> Contraindications { get; set; } = new();
    }

    public class SafetyWarning
    {
        public SafetyLevel Severity { get; set; }
        public SafetyLevel RiskLevel { get; set; } // Alias for Severity
        public string Message { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty; // Alias for Message
        public string Category { get; set; } = string.Empty;
        public string AffectedBodyPart { get; set; } = string.Empty;
        public List<string> Mitigation { get; set; } = new();
        public List<string> MitigationStrategies { get; set; } = new(); // Alias for Mitigation
        public bool RequiresAction { get; set; }
    }

    public class SafetyProfile
    {
        public int UserId { get; set; }
        public List<string> PhysicalLimitations { get; set; } = new();
        public List<string> InjuryHistory { get; set; } = new();
        public string ExperienceLevel { get; set; } = "Principiante";
        public int Age { get; set; }
        public List<string> Medications { get; set; } = new();
        public List<string> AllergiesAndConditions { get; set; } = new();
        public DateTime LastMedicalCheckup { get; set; }
        public bool RequiresMedicalClearance { get; set; }
        public List<string> PreferredEquipment { get; set; } = new();
        public SafetyLevel BaseRiskLevel { get; set; } = SafetyLevel.Safe;
    }

    public class ContraindicationResult
    {
        public bool HasContraindications { get; set; }
        public List<string> Contraindications { get; set; } = new();
        public List<string> IdentifiedContraindications { get; set; } = new(); // Alias for Contraindications
        public SafetyLevel RiskLevel { get; set; } = SafetyLevel.Safe;
        public List<string> Recommendations { get; set; } = new();
    }

    public class DifficultyAssessment
    {
        public SafetyLevel RiskLevel { get; set; } = SafetyLevel.Safe;
        public string Recommendation { get; set; } = string.Empty;
        public bool IsAppropriate { get; set; } = true;
        public List<string> Modifications { get; set; } = new();
    }

}