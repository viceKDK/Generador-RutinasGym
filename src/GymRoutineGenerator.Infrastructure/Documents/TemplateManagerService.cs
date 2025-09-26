using GymRoutineGenerator.Core.Services.Documents;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GymRoutineGenerator.Infrastructure.Documents;

public class TemplateManagerService : ITemplateManagerService
{
    private readonly ILogger<TemplateManagerService>? _logger;
    private readonly string _templateDirectory;

    public TemplateManagerService(ILogger<TemplateManagerService>? logger = null)
    {
        _logger = logger;
        _templateDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                        "GymRoutineGenerator", "Templates");

        EnsureTemplateDirectoryExists();
    }

    public async Task<List<DocumentTemplate>> GetAvailableTemplatesAsync()
    {
        try
        {
            var templates = new List<DocumentTemplate>();

            // Add built-in templates
            templates.AddRange(GetBuiltInTemplates());

            // Add custom templates from directory
            if (Directory.Exists(_templateDirectory))
            {
                var customTemplates = await LoadCustomTemplatesAsync();
                templates.AddRange(customTemplates);
            }

            return templates.OrderBy(t => t.Type).ThenBy(t => t.Name).ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading available templates");
            return GetBuiltInTemplates(); // Fallback to built-in templates
        }
    }

    public async Task<DocumentTemplate> GetTemplateAsync(string templateId)
    {
        var templates = await GetAvailableTemplatesAsync();
        var template = templates.FirstOrDefault(t => t.TemplateId == templateId);

        if (template == null)
        {
            _logger?.LogWarning($"Template {templateId} not found, using default");
            return GetDefaultTemplate();
        }

        return template;
    }

    public async Task<DocumentTemplate> SaveCustomTemplateAsync(DocumentTemplate template)
    {
        try
        {
            EnsureTemplateDirectoryExists();

            // Ensure unique ID
            if (string.IsNullOrEmpty(template.TemplateId))
            {
                template.TemplateId = Guid.NewGuid().ToString();
            }

            var fileName = $"{template.TemplateId}.json";
            var filePath = Path.Combine(_templateDirectory, fileName);

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var jsonString = JsonSerializer.Serialize(template, jsonOptions);
            await File.WriteAllTextAsync(filePath, jsonString);

            _logger?.LogInformation($"Custom template {template.Name} saved with ID {template.TemplateId}");
            return template;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error saving custom template {template.Name}");
            throw;
        }
    }

    public async Task<bool> DeleteCustomTemplateAsync(string templateId)
    {
        try
        {
            // Simulate async operation
            await Task.Delay(1);

            var fileName = $"{templateId}.json";
            var filePath = Path.Combine(_templateDirectory, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger?.LogInformation($"Custom template {templateId} deleted");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error deleting custom template {templateId}");
            return false;
        }
    }

    public DocumentTemplate CreateTemplateFromBase(TemplateType baseType, string name, string description = "")
    {
        var baseTemplate = GetBuiltInTemplates().First(t => t.Type == baseType);

        return new DocumentTemplate
        {
            TemplateId = Guid.NewGuid().ToString(),
            Name = name,
            Description = description,
            Type = TemplateType.Custom,
            Style = CloneTemplateStyle(baseTemplate.Style),
            Layout = CloneTemplateLayout(baseTemplate.Layout),
            IsDefault = false
        };
    }

    public async Task<DocumentPreview> PreviewTemplateAsync(DocumentTemplate template, RoutineDocumentRequest sampleRequest)
    {
        try
        {
            // Simulate async operation
            await Task.Delay(1);

            // Create a preview structure based on the template
            var preview = new DocumentPreview
            {
                PreviewId = Guid.NewGuid().ToString(),
                GeneratedAt = DateTime.UtcNow
            };

            // Generate page previews
            preview.Pages = GeneratePagePreviews(template, sampleRequest);

            // Generate document structure
            preview.Structure = GenerateDocumentStructure(template, sampleRequest);

            // Generate preview statistics
            preview.Statistics = GeneratePreviewStatistics(template, sampleRequest);

            return preview;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error generating template preview for {template.Name}");
            throw;
        }
    }

    private void EnsureTemplateDirectoryExists()
    {
        if (!Directory.Exists(_templateDirectory))
        {
            Directory.CreateDirectory(_templateDirectory);
            _logger?.LogInformation($"Template directory created: {_templateDirectory}");
        }
    }

    private List<DocumentTemplate> GetBuiltInTemplates()
    {
        return new List<DocumentTemplate>
        {
            CreateBasicTemplate(),
            CreateStandardTemplate(),
            CreateProfessionalTemplate(),
            CreateGymTemplate(),
            CreateRehabilitationTemplate()
        };
    }

    private DocumentTemplate CreateBasicTemplate()
    {
        return new DocumentTemplate
        {
            TemplateId = "basic",
            Name = "Básico",
            Description = "Plantilla simple para rutinas básicas",
            Type = TemplateType.Basic,
            IsDefault = true,
            Style = new TemplateStyle
            {
                ColorScheme = new ColorScheme
                {
                    PrimaryColor = "#2E86AB",
                    SecondaryColor = "#A23B72",
                    AccentColor = "#F18F01",
                    TextColor = "#333333",
                    BackgroundColor = "#FFFFFF"
                },
                FontScheme = new FontScheme
                {
                    HeaderFont = "Arial",
                    BodyFont = "Arial",
                    AccentFont = "Arial Black"
                },
                IncludeImages = false,
                HeaderFooter = new HeaderFooterStyle
                {
                    FooterText = "Rutina Básica - GymRoutine Generator"
                }
            },
            Layout = new TemplateLayout
            {
                IncludeCoverPage = true,
                IncludeTableOfContents = false,
                WarmupLayout = new SectionLayout { ShowImages = false },
                ExerciseLayout = new SectionLayout { ShowImages = false, DisplayFormat = ExerciseDisplayFormat.ListFormat },
                CooldownLayout = new SectionLayout { ShowImages = false }
            }
        };
    }

    private DocumentTemplate CreateStandardTemplate()
    {
        return new DocumentTemplate
        {
            TemplateId = "standard",
            Name = "Estándar",
            Description = "Plantilla estándar con imágenes y formato mejorado",
            Type = TemplateType.Standard,
            Style = new TemplateStyle
            {
                ColorScheme = new ColorScheme
                {
                    PrimaryColor = "#2E86AB",
                    SecondaryColor = "#A23B72",
                    AccentColor = "#F18F01",
                    TextColor = "#333333",
                    BackgroundColor = "#FFFFFF"
                },
                FontScheme = new FontScheme
                {
                    HeaderFont = "Calibri",
                    BodyFont = "Calibri",
                    AccentFont = "Calibri Light"
                },
                IncludeImages = true,
                HeaderFooter = new HeaderFooterStyle
                {
                    FooterText = "Rutina Personalizada - GymRoutine Generator",
                    IncludeDate = true
                }
            },
            Layout = new TemplateLayout
            {
                IncludeCoverPage = true,
                IncludeTableOfContents = false,
                WarmupLayout = new SectionLayout { ShowImages = true },
                ExerciseLayout = new SectionLayout { ShowImages = true, DisplayFormat = ExerciseDisplayFormat.TableFormat },
                CooldownLayout = new SectionLayout { ShowImages = true }
            }
        };
    }

    private DocumentTemplate CreateProfessionalTemplate()
    {
        return new DocumentTemplate
        {
            TemplateId = "professional",
            Name = "Profesional",
            Description = "Plantilla profesional con tabla de contenidos y formato avanzado",
            Type = TemplateType.Professional,
            Style = new TemplateStyle
            {
                ColorScheme = new ColorScheme
                {
                    PrimaryColor = "#1f4e79",
                    SecondaryColor = "#70ad47",
                    AccentColor = "#c55a11",
                    TextColor = "#1f1f1f",
                    BackgroundColor = "#FFFFFF",
                    HeaderColor = "#f2f2f2"
                },
                FontScheme = new FontScheme
                {
                    HeaderFont = "Calibri",
                    BodyFont = "Calibri",
                    AccentFont = "Calibri Light",
                    FontSizes = new Dictionary<string, int>
                    {
                        ["Title"] = 26,
                        ["Heading1"] = 20,
                        ["Heading2"] = 16,
                        ["Body"] = 12,
                        ["Caption"] = 10
                    }
                },
                IncludeImages = true,
                UseWatermark = true,
                WatermarkText = "CONFIDENCIAL",
                HeaderFooter = new HeaderFooterStyle
                {
                    HeaderText = "Plan de Entrenamiento Profesional",
                    FooterText = "© GymRoutine Generator - Entrenamiento Personalizado",
                    IncludeDate = true,
                    IncludePageNumbers = true
                }
            },
            Layout = new TemplateLayout
            {
                IncludeCoverPage = true,
                IncludeTableOfContents = true,
                IncludeProgressSection = true,
                IncludeSafetySection = true,
                WarmupLayout = new SectionLayout
                {
                    ShowImages = true,
                    ShowSafetyTips = true,
                    DisplayFormat = ExerciseDisplayFormat.DetailedFormat
                },
                ExerciseLayout = new SectionLayout
                {
                    ShowImages = true,
                    ShowInstructions = true,
                    ShowSafetyTips = true,
                    ShowTechniqueTips = true,
                    DisplayFormat = ExerciseDisplayFormat.DetailedFormat
                },
                CooldownLayout = new SectionLayout
                {
                    ShowImages = true,
                    ShowSafetyTips = true
                }
            }
        };
    }

    private DocumentTemplate CreateGymTemplate()
    {
        return new DocumentTemplate
        {
            TemplateId = "gym",
            Name = "Gimnasio",
            Description = "Plantilla especializada para gimnasios comerciales",
            Type = TemplateType.Gym,
            Style = new TemplateStyle
            {
                ColorScheme = new ColorScheme
                {
                    PrimaryColor = "#d32f2f",
                    SecondaryColor = "#1976d2",
                    AccentColor = "#ff9800",
                    TextColor = "#212121",
                    BackgroundColor = "#FFFFFF",
                    HeaderColor = "#fafafa"
                },
                FontScheme = new FontScheme
                {
                    HeaderFont = "Arial Black",
                    BodyFont = "Arial",
                    AccentFont = "Arial"
                },
                IncludeImages = true,
                HeaderFooter = new HeaderFooterStyle
                {
                    HeaderText = "PLAN DE ENTRENAMIENTO",
                    FooterText = "Tu Gimnasio - Resultados Garantizados",
                    IncludeLogo = true,
                    IncludeDate = true
                }
            },
            Layout = new TemplateLayout
            {
                IncludeCoverPage = true,
                IncludeTableOfContents = false,
                ExerciseLayout = new SectionLayout
                {
                    ShowImages = true,
                    DisplayFormat = ExerciseDisplayFormat.CardFormat
                }
            }
        };
    }

    private DocumentTemplate CreateRehabilitationTemplate()
    {
        return new DocumentTemplate
        {
            TemplateId = "rehabilitation",
            Name = "Rehabilitación",
            Description = "Plantilla especializada para rehabilitación y fisioterapia",
            Type = TemplateType.Rehabilitation,
            Style = new TemplateStyle
            {
                ColorScheme = new ColorScheme
                {
                    PrimaryColor = "#388e3c",
                    SecondaryColor = "#1976d2",
                    AccentColor = "#f57c00",
                    TextColor = "#2e2e2e",
                    BackgroundColor = "#FFFFFF"
                },
                FontScheme = new FontScheme
                {
                    HeaderFont = "Times New Roman",
                    BodyFont = "Times New Roman",
                    AccentFont = "Times New Roman"
                },
                IncludeImages = true,
                HeaderFooter = new HeaderFooterStyle
                {
                    HeaderText = "PLAN DE REHABILITACIÓN",
                    FooterText = "Centro de Rehabilitación - Cuidado Profesional"
                }
            },
            Layout = new TemplateLayout
            {
                IncludeCoverPage = true,
                IncludeSafetySection = true,
                WarmupLayout = new SectionLayout
                {
                    ShowImages = true,
                    ShowSafetyTips = true,
                    DisplayFormat = ExerciseDisplayFormat.DetailedFormat
                },
                ExerciseLayout = new SectionLayout
                {
                    ShowImages = true,
                    ShowInstructions = true,
                    ShowSafetyTips = true,
                    ShowTechniqueTips = true,
                    DisplayFormat = ExerciseDisplayFormat.DetailedFormat
                },
                CooldownLayout = new SectionLayout
                {
                    ShowImages = true,
                    ShowSafetyTips = true
                }
            }
        };
    }

    private async Task<List<DocumentTemplate>> LoadCustomTemplatesAsync()
    {
        var templates = new List<DocumentTemplate>();

        try
        {
            var templateFiles = Directory.GetFiles(_templateDirectory, "*.json");

            foreach (var file in templateFiles)
            {
                try
                {
                    var jsonContent = await File.ReadAllTextAsync(file);
                    var template = JsonSerializer.Deserialize<DocumentTemplate>(jsonContent,
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                    if (template != null)
                    {
                        templates.Add(template);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, $"Failed to load custom template from {file}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading custom templates directory");
        }

        return templates;
    }

    private DocumentTemplate GetDefaultTemplate()
    {
        return CreateStandardTemplate();
    }

    private TemplateStyle CloneTemplateStyle(TemplateStyle original)
    {
        // Simple cloning - in production you might want to use a more sophisticated approach
        var json = JsonSerializer.Serialize(original);
        return JsonSerializer.Deserialize<TemplateStyle>(json) ?? new TemplateStyle();
    }

    private TemplateLayout CloneTemplateLayout(TemplateLayout original)
    {
        // Simple cloning - in production you might want to use a more sophisticated approach
        var json = JsonSerializer.Serialize(original);
        return JsonSerializer.Deserialize<TemplateLayout>(json) ?? new TemplateLayout();
    }

    private List<PagePreview> GeneratePagePreviews(DocumentTemplate template, RoutineDocumentRequest request)
    {
        var pages = new List<PagePreview>();
        int pageNumber = 1;

        // Cover page
        if (template.Layout.IncludeCoverPage)
        {
            pages.Add(new PagePreview
            {
                PageNumber = pageNumber++,
                PageTitle = "Portada",
                ContentSummary = new List<string>
                {
                    $"Cliente: {request.ClientName}",
                    $"Fecha: {request.CreationDate:dd/MM/yyyy}",
                    "Información del plan"
                }
            });
        }

        // Table of contents
        if (template.Layout.IncludeTableOfContents)
        {
            pages.Add(new PagePreview
            {
                PageNumber = pageNumber++,
                PageTitle = "Índice",
                ContentSummary = new List<string> { "Tabla de contenidos del documento" }
            });
        }

        // Main content pages
        var exerciseCount = request.Routine?.MainWorkout?.Sum(b => b.Exercises.Count) ??
                           request.WeeklyPrograms.Sum(w => w.DailyWorkouts.Sum(d => d.ExerciseBlocks.Sum(b => b.Exercises.Count)));

        var estimatedMainPages = Math.Max(1, (int)Math.Ceiling(exerciseCount / 5.0)); // ~5 exercises per page

        for (int i = 0; i < estimatedMainPages; i++)
        {
            pages.Add(new PagePreview
            {
                PageNumber = pageNumber++,
                PageTitle = $"Rutina - Página {i + 1}",
                ContentSummary = new List<string>
                {
                    "Ejercicios de entrenamiento",
                    "Instrucciones y parámetros",
                    template.Style.IncludeImages ? "Imágenes de ejercicios" : "Sin imágenes"
                }
            });
        }

        return pages;
    }

    private DocumentStructure GenerateDocumentStructure(DocumentTemplate template, RoutineDocumentRequest request)
    {
        var sections = new List<DocumentSection>();
        int currentPage = 1;

        if (template.Layout.IncludeCoverPage)
        {
            sections.Add(new DocumentSection
            {
                SectionName = "Portada",
                StartPage = currentPage++,
                PageCount = 1
            });
        }

        if (template.Layout.IncludeTableOfContents)
        {
            sections.Add(new DocumentSection
            {
                SectionName = "Índice",
                StartPage = currentPage++,
                PageCount = 1
            });
        }

        // Estimate main content pages
        var exerciseCount = request.Routine?.MainWorkout?.Sum(b => b.Exercises.Count) ??
                           request.WeeklyPrograms.Sum(w => w.DailyWorkouts.Sum(d => d.ExerciseBlocks.Sum(b => b.Exercises.Count)));

        var mainContentPages = Math.Max(1, (int)Math.Ceiling(exerciseCount / 5.0));

        sections.Add(new DocumentSection
        {
            SectionName = "Rutina de Entrenamiento",
            StartPage = currentPage,
            PageCount = mainContentPages,
            SubSections = new List<string> { "Calentamiento", "Ejercicios Principales", "Enfriamiento" }
        });

        currentPage += mainContentPages;

        if (template.Layout.IncludeProgressSection)
        {
            sections.Add(new DocumentSection
            {
                SectionName = "Seguimiento de Progreso",
                StartPage = currentPage++,
                PageCount = 1
            });
        }

        return new DocumentStructure
        {
            Sections = sections,
            EstimatedPageCount = currentPage - 1,
            EstimatedReadingTime = TimeSpan.FromMinutes((currentPage - 1) * 2) // ~2 minutes per page
        };
    }

    private PreviewStatistics GeneratePreviewStatistics(DocumentTemplate template, RoutineDocumentRequest request)
    {
        var exerciseCount = request.Routine?.MainWorkout?.Sum(b => b.Exercises.Count) ??
                           request.WeeklyPrograms.Sum(w => w.DailyWorkouts.Sum(d => d.ExerciseBlocks.Sum(b => b.Exercises.Count)));

        return new PreviewStatistics
        {
            TotalExercises = exerciseCount,
            ExercisesWithImages = template.Style.IncludeImages ? (int)(exerciseCount * 0.8) : 0, // 80% have images
            TotalInstructions = exerciseCount * 3, // Average 3 instructions per exercise
            SafetyNotesCount = request.SafetyNotes.Count,
            ContentBreakdown = new Dictionary<string, int>
            {
                ["Ejercicios"] = exerciseCount,
                ["Imágenes"] = template.Style.IncludeImages ? (int)(exerciseCount * 0.8) : 0,
                ["Consejos de seguridad"] = request.SafetyNotes.Count,
                ["Días de entrenamiento"] = request.WeeklyPrograms.Sum(w => w.DailyWorkouts.Count)
            }
        };
    }
}