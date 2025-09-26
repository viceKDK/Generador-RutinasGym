using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GymRoutineGenerator.Core.Models;

namespace GymRoutineGenerator.Core.Services.Documents
{
    public interface IWordDocumentService
    {
        Task<DocumentGenerationResult> GenerateRoutineDocumentAsync(RoutineDocumentRequest request, CancellationToken cancellationToken = default);
        Task<DocumentGenerationResult> GenerateDocumentWithTemplateAsync(RoutineDocumentRequest request, DocumentTemplate template, CancellationToken cancellationToken = default);
        Task<List<DocumentTemplate>> GetAvailableTemplatesAsync(CancellationToken cancellationToken = default);
        Task<DocumentTemplate> GetTemplateAsync(string templateId, CancellationToken cancellationToken = default);
        Task<DocumentPreview> PreviewDocumentAsync(RoutineDocumentRequest request, DocumentTemplate template, CancellationToken cancellationToken = default);
    }

    // Document-specific data models
    public class RoutineDocumentRequest
    {
        public string ClientName { get; set; } = string.Empty;
        public int ClientAge { get; set; }
        public string ClientGender { get; set; } = string.Empty;
        public string GymName { get; set; } = string.Empty;
        public string TrainerName { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        public StructuredRoutine Routine { get; set; } = new();
        public List<WeeklyProgram> WeeklyPrograms { get; set; } = new();
        public ClientGoals Goals { get; set; } = new();
        public List<DocumentSafetyNote> SafetyNotes { get; set; } = new();
        public DocumentSettings Settings { get; set; } = new();
        public Dictionary<string, object> CustomFields { get; set; } = new();
    }

    public class WeeklyProgram
    {
        public int WeekNumber { get; set; }
        public string WeekFocus { get; set; } = string.Empty;
        public List<DailyWorkout> DailyWorkouts { get; set; } = new();
        public string WeekNotes { get; set; } = string.Empty;
        public ProgressionGoals ProgressionGoals { get; set; } = new();
    }

    public class DailyWorkout
    {
        public int DayNumber { get; set; }
        public string DayName { get; set; } = string.Empty;
        public string WorkoutName { get; set; } = string.Empty;
        public List<string> TargetMuscleGroups { get; set; } = new();
        public WarmupSection Warmup { get; set; } = new();
        public List<ExerciseBlock> ExerciseBlocks { get; set; } = new();
        public CooldownSection Cooldown { get; set; } = new();
        public TimeSpan EstimatedDuration { get; set; }
        public IntensityLevel TargetIntensity { get; set; }
        public string DayNotes { get; set; } = string.Empty;
    }

    public class ExerciseBlock
    {
        public string BlockName { get; set; } = string.Empty;
        public string BlockPurpose { get; set; } = string.Empty;
        public List<DocumentExercise> Exercises { get; set; } = new();
        public int OrderInWorkout { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
    }

    public class DocumentExercise
    {
        public int OrderNumber { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public string SpanishName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> StepByStepInstructions { get; set; } = new();
        public List<string> TargetMuscles { get; set; } = new();
        public DocumentExerciseParameters Parameters { get; set; } = new();
        public List<string> SafetyTips { get; set; } = new();
        public List<string> TechniqueTips { get; set; } = new();
        public ExerciseImageInfo ImageInfo { get; set; } = new();
        public List<ExerciseVariation> Variations { get; set; } = new();
        public string Equipment { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = string.Empty;
    }

    public class DocumentExerciseParameters
    {
        public int Sets { get; set; }
        public string Reps { get; set; } = string.Empty; // "8-12" or "30 segundos"
        public string RestPeriod { get; set; } = string.Empty;
        public string Weight { get; set; } = string.Empty;
        public string Tempo { get; set; } = string.Empty;
        public string IntensityNote { get; set; } = string.Empty;
        public List<string> ProgressionNotes { get; set; } = new();
    }

    public class ExerciseImageInfo
    {
        public string ImagePath { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public string ImageFormat { get; set; } = "PNG"; // PNG, JPEG, etc.
        public ImageDisplaySettings DisplaySettings { get; set; } = new();
        public bool HasImage => !string.IsNullOrEmpty(ImagePath) || !string.IsNullOrEmpty(ImageUrl) || ImageData.Length > 0;
    }

    public class ImageDisplaySettings
    {
        public int Width { get; set; } = 200; // pixels
        public int Height { get; set; } = 150; // pixels
        public ImageAlignment Alignment { get; set; } = ImageAlignment.Right;
        public bool MaintainAspectRatio { get; set; } = true;
        public int Quality { get; set; } = 80; // 1-100
    }

    public class ExerciseVariation
    {
        public string VariationName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DifficultyModification { get; set; } = string.Empty; // "Más fácil", "Más difícil"
        public List<string> Instructions { get; set; } = new();
    }

    public class WarmupSection
    {
        public string Title { get; set; } = "Calentamiento";
        public TimeSpan Duration { get; set; }
        public List<WarmupExercise> Exercises { get; set; } = new();
        public string Purpose { get; set; } = string.Empty;
        public List<string> GeneralInstructions { get; set; } = new();
    }

    public class WarmupExercise
    {
        public string Name { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty; // "2 minutos" or "10 repeticiones"
        public string Instructions { get; set; } = string.Empty;
        public string Intensity { get; set; } = string.Empty;
    }

    public class CooldownSection
    {
        public string Title { get; set; } = "Enfriamiento";
        public TimeSpan Duration { get; set; }
        public List<CooldownExercise> Exercises { get; set; } = new();
        public string Purpose { get; set; } = string.Empty;
        public List<string> RecoveryTips { get; set; } = new();
    }

    public class CooldownExercise
    {
        public string Name { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public string BodyArea { get; set; } = string.Empty;
    }

    public class ClientGoals
    {
        public string PrimaryGoal { get; set; } = string.Empty;
        public List<string> SecondaryGoals { get; set; } = new();
        public string TargetTimeframe { get; set; } = string.Empty;
        public List<SpecificTarget> SpecificTargets { get; set; } = new();
        public string MotivationalMessage { get; set; } = string.Empty;
    }

    public class SpecificTarget
    {
        public string TargetName { get; set; } = string.Empty;
        public string CurrentValue { get; set; } = string.Empty;
        public string TargetValue { get; set; } = string.Empty;
        public string Timeframe { get; set; } = string.Empty;
    }

    public class ProgressionGoals
    {
        public List<string> WeeklyTargets { get; set; } = new();
        public Dictionary<string, string> ExerciseProgressions { get; set; } = new();
        public List<string> AssessmentCriteria { get; set; } = new();
    }

    public class DocumentSafetyNote
    {
        public string Category { get; set; } = string.Empty; // "General", "Cardíaco", "Articular", etc.
        public string Note { get; set; } = string.Empty;
        public SafetyPriority Priority { get; set; } = SafetyPriority.Medium;
        public List<string> WarningSignsToStop { get; set; } = new();
        public string RecommendedAction { get; set; } = string.Empty;
    }

    public class DocumentSettings
    {
        public string PreferredLanguage { get; set; } = "es-ES";
        public bool IncludeImages { get; set; } = true;
        public bool IncludeProgressTracking { get; set; } = true;
        public bool IncludeNutritionTips { get; set; } = false;
        public bool IncludeMotivationalQuotes { get; set; } = true;
        public DocumentDetailLevel DetailLevel { get; set; } = DocumentDetailLevel.Standard;
        public PageLayout PageLayout { get; set; } = new();
        public string OutputFileName { get; set; } = string.Empty;
        public string OutputDirectory { get; set; } = string.Empty;
        public bool AutoOpenAfterGeneration { get; set; } = true;
    }

    public class PageLayout
    {
        public PageSize PageSize { get; set; } = PageSize.A4;
        public PageOrientation Orientation { get; set; } = PageOrientation.Portrait;
        public Margins Margins { get; set; } = new();
        public bool IncludeHeader { get; set; } = true;
        public bool IncludeFooter { get; set; } = true;
        public bool IncludePageNumbers { get; set; } = true;
    }

    public class Margins
    {
        public double Top { get; set; } = 2.5; // cm
        public double Bottom { get; set; } = 2.0; // cm
        public double Left { get; set; } = 2.0; // cm
        public double Right { get; set; } = 2.0; // cm
    }

    public class DocumentTemplate
    {
        public string TemplateId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TemplateType Type { get; set; } = TemplateType.Standard;
        public TemplateStyle Style { get; set; } = new();
        public TemplateLayout Layout { get; set; } = new();
        public List<TemplateSection> Sections { get; set; } = new();
        public Dictionary<string, object> CustomSettings { get; set; } = new();
        public bool IsDefault { get; set; }
        public string PreviewImagePath { get; set; } = string.Empty;
    }

    public class TemplateStyle
    {
        public ColorScheme ColorScheme { get; set; } = new();
        public FontScheme FontScheme { get; set; } = new();
        public string LogoPath { get; set; } = string.Empty;
        public HeaderFooterStyle HeaderFooter { get; set; } = new();
        public bool UseWatermark { get; set; }
        public string WatermarkText { get; set; } = string.Empty;
        public bool IncludeImages { get; set; } = true;
        public int ImageQuality { get; set; } = 80; // 1-100 quality scale
        public int MaxImageWidth { get; set; } = 300; // pixels
        public int MaxImageHeight { get; set; } = 200; // pixels
    }

    public class ColorScheme
    {
        public string PrimaryColor { get; set; } = "#2E86AB"; // Blue
        public string SecondaryColor { get; set; } = "#A23B72"; // Pink
        public string AccentColor { get; set; } = "#F18F01"; // Orange
        public string TextColor { get; set; } = "#333333"; // Dark Gray
        public string BackgroundColor { get; set; } = "#FFFFFF"; // White
        public string HeaderColor { get; set; } = "#F8F9FA"; // Light Gray
    }

    public class FontScheme
    {
        public string HeaderFont { get; set; } = "Calibri";
        public string BodyFont { get; set; } = "Calibri";
        public string AccentFont { get; set; } = "Calibri Light";
        public Dictionary<string, int> FontSizes { get; set; } = new()
        {
            ["Title"] = 24,
            ["Heading1"] = 18,
            ["Heading2"] = 14,
            ["Body"] = 11,
            ["Caption"] = 9
        };
    }

    public class HeaderFooterStyle
    {
        public string HeaderText { get; set; } = string.Empty;
        public string FooterText { get; set; } = "Rutina generada por GymRoutine Generator";
        public bool IncludeLogo { get; set; } = true;
        public bool IncludeDate { get; set; } = true;
        public bool IncludePageNumbers { get; set; } = true;
        public HeaderFooterAlignment Alignment { get; set; } = HeaderFooterAlignment.Center;
    }

    public class TemplateLayout
    {
        public bool IncludeCoverPage { get; set; } = true;
        public bool IncludeTableOfContents { get; set; } = false;
        public SectionLayout WarmupLayout { get; set; } = new();
        public SectionLayout ExerciseLayout { get; set; } = new();
        public SectionLayout CooldownLayout { get; set; } = new();
        public bool IncludeProgressSection { get; set; } = true;
        public bool IncludeSafetySection { get; set; } = true;
    }

    public class SectionLayout
    {
        public bool ShowSectionTitle { get; set; } = true;
        public bool ShowImages { get; set; } = true;
        public bool ShowInstructions { get; set; } = true;
        public bool ShowParameters { get; set; } = true;
        public bool ShowSafetyTips { get; set; } = true;
        public bool ShowTechniqueTips { get; set; } = true;
        public bool ShowVariations { get; set; } = false;
        public ExerciseDisplayFormat DisplayFormat { get; set; } = ExerciseDisplayFormat.TableFormat;
        public int ExercisesPerPage { get; set; } = 0; // 0 = auto
    }

    public class TemplateSection
    {
        public string SectionId { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public bool IsRequired { get; set; } = true;
        public bool IsVisible { get; set; } = true;
        public int DisplayOrder { get; set; }
        public Dictionary<string, object> SectionSettings { get; set; } = new();
    }

    public class DocumentGenerationResult
    {
        public bool Success { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public TimeSpan GenerationTime { get; set; }
        public List<string> Warnings { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public DocumentMetadata Metadata { get; set; } = new();
        public GenerationStatistics Statistics { get; set; } = new();
    }

    public class DocumentMetadata
    {
        public string Title { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public List<string> Keywords { get; set; } = new();
        public string Comments { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        public string Version { get; set; } = "1.0";
    }

    public class GenerationStatistics
    {
        public int TotalPages { get; set; }
        public int TotalExercises { get; set; }
        public int TotalImages { get; set; }
        public int TotalSections { get; set; }
        public Dictionary<string, int> SectionCounts { get; set; } = new();
        public List<string> TemplatesUsed { get; set; } = new();
    }

    public class DocumentPreview
    {
        public string PreviewId { get; set; } = Guid.NewGuid().ToString();
        public List<PagePreview> Pages { get; set; } = new();
        public DocumentStructure Structure { get; set; } = new();
        public PreviewStatistics Statistics { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class PagePreview
    {
        public int PageNumber { get; set; }
        public string PageTitle { get; set; } = string.Empty;
        public List<string> ContentSummary { get; set; } = new();
        public string ThumbnailPath { get; set; } = string.Empty; // Optional
    }

    public class DocumentStructure
    {
        public List<DocumentSection> Sections { get; set; } = new();
        public int EstimatedPageCount { get; set; }
        public TimeSpan EstimatedReadingTime { get; set; }
    }

    public class DocumentSection
    {
        public string SectionName { get; set; } = string.Empty;
        public int StartPage { get; set; }
        public int PageCount { get; set; }
        public List<string> SubSections { get; set; } = new();
    }

    public class PreviewStatistics
    {
        public int TotalExercises { get; set; }
        public int ExercisesWithImages { get; set; }
        public int TotalInstructions { get; set; }
        public int SafetyNotesCount { get; set; }
        public Dictionary<string, int> ContentBreakdown { get; set; } = new();
    }

    // Enums
    public enum ImageAlignment
    {
        Left,
        Center,
        Right,
        Inline
    }

    public enum SafetyPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum DocumentDetailLevel
    {
        Basic,      // Solo ejercicios básicos
        Standard,   // Incluye instrucciones y tips
        Detailed,   // Incluye todo + variaciones
        Professional // Incluye análisis avanzado
    }

    public enum PageSize
    {
        A4,
        Letter,
        Legal,
        A3
    }

    public enum PageOrientation
    {
        Portrait,
        Landscape
    }

    public enum TemplateType
    {
        Basic,
        Standard,
        Professional,
        Gym,
        PersonalTrainer,
        Rehabilitation,
        Custom
    }

    public enum HeaderFooterAlignment
    {
        Left,
        Center,
        Right
    }

    public enum ExerciseDisplayFormat
    {
        ListFormat,      // Formato de lista simple
        TableFormat,     // Formato de tabla estructurada
        CardFormat,      // Formato de tarjetas con imágenes
        DetailedFormat   // Formato detallado con todo
    }
}