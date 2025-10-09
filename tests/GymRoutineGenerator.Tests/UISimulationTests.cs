using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
public class UISimulationTests
{
    private ServiceProvider _serviceProvider;
    private GymRoutineContext _context;
    private IExerciseManagementService _managementService;

    [TestInitialize]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        services.AddDbContext<GymRoutineContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IExerciseManagementService, ExerciseManagementService>();

        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<GymRoutineContext>();
        _managementService = _serviceProvider.GetRequiredService<IExerciseManagementService>();

        SeedTestData();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _serviceProvider?.Dispose();
    }

    private void SeedTestData()
    {
        var muscleGroup = new Data.Entities.MuscleGroup
        {
            Id = 1,
            Name = "Chest",
            SpanishName = "Pecho",
            Description = "Chest muscles"
        };
        _context.MuscleGroups.Add(muscleGroup);

        var equipmentType = new Data.Entities.EquipmentType
        {
            Id = 1,
            Name = "Bodyweight",
            SpanishName = "Peso corporal",
            Description = "No equipment needed"
        };
        _context.EquipmentTypes.Add(equipmentType);

        var exercise = new Exercise
        {
            Id = 1,
            Name = "Push-up",
            SpanishName = "Flexiones",
            Description = "Basic push-up exercise for UI testing",
            Instructions = "Lower your body until chest nearly touches floor, then push back up",
            PrimaryMuscleGroupId = 1,
            EquipmentTypeId = 1,
            DifficultyLevel = DifficultyLevel.Beginner,
            ExerciseType = ExerciseType.Strength,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Exercises.Add(exercise);

        _context.SaveChanges();
    }

    [TestMethod]
    public async Task SimulateUI_CompleteImageWorkflow()
    {
        Console.WriteLine("=== SIMULACIÓN COMPLETA DE UI: Workflow de Imágenes ===");

        // PASO 1: Simular agregado de imagen desde UI
        Console.WriteLine("\n🎬 PASO 1: Usuario selecciona imagen desde UI");
        var exerciseName = "Flexiones";
        var testImageBytes = CreateTestImageForUI("Flexiones Demo UI");

        // Simular el método SaveImageToDbAsync del UI
        Console.WriteLine($"💾 Guardando imagen para ejercicio: {exerciseName}");

        // Buscar ejercicio (como hace el UI)
        var norm = exerciseName.Trim().ToLower();
        var exercise = _context.Exercises.FirstOrDefault(e =>
            e.SpanishName.ToLower() == norm || e.Name.ToLower() == norm);

        Assert.IsNotNull(exercise, $"Ejercicio '{exerciseName}' debe existir");
        Console.WriteLine($"✅ Ejercicio encontrado: ID={exercise.Id}, Nombre='{exercise.SpanishName}'");

        // Crear upload object (como hace el UI)
        var upload = new ExerciseImageUpload
        {
            ImageData = testImageBytes,
            FileName = "flexiones_ui_test.png",
            ContentType = "image/png",
            Position = "demonstration",
            IsPrimary = true,
            Description = "Imagen de prueba desde UI"
        };

        Console.WriteLine($"📦 Upload creado: {upload.FileName}, {testImageBytes.Length:N0} bytes");

        // Guardar usando management service (como hace el UI)
        var saveResult = await _managementService.AddExerciseImageAsync(exercise.Id, upload);

        Assert.IsTrue(saveResult.Success, $"Guardado debe ser exitoso. Errores: {string.Join(", ", saveResult.Errors)}");
        Console.WriteLine($"🎉 GUARDADO EXITOSO: {saveResult.Message}");

        // PASO 2: Simular carga de imagen en UI
        Console.WriteLine("\n🎬 PASO 2: UI carga imagen para mostrar preview");

        // Buscar ejercicio por nombre (como hace LoadExerciseDetails)
        var exerciseForLoad = _context.Exercises.AsNoTracking().FirstOrDefault(e =>
            e.SpanishName.ToLower() == exerciseName.ToLower() ||
            e.Name.ToLower() == exerciseName.ToLower());

        Assert.IsNotNull(exerciseForLoad, "Ejercicio debe encontrarse para cargar imagen");
        Console.WriteLine($"🔍 Ejercicio encontrado para carga: ID={exerciseForLoad.Id}, Nombre={exerciseForLoad.SpanishName}");

        // Obtener imágenes (como hace el UI)
        var images = await _managementService.GetExerciseImagesAsync(exerciseForLoad.Id);
        Console.WriteLine($"📊 Imágenes encontradas: {images.Count}");

        Assert.IsTrue(images.Count > 0, "Debe haber al menos una imagen");

        // Aplicar lógica de selección del UI
        var preferred = images
            .OrderBy(i => i.ImagePosition == "demonstration" ? 0 : 1)
            .ThenBy(i => i.IsPrimary ? 0 : 1)
            .FirstOrDefault();

        Assert.IsNotNull(preferred, "Debe haber una imagen preferida");
        Console.WriteLine($"🎯 Imagen preferida seleccionada: ID={preferred.Id}, Position={preferred.ImagePosition}");

        // Mostrar metadata si existe (como hace el UI nuevo)
        string metadataInfo = "sin metadata";
        if (!string.IsNullOrEmpty(preferred.ImageMetadata))
        {
            try
            {
                var metadata = System.Text.Json.JsonSerializer.Deserialize<GymRoutineGenerator.Data.Models.ImageMetadata>(preferred.ImageMetadata);
                metadataInfo = metadata?.GetDisplaySummary() ?? "metadata inválida";
                Console.WriteLine($"📋 Metadata: {metadataInfo}");
            }
            catch (Exception metaEx)
            {
                Console.WriteLine($"⚠️ Error leyendo metadata: {metaEx.Message}");
                metadataInfo = "error leyendo metadata";
            }
        }

        // PASO 3: Simular carga de imagen en PictureBox
        Console.WriteLine("\n🎬 PASO 3: Cargar imagen en PictureBox (como hace UI)");

        Assert.IsNotNull(preferred.ImageData, "Imagen debe tener datos binarios");
        Assert.IsTrue(preferred.ImageData.Length > 0, "Datos de imagen no pueden estar vacíos");

        // Simular el código del UI para cargar imagen
        try
        {
            using var ms = new MemoryStream(preferred.ImageData);
            using var tempImage = Image.FromStream(ms);
            using var displayBitmap = new Bitmap(tempImage); // Esto es lo que se asigna a PictureBox.Image

            Console.WriteLine($"✅ ÉXITO: Imagen cargada para display - {displayBitmap.Width}x{displayBitmap.Height}");
            Console.WriteLine($"📸 Tamaño original: {tempImage.Width}x{tempImage.Height}");
            Console.WriteLine($"💽 Bytes en BD: {preferred.ImageData.Length:N0}");

            // Verificar que la imagen tiene contenido válido
            Assert.IsTrue(displayBitmap.Width > 0 && displayBitmap.Height > 0, "Imagen debe tener dimensiones válidas");

            Console.WriteLine($"🎨 Formato de pixel: {displayBitmap.PixelFormat}");
            Console.WriteLine($"📐 Resolución: {displayBitmap.HorizontalResolution}x{displayBitmap.VerticalResolution} DPI");

        }
        catch (Exception displayEx)
        {
            Console.WriteLine($"❌ ERROR cargando imagen para display: {displayEx.Message}");
            Assert.Fail($"Falló la carga de imagen en PictureBox: {displayEx.Message}");
        }

        // PASO 4: Verificar que todo el workflow funciona end-to-end
        Console.WriteLine("\n🎬 PASO 4: Verificación final del workflow");

        // Simular actualización de status (como hace el UI)
        var statusMessage = $"✅ ÉXITO: Imagen cargada desde BD ({preferred.ImageData.Length:N0} bytes)";
        Console.WriteLine($"📱 Status UI: {statusMessage}");

        // Verificar que tenemos toda la información necesaria
        Assert.IsNotNull(preferred.ImageData, "ImageData requerido");
        Assert.IsTrue(preferred.ImageData.Length > 1000, "ImageData debe tener tamaño razonable");
        Assert.AreEqual("demonstration", preferred.ImagePosition, "Position debe ser demonstration");
        Assert.IsTrue(preferred.IsPrimary, "Debe ser imagen primaria");
        Assert.IsFalse(string.IsNullOrEmpty(preferred.ImageMetadata), "Debe tener metadata JSON");

        Console.WriteLine("\n🏆 SIMULACIÓN UI COMPLETADA EXITOSAMENTE");
        Console.WriteLine("✅ El workflow completo de imágenes funciona correctamente");
        Console.WriteLine("✅ Las imágenes se pueden agregar, guardar, recuperar y mostrar");
        Console.WriteLine("✅ El sistema está listo para uso en producción");
    }

    [TestMethod]
    public async Task SimulateUI_MultiplexImages()
    {
        Console.WriteLine("=== SIMULACIÓN UI: Múltiples Imágenes ===");

        var exerciseId = 1;

        // Agregar múltiples imágenes en diferentes posiciones
        var images = new[]
        {
            new { pos = "start", primary = false, name = "flexiones_inicio.jpg" },
            new { pos = "demonstration", primary = true, name = "flexiones_demo.jpg" },
            new { pos = "end", primary = false, name = "flexiones_fin.jpg" }
        };

        foreach (var img in images)
        {
            var testBytes = CreateTestImageForUI($"Flexiones {img.pos}");
            var upload = new ExerciseImageUpload
            {
                ImageData = testBytes,
                FileName = img.name,
                ContentType = "image/jpeg",
                Position = img.pos,
                IsPrimary = img.primary,
                Description = $"Imagen de {img.pos}"
            };

            var result = await _managementService.AddExerciseImageAsync(exerciseId, upload);
            Assert.IsTrue(result.Success, $"Debe guardar imagen {img.pos}");
            Console.WriteLine($"✅ Guardada imagen {img.pos}: {img.name}");
        }

        // Simular lógica de selección del UI
        var allImages = await _managementService.GetExerciseImagesAsync(exerciseId);
        Console.WriteLine($"📊 Total imágenes: {allImages.Count}");

        // Test: UI debe seleccionar 'demonstration' como preferida
        var preferred = allImages
            .OrderBy(i => i.ImagePosition == "demonstration" ? 0 : 1)
            .ThenBy(i => i.IsPrimary ? 0 : 1)
            .FirstOrDefault();

        Assert.IsNotNull(preferred, "Debe haber imagen preferida");
        Assert.AreEqual("demonstration", preferred.ImagePosition, "Debe seleccionar 'demonstration'");
        Assert.IsTrue(preferred.IsPrimary, "La imagen 'demonstration' debe ser primaria");

        Console.WriteLine($"🎯 Imagen preferida correcta: {preferred.ImagePosition} (primary: {preferred.IsPrimary})");

        // Verificar que todas las imágenes tienen metadata
        foreach (var image in allImages)
        {
            Assert.IsFalse(string.IsNullOrEmpty(image.ImageMetadata), $"Imagen {image.ImagePosition} debe tener metadata");

            var metadata = System.Text.Json.JsonSerializer.Deserialize<GymRoutineGenerator.Data.Models.ImageMetadata>(image.ImageMetadata);
            Assert.IsNotNull(metadata, $"Metadata de {image.ImagePosition} debe ser válida");
            Assert.IsTrue(metadata.Width > 0 && metadata.Height > 0, $"Dimensiones de {image.ImagePosition} deben ser válidas");

            Console.WriteLine($"📋 {image.ImagePosition}: {metadata.GetDisplaySummary()}");
        }

        Console.WriteLine("🏆 Test de múltiples imágenes EXITOSO");
    }

    private byte[] CreateTestImageForUI(string text)
    {
        using var bitmap = new Bitmap(400, 300);
        using var graphics = Graphics.FromImage(bitmap);

        // Fondo más realista
        using var gradientBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
            new Rectangle(0, 0, 400, 300),
            Color.LightBlue, Color.LightGreen,
            System.Drawing.Drawing2D.LinearGradientMode.Vertical);

        graphics.FillRectangle(gradientBrush, 0, 0, 400, 300);

        // Borde
        using var pen = new Pen(Color.DarkBlue, 3);
        graphics.DrawRectangle(pen, 5, 5, 390, 290);

        // Texto principal
        using var font = new Font("Arial", 16, FontStyle.Bold);
        using var brush = new SolidBrush(Color.DarkBlue);

        var rect = new RectangleF(10, 100, 380, 100);
        var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        graphics.DrawString(text, font, brush, rect, format);

        // Información adicional
        using var smallFont = new Font("Arial", 10);
        graphics.DrawString($"Generado: {DateTime.Now:HH:mm:ss}", smallFont, brush, 10, 270);
        graphics.DrawString($"Para UI Test", smallFont, brush, 300, 270);

        // Simular contenido visual (círculo)
        using var fillBrush = new SolidBrush(Color.Yellow);
        graphics.FillEllipse(fillBrush, 150, 50, 100, 50);

        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        return stream.ToArray();
    }
}