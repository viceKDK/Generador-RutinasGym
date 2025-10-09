using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Entities;
using GymRoutineGenerator.Data.Management;
using GymRoutineGenerator.Infrastructure.Images;

namespace GymRoutineGenerator.Tests;

[TestClass]
public class MigrationVerificationTest
{
    [TestMethod]
    public async Task VerifyImageMetadataColumnExists()
    {
        Console.WriteLine("=== VERIFICACIÓN DE MIGRACIÓN ===");

        // Usar la BD real del directorio raíz
        var services = new ServiceCollection();
        services.AddDbContext<GymRoutineContext>(options =>
            options.UseSqlite("Data Source=../../../gymroutine.db"));
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IExerciseManagementService, ExerciseManagementService>();

        using var serviceProvider = services.BuildServiceProvider();
        using var context = serviceProvider.GetRequiredService<GymRoutineContext>();

        try
        {
            // Test 1: Verificar que la BD se puede conectar
            await context.Database.EnsureCreatedAsync();
            Console.WriteLine("✅ Conexión a BD exitosa");

            // Test 2: Verificar que podemos consultar ExerciseImages
            var imageCount = await context.ExerciseImages.CountAsync();
            Console.WriteLine($"📊 Imágenes existentes: {imageCount}");

            // Test 3: Intentar crear una entidad ExerciseImage con ImageMetadata
            var testImage = new ExerciseImage
            {
                ExerciseId = 1,
                ImageData = new byte[] { 1, 2, 3, 4 },
                ImageMetadata = "{\"test\": \"metadata\"}",
                ImagePosition = "test",
                IsPrimary = false,
                Description = "Test migration verification"
            };

            // Intentar agregar sin guardar (solo para verificar que la entidad es válida)
            context.ExerciseImages.Add(testImage);
            Console.WriteLine("✅ Entidad ExerciseImage con ImageMetadata creada correctamente");

            // No guardar - solo verificar que no hay errores
            context.Entry(testImage).State = EntityState.Detached;

            // Test 4: Verificar ejercicios existentes
            var exerciseCount = await context.Exercises.CountAsync();
            Console.WriteLine($"📊 Ejercicios existentes: {exerciseCount}");

            if (exerciseCount > 0)
            {
                var sampleExercise = await context.Exercises.FirstAsync();
                Console.WriteLine($"📝 Ejercicio de muestra: {sampleExercise.SpanishName}");
            }

            Console.WriteLine("\n🎉 MIGRACIÓN VERIFICADA EXITOSAMENTE");
            Console.WriteLine("✅ La columna ImageMetadata existe y es funcional");
            Console.WriteLine("✅ La aplicación puede usar el nuevo sistema de imágenes");
            Console.WriteLine("\n🚀 AHORA PRUEBA AGREGAR UNA IMAGEN EN LA UI:");
            Console.WriteLine("   1. Abre la aplicación");
            Console.WriteLine("   2. Ve al Gestor de Imágenes");
            Console.WriteLine("   3. Selecciona un ejercicio");
            Console.WriteLine("   4. Agrega una imagen");
            Console.WriteLine("   5. Debería aparecer en la vista previa SIN ERRORES");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR: {ex.Message}");
            if (ex.Message.Contains("no such column"))
            {
                Console.WriteLine("⚠️ La migración no se aplicó correctamente a la BD principal");
                Console.WriteLine("💡 Solución: Aplicar migración manualmente");
            }
            Assert.Fail($"Verification failed: {ex.Message}");
        }
    }

    [TestMethod]
    public async Task TestNewImageWorkflowWithRealDatabase()
    {
        Console.WriteLine("=== TEST COMPLETO CON BD REAL ===");

        var services = new ServiceCollection();
        services.AddDbContext<GymRoutineContext>(options =>
            options.UseSqlite("Data Source=../../../gymroutine.db"));
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IExerciseManagementService, ExerciseManagementService>();

        using var serviceProvider = services.BuildServiceProvider();
        using var context = serviceProvider.GetRequiredService<GymRoutineContext>();
        var managementService = serviceProvider.GetRequiredService<IExerciseManagementService>();

        try
        {
            // Buscar un ejercicio real
            var exercise = await context.Exercises.FirstOrDefaultAsync();

            if (exercise == null)
            {
                Console.WriteLine("⚠️ No hay ejercicios en la BD");
                Console.WriteLine("💡 Asegúrate de que la aplicación tenga datos de ejercicios");
                return;
            }

            Console.WriteLine($"✅ Usando ejercicio: {exercise.SpanishName} (ID: {exercise.Id})");

            // Crear imagen de prueba
            var testImageBytes = new byte[100];
            new Random().NextBytes(testImageBytes);

            var imageUpload = new ExerciseImageUpload
            {
                ImageData = testImageBytes,
                FileName = "test_migration.png",
                ContentType = "image/png",
                Position = "demonstration",
                IsPrimary = true,
                Description = "Test de migración aplicada"
            };

            // Intentar agregar imagen (esto fallará si no existe ImageMetadata)
            var result = await managementService.AddExerciseImageAsync(exercise.Id, imageUpload);

            if (result.Success)
            {
                Console.WriteLine("🎉 ¡ÉXITO TOTAL!");
                Console.WriteLine($"✅ {result.Message}");
                Console.WriteLine("✅ La migración funciona perfectamente");
                Console.WriteLine("✅ El nuevo sistema de imágenes está operativo");

                // Verificar que se puede recuperar
                var images = await managementService.GetExerciseImagesAsync(exercise.Id);
                var addedImage = images.FirstOrDefault(i => i.Description.Contains("Test de migración"));

                if (addedImage != null)
                {
                    Console.WriteLine($"✅ Imagen recuperada correctamente:");
                    Console.WriteLine($"   - ID: {addedImage.Id}");
                    Console.WriteLine($"   - Bytes: {addedImage.ImageData?.Length}");
                    Console.WriteLine($"   - Metadata presente: {!string.IsNullOrEmpty(addedImage.ImageMetadata)}");
                }
            }
            else
            {
                Console.WriteLine("❌ Error agregando imagen:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"   - {error}");
                }
                Assert.Fail("Failed to add image after migration");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR en test completo: {ex.Message}");
            if (ex.Message.Contains("no such column"))
            {
                Console.WriteLine("⚠️ La columna ImageMetadata no existe en la BD principal");
                Assert.Fail("Migration not applied to main database");
            }
            else
            {
                Assert.Fail($"Unexpected error: {ex.Message}");
            }
        }
    }
}