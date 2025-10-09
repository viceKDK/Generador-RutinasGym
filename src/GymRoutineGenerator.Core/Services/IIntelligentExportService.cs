using GymRoutineGenerator.Core.Models;

namespace GymRoutineGenerator.Core.Services
{
    public interface IIntelligentExportService
    {
        Task<ExportResult> ExportWithAIEnhancementsAsync(UserRoutine routine, ExportOptions options);
        Task<ExportResult> ExportProgressReportAsync(int userId, ProgressReportOptions options);
        Task<ExportResult> ExportCustomizedRoutineBookAsync(List<UserRoutine> routines, BookOptions options);
        Task<ExportResult> ExportWorkoutLogAsync(List<WorkoutSession> sessions, WorkoutLogOptions options);
        Task<byte[]> GenerateInstructionalPDFAsync(List<Exercise> exercises, InstructionalOptions options);
        Task<ExportResult> ExportNutritionGuideAsync(UserProfile profile, NutritionOptions options);
        Task<ExportResult> ExportComprehensiveReportAsync(int userId, ComprehensiveReportOptions options);
    }

    public class ExportOptions
    {
        public string OutputPath { get; set; } = string.Empty;
        public ExportFormat Format { get; set; } = ExportFormat.Word;
        public bool IncludeInstructions { get; set; } = true;
        public bool IncludeImages { get; set; } = true;
        public bool IncludeVideos { get; set; } = false;
        public bool IncludeAIExplanations { get; set; } = true;
        public bool IncludeProgressionSuggestions { get; set; } = true;
        public bool IncludeScientificReferences { get; set; } = false;
        public TemplateStyle TemplateStyle { get; set; } = TemplateStyle.Professional;
        public bool GenerateQRCode { get; set; } = false;
        public string Language { get; set; } = "es";
        public bool IncludeSafetyNotes { get; set; } = true;
        public bool IncludeAlternatives { get; set; } = true;
        public bool IncludePersonalization { get; set; } = true;
        public WatermarkOptions? Watermark { get; set; }
    }

    public class ProgressReportOptions : ExportOptions
    {
        public TimeRange TimeRange { get; set; } = new();
        public bool IncludeCharts { get; set; } = true;
        public bool IncludeAchievements { get; set; } = true;
        public bool IncludeGoals { get; set; } = true;
        public bool IncludeRecommendations { get; set; } = true;
        public List<string> MetricsToInclude { get; set; } = new();
        public bool IncludeComparisons { get; set; } = true;
        public bool IncludePredictions { get; set; } = true;
    }

    public class BookOptions : ExportOptions
    {
        public string BookTitle { get; set; } = "Mi Libro de Rutinas";
        public string AuthorName { get; set; } = string.Empty;
        public bool IncludeTableOfContents { get; set; } = true;
        public bool IncludeIndex { get; set; } = true;
        public bool IncludeGlossary { get; set; } = true;
        public bool IncludeIntroduction { get; set; } = true;
        public bool IncludeAppendix { get; set; } = true;
        public BookFormat BookFormat { get; set; } = BookFormat.Digital;
        public bool IncludeWorksheets { get; set; } = true;
        public bool IncludeProgressTrackers { get; set; } = true;
    }

    public class WorkoutLogOptions : ExportOptions
    {
        public bool IncludePerformanceMetrics { get; set; } = true;
        public bool IncludeRPEAnalysis { get; set; } = true;
        public bool IncludeProgressPhotos { get; set; } = false;
        public bool IncludeNotes { get; set; } = true;
        public bool IncludeEnvironmentalFactors { get; set; } = false;
        public LogFormat LogFormat { get; set; } = LogFormat.Detailed;
        public bool GroupByWeek { get; set; } = true;
        public bool IncludeSummaryStats { get; set; } = true;
    }

    public class InstructionalOptions
    {
        public bool IncludeStepByStep { get; set; } = true;
        public bool IncludeCommonMistakes { get; set; } = true;
        public bool IncludeSafetyTips { get; set; } = true;
        public bool IncludeVariations { get; set; } = true;
        public bool IncludeProgressions { get; set; } = true;
        public bool IncludeAnatomy { get; set; } = false;
        public InstructionalLevel Level { get; set; } = InstructionalLevel.Intermediate;
        public bool IncludeQRCodes { get; set; } = true;
        public bool IncludePrintableVersion { get; set; } = true;
    }

    public class NutritionOptions : ExportOptions
    {
        public bool IncludeMealPlans { get; set; } = true;
        public bool IncludeRecipes { get; set; } = true;
        public bool IncludeSupplements { get; set; } = false;
        public bool IncludeHydrationGuidance { get; set; } = true;
        public bool IncludeTimingAdvice { get; set; } = true;
        public DietaryRestrictions DietaryRestrictions { get; set; } = new();
        public NutritionGoal Goal { get; set; } = NutritionGoal.Maintenance;
        public bool IncludeShoppingLists { get; set; } = true;
    }

    public class ComprehensiveReportOptions : ExportOptions
    {
        public bool IncludeRoutineHistory { get; set; } = true;
        public bool IncludeProgressAnalysis { get; set; } = true;
        public bool IncludeGoalAssessment { get; set; } = true;
        public bool IncludeRecommendations { get; set; } = true;
        public bool IncludeNutritionGuidance { get; set; } = false;
        public bool IncludeRecoveryAdvice { get; set; } = true;
        public bool IncludeMotivationalContent { get; set; } = true;
        public bool IncludeEducationalContent { get; set; } = true;
        public ReportDepth Depth { get; set; } = ReportDepth.Comprehensive;
        public bool IncludeExecutiveSummary { get; set; } = true;
    }

    public class ExportResult
    {
        public bool Success { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public byte[]? FileData { get; set; }
        public string FileName { get; set; } = string.Empty;
        public ExportFormat Format { get; set; }
        public long FileSizeBytes { get; set; }
        public TimeSpan GenerationTime { get; set; }
        public List<string> Warnings { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public ExportMetadata Metadata { get; set; } = new();
        public string ShareableLink { get; set; } = string.Empty;
        public QRCodeData? QRCode { get; set; }
    }

    public class ExportMetadata
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0";
        public int PageCount { get; set; }
        public int ImageCount { get; set; }
        public int ExerciseCount { get; set; }
        public Dictionary<string, object> CustomProperties { get; set; } = new();
        public List<string> AIGeneratedSections { get; set; } = new();
        public string TemplateUsed { get; set; } = string.Empty;
    }

    public class AIEnhancedContent
    {
        public string ExecutiveSummary { get; set; } = string.Empty;
        public List<AIExplanation> ExerciseExplanations { get; set; } = new();
        public List<AIInsight> ScientificInsights { get; set; } = new();
        public string ProgressionRationale { get; set; } = string.Empty;
        public List<PersonalizedTip> PersonalizedTips { get; set; } = new();
        public string SafetyAssessment { get; set; } = string.Empty;
        public string MotivationalMessage { get; set; } = string.Empty;
        public List<AIRecommendation> Recommendations { get; set; } = new();
    }

    public class AIExplanation
    {
        public Exercise Exercise { get; set; } = new();
        public string WhyIncluded { get; set; } = string.Empty;
        public string ScientificRationale { get; set; } = string.Empty;
        public List<string> Benefits { get; set; } = new();
        public List<string> TechniqueKeys { get; set; } = new();
        public string ProgressionPath { get; set; } = string.Empty;
        public List<string> CommonMistakes { get; set; } = new();
        public string SafetyNotes { get; set; } = string.Empty;
    }

    public class AIInsight
    {
        public string Topic { get; set; } = string.Empty;
        public string Insight { get; set; } = string.Empty;
        public string Evidence { get; set; } = string.Empty;
        public List<string> Applications { get; set; } = new();
        public string Source { get; set; } = string.Empty;
        public ReliabilityLevel Reliability { get; set; }
    }

    public class PersonalizedTip
    {
        public string Category { get; set; } = string.Empty;
        public string Tip { get; set; } = string.Empty;
        public string Reasoning { get; set; } = string.Empty;
        public string Implementation { get; set; } = string.Empty;
        public Priority Priority { get; set; }
        public List<string> ExpectedBenefits { get; set; } = new();
    }

    public class AIRecommendation
    {
        public string Type { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public string Justification { get; set; } = string.Empty;
        public string Timeline { get; set; } = string.Empty;
        public float ConfidenceLevel { get; set; }
        public List<string> ActionSteps { get; set; } = new();
    }

    public class WatermarkOptions
    {
        public string Text { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public WatermarkPosition Position { get; set; } = WatermarkPosition.BottomRight;
        public float Opacity { get; set; } = 0.5f;
        public string FontFamily { get; set; } = "Arial";
        public int FontSize { get; set; } = 12;
        public string Color { get; set; } = "#808080";
    }

    public class DietaryRestrictions
    {
        public bool Vegetarian { get; set; }
        public bool Vegan { get; set; }
        public bool GlutenFree { get; set; }
        public bool DairyFree { get; set; }
        public bool NutFree { get; set; }
        public List<string> Allergies { get; set; } = new();
        public List<string> Dislikes { get; set; } = new();
        public List<string> CustomRestrictions { get; set; } = new();
    }

    public class QRCodeData
    {
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public string URL { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public enum ExportFormat
    {
        Word,
        PDF,
        HTML,
        JSON,
        Excel,
        PowerPoint,
        ePub,
        Text
    }

    public enum TemplateStyle
    {
        Professional,
        Modern,
        Minimalist,
        Colorful,
        Sport,
        Medical,
        Custom
    }

    public enum BookFormat
    {
        Digital,
        Print,
        EBook,
        Interactive
    }

    public enum LogFormat
    {
        Simple,
        Detailed,
        Analytics,
        Visual
    }

    public enum InstructionalLevel
    {
        Beginner,
        Intermediate,
        Advanced,
        Expert
    }

    public enum NutritionGoal
    {
        WeightLoss,
        WeightGain,
        Maintenance,
        MuscleGain,
        Performance,
        Health
    }

    public enum ReportDepth
    {
        Summary,
        Standard,
        Detailed,
        Comprehensive,
        Executive
    }

    public enum WatermarkPosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    public enum ReliabilityLevel
    {
        Low,
        Medium,
        High,
        Peer_Reviewed,
        Meta_Analysis
    }
}