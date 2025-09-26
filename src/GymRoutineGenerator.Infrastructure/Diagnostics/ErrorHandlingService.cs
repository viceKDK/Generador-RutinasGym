using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using GymRoutineGenerator.Core.Services.Diagnostics;

namespace GymRoutineGenerator.Infrastructure.Diagnostics;

public class ErrorHandlingService : IErrorHandlingService
{
    private readonly ILogger<ErrorHandlingService> _logger;
    private readonly Dictionary<string, bool> _degradedServices;
    private readonly string _logDirectory;

    public ErrorHandlingService(ILogger<ErrorHandlingService>? logger = null)
    {
        _logger = logger ?? CreateFileLogger();
        _degradedServices = new Dictionary<string, bool>();
        _logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                   "GymRoutineGenerator", "Logs");

        EnsureLogDirectoryExists();
    }

    public async Task<ErrorResult> HandleErrorAsync(Exception exception, string context = "", object? additionalData = null)
    {
        var errorResult = new ErrorResult();

        try
        {
            // Determinar categoría del error
            errorResult.Category = DetermineErrorCategory(exception);
            errorResult.ErrorCode = GenerateErrorCode(exception);

            // Generar mensajes amigables
            errorResult.UserMessage = GenerateUserFriendlyMessage(exception, errorResult.Category);
            errorResult.TechnicalMessage = $"{exception.GetType().Name}: {exception.Message}";

            // Determinar si es recuperable
            errorResult.IsRecoverable = IsRecoverableError(exception, errorResult.Category);

            // Generar acciones sugeridas
            errorResult.SuggestedActions = GenerateSuggestedActions(exception, errorResult.Category);

            // Log del error
            var logMessage = $"[{context}] {errorResult.TechnicalMessage}";
            if (additionalData != null)
            {
                logMessage += $" | Data: {System.Text.Json.JsonSerializer.Serialize(additionalData)}";
            }

            _logger?.LogError(exception, logMessage);

            await LogErrorToFileAsync(exception, context, errorResult);

        }
        catch (Exception logException)
        {
            // Fallback si el logging falla
            errorResult.UserMessage = "Ha ocurrido un error inesperado. Por favor, reinicie la aplicación.";
            errorResult.TechnicalMessage = exception.Message;
            errorResult.IsRecoverable = false;
            errorResult.Category = ErrorCategory.System;

            Debug.WriteLine($"Error logging failed: {logException.Message}");
        }

        return errorResult;
    }

    public async Task LogErrorAsync(string message, ErrorSeverity severity = ErrorSeverity.Error, string context = "")
    {
        var logLevel = severity switch
        {
            ErrorSeverity.Debug => LogLevel.Debug,
            ErrorSeverity.Info => LogLevel.Information,
            ErrorSeverity.Warning => LogLevel.Warning,
            ErrorSeverity.Error => LogLevel.Error,
            ErrorSeverity.Critical => LogLevel.Critical,
            _ => LogLevel.Error
        };

        _logger?.Log(logLevel, "[{Context}] {Message}", context, message);

        await LogMessageToFileAsync(message, severity, context);
    }

    public async Task LogInfoAsync(string message, string context = "")
    {
        await LogErrorAsync(message, ErrorSeverity.Info, context);
    }

    public async Task<SystemHealthStatus> GetSystemHealthAsync()
    {
        var health = new SystemHealthStatus
        {
            LastChecked = DateTime.Now,
            IsHealthy = true
        };

        // Verificar servicios críticos
        health.Services["FileSystem"] = await CheckFileSystemHealthAsync();
        health.Services["Memory"] = await CheckMemoryHealthAsync();
        health.Services["Network"] = await CheckNetworkHealthAsync();
        health.Services["WordGeneration"] = await CheckWordGenerationHealthAsync();
        health.Services["Ollama"] = await CheckOllamaHealthAsync();

        // Determinar salud general
        foreach (var service in health.Services.Values)
        {
            if (!service.IsAvailable)
            {
                health.IsHealthy = false;
                health.Issues.Add($"Servicio {service.ServiceName} no disponible: {service.LastError}");
            }
            else if (service.IsDegraded)
            {
                health.Issues.Add($"Servicio {service.ServiceName} funcionando en modo degradado");
            }
        }

        await LogInfoAsync($"System Health Check - Healthy: {health.IsHealthy}, Issues: {health.Issues.Count}", "HealthCheck");

        return health;
    }

    public void EnableGracefulDegradation(string serviceName, bool isEnabled = true)
    {
        _degradedServices[serviceName] = isEnabled;

        var status = isEnabled ? "habilitado" : "deshabilitado";
        _logger?.LogInformation("Degradación elegante {Status} para servicio {ServiceName}", status, serviceName);
    }

    public bool IsServiceDegraded(string serviceName)
    {
        return _degradedServices.GetValueOrDefault(serviceName, false);
    }

    #region Private Methods

    private ErrorCategory DetermineErrorCategory(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => ErrorCategory.Permission,
            DirectoryNotFoundException => ErrorCategory.FileSystem,
            FileNotFoundException => ErrorCategory.FileSystem,
            IOException => ErrorCategory.FileSystem,
            System.Net.NetworkInformation.NetworkInformationException => ErrorCategory.Network,
            System.Net.Http.HttpRequestException => ErrorCategory.Network,
            ArgumentException => ErrorCategory.UserInput,
            InvalidOperationException when exception.Message.Contains("Ollama") => ErrorCategory.Ollama,
            InvalidOperationException when exception.Message.Contains("Word") => ErrorCategory.WordGeneration,
            OutOfMemoryException => ErrorCategory.System,
            _ => ErrorCategory.Unknown
        };
    }

    private string GenerateErrorCode(Exception exception)
    {
        var category = DetermineErrorCategory(exception);
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmm");
        var hash = Math.Abs(exception.Message.GetHashCode()) % 1000;

        return $"{category:G}_{timestamp}_{hash:D3}";
    }

    private string GenerateUserFriendlyMessage(Exception exception, ErrorCategory category)
    {
        return category switch
        {
            ErrorCategory.FileSystem => "❌ No se puede acceder al archivo o carpeta solicitada. Verifique que existe y tiene permisos para accederla.",
            ErrorCategory.Permission => "🔒 No tiene permisos suficientes para realizar esta operación. Intente ejecutar la aplicación como administrador.",
            ErrorCategory.Network => "🌐 Problema de conexión de red. Verifique su conexión a internet e intente nuevamente.",
            ErrorCategory.Ollama => "🤖 El servicio de inteligencia artificial no está disponible. La aplicación funcionará con características limitadas.",
            ErrorCategory.WordGeneration => "📄 Error al crear el documento Word. Verifique que Microsoft Word esté instalado o que tenga espacio suficiente en disco.",
            ErrorCategory.UserInput => "⚠️ La información proporcionada no es válida. Por favor, verifique los datos ingresados e intente nuevamente.",
            ErrorCategory.Configuration => "⚙️ Error de configuración. Algunos ajustes de la aplicación no son válidos.",
            ErrorCategory.Database => "💾 Error al acceder a la base de datos de ejercicios. Intente reiniciar la aplicación.",
            ErrorCategory.System => "💥 Error crítico del sistema. Por favor, reinicie la aplicación e intente nuevamente.",
            _ => "❓ Ha ocurrido un error inesperado. La aplicación intentará continuar funcionando."
        };
    }

    private bool IsRecoverableError(Exception exception, ErrorCategory category)
    {
        return category switch
        {
            ErrorCategory.Network => true,
            ErrorCategory.FileSystem => true,
            ErrorCategory.UserInput => true,
            ErrorCategory.Ollama => true, // La app puede funcionar sin AI
            ErrorCategory.Configuration => true,
            ErrorCategory.Permission => false,
            ErrorCategory.System => false,
            ErrorCategory.Database => false,
            _ => false
        };
    }

    private List<string> GenerateSuggestedActions(Exception exception, ErrorCategory category)
    {
        return category switch
        {
            ErrorCategory.FileSystem => new List<string>
            {
                "Verifique que el archivo o carpeta existe",
                "Asegúrese de tener permisos de lectura/escritura",
                "Verifique que hay espacio suficiente en disco"
            },
            ErrorCategory.Permission => new List<string>
            {
                "Ejecute la aplicación como administrador",
                "Verifique los permisos de la carpeta de documentos",
                "Contacte al administrador del sistema"
            },
            ErrorCategory.Network => new List<string>
            {
                "Verifique su conexión a internet",
                "Intente nuevamente en unos momentos",
                "Revise la configuración de firewall"
            },
            ErrorCategory.Ollama => new List<string>
            {
                "La aplicación funcionará sin inteligencia artificial",
                "Verifique si Ollama está instalado y ejecutándose",
                "Use las funciones básicas de generación de rutinas"
            },
            ErrorCategory.WordGeneration => new List<string>
            {
                "Verifique que Microsoft Word está instalado",
                "Asegúrese de tener espacio suficiente en disco",
                "Intente con una plantilla más simple"
            },
            ErrorCategory.UserInput => new List<string>
            {
                "Revise la información ingresada",
                "Asegúrese de completar todos los campos requeridos",
                "Verifique que los valores estén en el rango correcto"
            },
            _ => new List<string>
            {
                "Reinicie la aplicación",
                "Si el problema persiste, contacte soporte técnico",
                "Intente con una operación más simple"
            }
        };
    }

    private async Task<ServiceStatus> CheckFileSystemHealthAsync()
    {
        var service = new ServiceStatus { ServiceName = "FileSystem", IsAvailable = false };
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var testPath = Path.Combine(documentsPath, "GymRoutineGenerator_HealthCheck.tmp");

            await File.WriteAllTextAsync(testPath, "health_check");
            var content = await File.ReadAllTextAsync(testPath);
            File.Delete(testPath);

            service.IsAvailable = content == "health_check";
            service.Status = service.IsAvailable ? "Funcionando correctamente" : "Error de lectura/escritura";
        }
        catch (Exception ex)
        {
            service.LastError = ex.Message;
            service.Status = "Error de acceso al sistema de archivos";
        }
        finally
        {
            stopwatch.Stop();
            service.ResponseTime = stopwatch.Elapsed;
        }

        return service;
    }

    private async Task<ServiceStatus> CheckMemoryHealthAsync()
    {
        var service = new ServiceStatus { ServiceName = "Memory" };
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var process = Process.GetCurrentProcess();
            var memoryMB = process.WorkingSet64 / (1024 * 1024);

            service.IsAvailable = memoryMB < 500; // Menos de 500MB está bien
            service.IsDegraded = memoryMB > 300;   // Más de 300MB es degradado
            service.Status = $"Usando {memoryMB} MB de RAM";

            if (memoryMB > 500)
            {
                service.LastError = "Consumo de memoria alto";
            }
        }
        catch (Exception ex)
        {
            service.IsAvailable = false;
            service.LastError = ex.Message;
            service.Status = "Error al verificar memoria";
        }
        finally
        {
            stopwatch.Stop();
            service.ResponseTime = stopwatch.Elapsed;
        }

        await Task.CompletedTask;
        return service;
    }

    private async Task<ServiceStatus> CheckNetworkHealthAsync()
    {
        var service = new ServiceStatus { ServiceName = "Network" };
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var ping = new Ping();
            var reply = await ping.SendPingAsync("8.8.8.8", 5000);

            service.IsAvailable = reply.Status == IPStatus.Success;
            service.Status = reply.Status.ToString();

            if (!service.IsAvailable)
            {
                service.LastError = $"Ping failed: {reply.Status}";
            }
        }
        catch (Exception ex)
        {
            service.IsAvailable = false;
            service.LastError = ex.Message;
            service.Status = "Error de conectividad";
        }
        finally
        {
            stopwatch.Stop();
            service.ResponseTime = stopwatch.Elapsed;
        }

        return service;
    }

    private async Task<ServiceStatus> CheckWordGenerationHealthAsync()
    {
        var service = new ServiceStatus { ServiceName = "WordGeneration" };
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Verificar que las librerías de Word están disponibles
            var hasDocumentFormat = Type.GetType("DocumentFormat.OpenXml.WordprocessingDocument, DocumentFormat.OpenXml") != null;

            service.IsAvailable = hasDocumentFormat;
            service.Status = hasDocumentFormat ? "DocumentFormat.OpenXml disponible" : "Librerías Word no encontradas";

            if (!hasDocumentFormat)
            {
                service.LastError = "DocumentFormat.OpenXml no está disponible";
            }
        }
        catch (Exception ex)
        {
            service.IsAvailable = false;
            service.LastError = ex.Message;
            service.Status = "Error al verificar capacidades Word";
        }
        finally
        {
            stopwatch.Stop();
            service.ResponseTime = stopwatch.Elapsed;
        }

        await Task.CompletedTask;
        return service;
    }

    private async Task<ServiceStatus> CheckOllamaHealthAsync()
    {
        var service = new ServiceStatus { ServiceName = "Ollama" };
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var client = new System.Net.Http.HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            var response = await client.GetAsync("http://localhost:11434/api/version");
            service.IsAvailable = response.IsSuccessStatusCode;
            service.Status = service.IsAvailable ? "Ollama respondiendo" : "Ollama no disponible";

            if (!service.IsAvailable)
            {
                service.LastError = $"HTTP {response.StatusCode}";
                EnableGracefulDegradation("Ollama", true); // Activar modo degradado
            }
        }
        catch (Exception ex)
        {
            service.IsAvailable = false;
            service.LastError = ex.Message;
            service.Status = "Ollama no accesible";
            EnableGracefulDegradation("Ollama", true); // Activar modo degradado
        }
        finally
        {
            stopwatch.Stop();
            service.ResponseTime = stopwatch.Elapsed;
        }

        return service;
    }

    private void EnsureLogDirectoryExists()
    {
        try
        {
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Could not create log directory: {ex.Message}");
        }
    }

    private async Task LogErrorToFileAsync(Exception exception, string context, ErrorResult result)
    {
        try
        {
            var logFileName = $"errors_{DateTime.Now:yyyy-MM-dd}.log";
            var logFilePath = Path.Combine(_logDirectory, logFileName);

            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR [{context}]\n" +
                          $"Code: {result.ErrorCode}\n" +
                          $"Category: {result.Category}\n" +
                          $"User Message: {result.UserMessage}\n" +
                          $"Technical: {result.TechnicalMessage}\n" +
                          $"Stack Trace: {exception.StackTrace}\n" +
                          $"Recoverable: {result.IsRecoverable}\n" +
                          new string('-', 80) + "\n\n";

            await File.AppendAllTextAsync(logFilePath, logEntry);

            // Limpiar logs antiguos (mantener solo últimos 30 días)
            CleanupOldLogs();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to write error log: {ex.Message}");
        }
    }

    private async Task LogMessageToFileAsync(string message, ErrorSeverity severity, string context)
    {
        try
        {
            var logFileName = $"app_{DateTime.Now:yyyy-MM-dd}.log";
            var logFilePath = Path.Combine(_logDirectory, logFileName);

            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {severity.ToString().ToUpper()} [{context}] {message}\n";

            await File.AppendAllTextAsync(logFilePath, logEntry);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to write log: {ex.Message}");
        }
    }

    private void CleanupOldLogs()
    {
        try
        {
            var cutoffDate = DateTime.Now.AddDays(-30);
            var logFiles = Directory.GetFiles(_logDirectory, "*.log");

            foreach (var logFile in logFiles)
            {
                var fileInfo = new FileInfo(logFile);
                if (fileInfo.CreationTime < cutoffDate)
                {
                    File.Delete(logFile);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to cleanup old logs: {ex.Message}");
        }
    }

    private ILogger<ErrorHandlingService> CreateFileLogger()
    {
        // Fallback logger simple que no requiere dependencias externas
        return new NullLogger<ErrorHandlingService>();
    }

    private class NullLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullDisposable.Instance;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }

        private class NullDisposable : IDisposable
        {
            public static NullDisposable Instance = new();
            public void Dispose() { }
        }
    }

    #endregion
}