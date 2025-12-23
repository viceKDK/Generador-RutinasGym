using System;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

class InsertMultipleTestImages
{
    static void Main()
    {
        var dbPath = @"C:\Users\vicen\OneDrive\Escritorio\apps\por hacer\app generacion rutinas gym\gymroutine.db";
        var connectionString = $"Data Source={dbPath};Version=3;";

        // Lista de ejercicios para agregar imágenes (vacía por defecto)
        var exercises = new[]
        {
            // Lista vacía para evitar insertar ejercicios hardcoded
            new { Name = "", SpanishName = "", Color = Color.Transparent }
        }.Where(e => !string.IsNullOrEmpty(e.Name)).ToArray();

        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();

            foreach (var exercise in exercises)
            {
                try
                {
                    // Buscar ID del ejercicio
                    int? exerciseId = null;
                    var getIdQuery = "SELECT Id FROM Exercises WHERE SpanishName LIKE @name OR Name LIKE @name LIMIT 1";
                    using (var cmd = new SQLiteCommand(getIdQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@name", $"%{exercise.SpanishName}%");
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            exerciseId = Convert.ToInt32(result);
                        }
                    }

                    if (exerciseId == null)
                    {
                        Console.WriteLine($"[!] Ejercicio '{exercise.SpanishName}' no encontrado, creándolo...");

                        // Crear ejercicio
                        var insertExerciseQuery = @"
                            INSERT INTO Exercises (Name, SpanishName, Description, Instructions, PrimaryMuscleGroupId, EquipmentTypeId, DifficultyLevel, ExerciseType, IsActive, CreatedAt)
                            VALUES (@name, @spanishName, '', '', 1, 1, 1, 0, 1, @createdAt);
                            SELECT last_insert_rowid();";

                        using (var cmd = new SQLiteCommand(insertExerciseQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@name", exercise.Name);
                            cmd.Parameters.AddWithValue("@spanishName", exercise.SpanishName);
                            cmd.Parameters.AddWithValue("@createdAt", DateTime.Now);
                            exerciseId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        Console.WriteLine($"    Ejercicio creado con ID: {exerciseId}");
                    }

                    // Crear imagen de prueba con color único
                    byte[] imageBytes;
                    using (var bitmap = new Bitmap(200, 200))
                    {
                        using (var graphics = Graphics.FromImage(bitmap))
                        {
                            graphics.Clear(exercise.Color);
                            using (var font = new Font("Arial", 16, FontStyle.Bold))
                            using (var brush = new SolidBrush(Color.White))
                            {
                                // Dibujar nombre en el centro
                                var text = exercise.SpanishName;
                                var textSize = graphics.MeasureString(text, font);
                                var x = (200 - textSize.Width) / 2;
                                var y = (200 - textSize.Height) / 2;
                                graphics.DrawString(text, font, brush, x, y);
                            }
                        }

                        using (var ms = new MemoryStream())
                        {
                            bitmap.Save(ms, ImageFormat.Png);
                            imageBytes = ms.ToArray();
                        }
                    }

                    Console.WriteLine($"[*] Procesando '{exercise.SpanishName}' (ID: {exerciseId})");
                    Console.WriteLine($"    Imagen creada: {imageBytes.Length} bytes");

                    // Eliminar imagen anterior si existe
                    var deleteQuery = "DELETE FROM ExerciseImages WHERE ExerciseId = @exerciseId";
                    using (var cmd = new SQLiteCommand(deleteQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@exerciseId", exerciseId.Value);
                        var deleted = cmd.ExecuteNonQuery();
                        if (deleted > 0)
                        {
                            Console.WriteLine($"    Imagenes anteriores eliminadas: {deleted}");
                        }
                    }

                    // Insertar nueva imagen
                    var insertQuery = @"
                        INSERT INTO ExerciseImages (ExerciseId, ImageData, ImagePath, ImagePosition, IsPrimary, Description)
                        VALUES (@exerciseId, @imageData, '', 'Front', 1, @description)";

                    using (var cmd = new SQLiteCommand(insertQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@exerciseId", exerciseId.Value);
                        cmd.Parameters.AddWithValue("@imageData", imageBytes);
                        cmd.Parameters.AddWithValue("@description", $"Test image for {exercise.SpanishName}");
                        var rows = cmd.ExecuteNonQuery();
                        Console.WriteLine($"    Filas insertadas: {rows}");
                    }

                    // Verificar
                    var checkQuery = "SELECT LENGTH(ImageData) FROM ExerciseImages WHERE ExerciseId = @exerciseId";
                    using (var cmd = new SQLiteCommand(checkQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@exerciseId", exerciseId.Value);
                        var size = cmd.ExecuteScalar();
                        Console.WriteLine($"    Verificacion - Tamaño en BD: {size} bytes");
                    }

                    Console.WriteLine($"[OK] '{exercise.SpanishName}' completado!");
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Error con '{exercise.SpanishName}': {ex.Message}");
                    Console.WriteLine();
                }
            }
        }

        Console.WriteLine("======================================");
        Console.WriteLine("PROCESO COMPLETADO!");
        Console.WriteLine($"Total ejercicios procesados: {exercises.Length}");
        Console.WriteLine("======================================");
    }
}
