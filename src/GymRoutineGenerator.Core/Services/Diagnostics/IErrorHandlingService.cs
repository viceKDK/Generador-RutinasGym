using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymRoutineGenerator.Core.Services.Diagnostics;

public interface IErrorHandlingService
{
    /// <summary>
    /// Maneja una excepción y proporciona un mensaje amigable para el usuario
    /// </summary>
    Task<ErrorResult> HandleErrorAsync(Exception exception, string context = "", object additionalData = null);

    /// <summary>
    /// Registra un error sin procesar una excepción
    /// </summary>
    Task LogErrorAsync(string message, ErrorSeverity severity = ErrorSeverity.Error, string context = "");

    /// <summary>
    /// Registra información de diagnóstico
    /// </summary>
    Task LogInfoAsync(string message, string context = "");

    /// <summary>
    /// Obtiene el estado de salud de los servicios críticos
    /// </summary>
    Task<SystemHealthStatus> GetSystemHealthAsync();

    /// <summary>
    /// Configura el modo de degradación elegante cuando falla un servicio
    /// </summary>
    void EnableGracefulDegradation(string serviceName, bool isEnabled = true);

    /// <summary>
    /// Verifica si un servicio está funcionando en modo degradado
    /// </summary>
    bool IsServiceDegraded(string serviceName);
}

public class ErrorResult
{
    public bool IsRecoverable { get; set; }
    public string UserMessage { get; set; } = string.Empty;
    public string TechnicalMessage { get; set; } = string.Empty;
    public ErrorCategory Category { get; set; }
    public List<string> SuggestedActions { get; set; } = new();
    public string ErrorCode { get; set; } = string.Empty;
}

public enum ErrorCategory
{
    Unknown,
    FileSystem,
    Network,
    Permission,
    Configuration,
    Ollama,
    WordGeneration,
    Database,
    UserInput,
    System
}

public enum ErrorSeverity
{
    Debug,
    Info,
    Warning,
    Error,
    Critical
}

public class SystemHealthStatus
{
    public bool IsHealthy { get; set; }
    public Dictionary<string, ServiceStatus> Services { get; set; } = new();
    public List<string> Issues { get; set; } = new();
    public DateTime LastChecked { get; set; }
}

public class ServiceStatus
{
    public string ServiceName { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public bool IsDegraded { get; set; }
    public string Status { get; set; } = string.Empty;
    public TimeSpan ResponseTime { get; set; }
    public string LastError { get; set; } = string.Empty;
}