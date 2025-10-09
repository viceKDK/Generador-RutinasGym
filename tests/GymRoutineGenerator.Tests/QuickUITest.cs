using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Entities;
using GymRoutineGenerator.Data.Management;
using GymRoutineGenerator.Infrastructure.Images;
using GymRoutineGenerator.Core.Enums;
using System.Drawing;
using System.Drawing.Imaging;

namespace GymRoutineGenerator.Tests;

[TestClass]
public class QuickUITest
{
    [TestMethod]
    public async Task AddImageToRealDatabase()
    {
        Console.WriteLine("=== AGREGANDO IMAGEN REAL A LA BASE DE DATOS ===");

        // Usar SQLite real (como la aplicación)
        var services = new ServiceCollection();
        services.AddDbContext<GymRoutineContext>(options =>
            options.UseSqlite("Data Source=gym_routine.db"));
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IExerciseManagementService, ExerciseManagementService>();

        using var serviceProvider = services.BuildServiceProvider();
        using var context = serviceProvider.GetRequiredService<GymRoutineContext>();
        var managementService = serviceProvider.GetRequiredService<IExerciseManagementService>();

        // Asegurar que la BD existe
        await context.Database.EnsureCreatedAsync();

        Console.WriteLine("✅ Conectado a base de datos real");

        // Buscar un ejercicio real
        var exercise = context.Exercises.FirstOrDefault(e =>
            e.SpanishName.ToLower().Contains("flexion") ||
            e.Name.ToLower().Contains("push"));

        if (exercise == null)
        {
            Console.WriteLine("❌ No se encontró ejercicio de flexiones");
            Console.WriteLine("Ejercicios disponibles:");
            var exercises = context.Exercises.Take(5).Select(e => e.SpanishName).ToList();
            foreach (var ex in exercises)
            {
                Console.WriteLine($"  - {ex}");
            }
            return;
        }

        Console.WriteLine($"✅ Ejercicio encontrado: {exercise.SpanishName} (ID: {exercise.Id})");

        // Crear imagen de prueba
        var testImageBytes = CreateTestImage("PRUEBA REAL UI");

        var imageUpload = new ExerciseImageUpload
        {
            ImageData = testImageBytes,
            FileName = "prueba_ui_real.png",
            ContentType = "image/png",
            Position = "demonstration",
            IsPrimary = true,
            Description = "Imagen de prueba para verificar UI real"
        };

        Console.WriteLine($"📦 Imagen creada: {testImageBytes.Length:N0} bytes");

        // Agregar imagen
        var result = await managementService.AddExerciseImageAsync(exercise.Id, imageUpload);

        if (result.Success)
        {
            Console.WriteLine($"🎉 ÉXITO: {result.Message}");

            // Verificar que se puede recuperar
            var images = await managementService.GetExerciseImagesAsync(exercise.Id);
            var addedImage = images.FirstOrDefault(i => i.Description.Contains("Imagen de prueba"));

            if (addedImage != null)
            {
                Console.WriteLine($"✅ Imagen verificada en BD:");
                Console.WriteLine($"   - ID: {addedImage.Id}");
                Console.WriteLine($"   - Bytes: {addedImage.ImageData?.Length:N0}");
                Console.WriteLine($"   - Position: {addedImage.ImagePosition}");
                Console.WriteLine($"   - Primary: {addedImage.IsPrimary}");

                if (!string.IsNullOrEmpty(addedImage.ImageMetadata))
                {
                    var metadata = System.Text.Json.JsonSerializer.Deserialize<GymRoutineGenerator.Data.Models.ImageMetadata>(addedImage.ImageMetadata);
                    Console.WriteLine($"   - Metadata: {metadata?.GetDisplaySummary()}");
                }

                Console.WriteLine("\n🚀 AHORA ABRE LA APLICACIÓN Y BUSCA EL EJERCICIO:");
                Console.WriteLine($"   Ejercicio: {exercise.SpanishName}");
                Console.WriteLine("   La imagen debería aparecer automáticamente en la vista previa");
            }
            else
            {
                Console.WriteLine("⚠️ Imagen agregada pero no encontrada en verificación");
            }
        }
        else
        {
            Console.WriteLine($"❌ FALLO: {string.Join(", ", result.Errors)}");
        }
    }

    private byte[] CreateTestImage(string text)
    {
        using var bitmap = new Bitmap(500, 350);
        using var graphics = Graphics.FromImage(bitmap);

        // Fondo llamativo
        graphics.Clear(Color.LightCyan);

        // Borde
        using var pen = new Pen(Color.DarkRed, 4);
        graphics.DrawRectangle(pen, 5, 5, 490, 340);

        // Texto grande
        using var font = new Font("Arial", 20, FontStyle.Bold);
        using var brush = new SolidBrush(Color.DarkRed);

        var rect = new RectangleF(10, 100, 480, 150);
        var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        graphics.DrawString(text, font, brush, rect, format);

        // Info adicional
        using var smallFont = new Font("Arial", 12);
        graphics.DrawString($"Creado: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", smallFont, brush, 20, 300);
        graphics.DrawString("SI VES ESTO, EL SISTEMA FUNCIONA ✅", smallFont, brush, 180, 320);

        // Elemento visual distintivo
        using var yellowBrush = new SolidBrush(Color.Yellow);
        graphics.FillEllipse(yellowBrush, 200, 50, 100, 40);
        graphics.DrawString("NUEVO", font, brush, 210, 60);

        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        return stream.ToArray();
    }
}