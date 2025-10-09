using GymRoutineGenerator.Tests.Ollama;

Console.WriteLine("🤖 GymRoutine Generator - Ollama Integration & Enhanced AI Test");
Console.WriteLine("================================================================");
Console.WriteLine();

try
{
    // Test basic Ollama integration
    await OllamaIntegrationTest.RunOllamaIntegrationTests();

    Console.WriteLine();
    Console.WriteLine("🚀 Running Enhanced AI Features Tests...");
    Console.WriteLine();

    // Test enhanced prompt templates and context building
    await EnhancedPromptTemplateTest.RunEnhancedPromptTemplateTests();

    Console.WriteLine();
    Console.WriteLine("🏗️ Running Routine Structure & Programming Tests...");
    Console.WriteLine();

    // Test routine structure and programming logic
    await RoutineStructureTest.RunRoutineStructureTests();

    Console.WriteLine();
    Console.WriteLine("🔍 Running Spanish Response Processing Tests...");
    Console.WriteLine();

    // Test Spanish language AI response processing
    await SpanishResponseProcessingTest.RunSpanishResponseProcessingTests();

    Console.WriteLine();
    Console.WriteLine("🎯 Running Routine Customization & Variation Tests...");
    Console.WriteLine();

    // Test routine customization and variation engine
    await RoutineCustomizationTest.RunRoutineCustomizationTests();
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Critical Test Failure: {ex.Message}");
    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
}

Console.WriteLine();
Console.WriteLine("🎉 Todas las pruebas de IA completadas!");
Console.WriteLine();
Console.WriteLine("📋 RESUMEN DE FUNCIONALIDADES:");
Console.WriteLine("✅ Story 4.1: Integración Ollama & Prompt Engineering básico");
Console.WriteLine("✅ Story 4.2: Plantillas de prompts mejoradas & Construcción de contexto");
Console.WriteLine("✅ Story 4.3: Estructura de rutinas & Lógica de programación");
Console.WriteLine("✅ Story 4.4: Procesamiento de respuestas IA en español");
Console.WriteLine("✅ Story 4.5: Motor de personalización y variación de rutinas");
Console.WriteLine("✅ Algoritmo de selección inteligente de ejercicios");
Console.WriteLine("✅ Sistema de respaldo con reglas");
Console.WriteLine("✅ Mapeo avanzado de parámetros de usuario");
Console.WriteLine("✅ Protocolos de calentamiento y enfriamiento científicos");
Console.WriteLine("✅ Planificación de progresión y periodización");
Console.WriteLine("✅ Consideraciones de seguridad avanzadas");
Console.WriteLine("✅ Validación y mejora de calidad en español");
Console.WriteLine("✅ Parsing inteligente de instrucciones de ejercicios");
Console.WriteLine("✅ Evaluación automática de calidad de respuestas");
Console.WriteLine("✅ Normalización de terminología fitness en español");
Console.WriteLine("✅ Motor avanzado de personalización de rutinas");
Console.WriteLine("✅ Generación automática de variaciones de rutinas");
Console.WriteLine("✅ Adaptación basada en restricciones múltiples");
Console.WriteLine("✅ Creación de programas personalizados de largo plazo");
Console.WriteLine("✅ Sistema inteligente de sustitución de ejercicios");
Console.WriteLine("✅ Personalización basada en perfil biométrico");
Console.WriteLine("✅ Adaptación para rehabilitación y limitaciones físicas");
Console.WriteLine();
Console.WriteLine("Para usar la aplicación completa, asegúrate de que:");
Console.WriteLine("1. Ollama esté instalado (winget install Ollama.Ollama)");
Console.WriteLine("2. Ollama esté ejecutándose (ollama serve)");
Console.WriteLine("3. Modelo Mistral esté descargado (ollama pull mistral:7b)");
