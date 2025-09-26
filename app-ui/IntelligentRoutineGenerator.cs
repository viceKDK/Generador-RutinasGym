using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GymRoutineGenerator.UI
{
    public class IntelligentRoutineGenerator
    {
        private readonly Dictionary<string, List<Exercise>> exerciseDatabase;
        private readonly Random random;
        private readonly ExerciseImageDatabase imageDatabase;

        public IntelligentRoutineGenerator()
        {
            random = new Random();
            imageDatabase = new ExerciseImageDatabase();
            exerciseDatabase = InitializeExerciseDatabase();
        }

        public string GeneratePersonalizedRoutine(UserProfile profile)
        {
            var routine = new StringBuilder();

            // Header personalizado
            routine.AppendLine($" RUTINA PERSONALIZADA PARA {profile.Name.ToUpper()}");
            routine.AppendLine("".PadRight(50, ' '));
            routine.AppendLine();

            // Información personal
            routine.AppendLine(" INFORMACIÓN DEL CLIENTE:");
            routine.AppendLine("");
            routine.AppendLine($"  Nombre: {profile.Name,-21} ");
            routine.AppendLine($"  Edad: {profile.Age} aos{new string(' ', Math.Max(0, 20 - $"{profile.Age} aos".Length))} ");
            routine.AppendLine($"  Gnero: {profile.Gender,-20} ");
            routine.AppendLine($"  Nivel: {profile.FitnessLevel,-21} ");
            routine.AppendLine($"  Frecuencia: {profile.TrainingDays} das/semana{new string(' ', Math.Max(0, 12 - $"{profile.TrainingDays} das/semana".Length))} ");
            routine.AppendLine("");
            routine.AppendLine();

            // Objetivos
            routine.AppendLine(" OBJETIVOS SELECCIONADOS:");
            routine.AppendLine("");
            foreach (var goal in profile.Goals)
            {
                routine.AppendLine($"  {goal,-29} ");
            }
            routine.AppendLine("");
            routine.AppendLine();

            // Generar rutina personalizada
            var workoutPlan = CreatePersonalizedWorkout(profile);

            routine.AppendLine(" PLAN DE ENTRENAMIENTO PERSONALIZADO:");
            routine.AppendLine("".PadRight(50, ' '));
            routine.AppendLine();

            // Agregar análisis de perfil
            routine.AppendLine(" ANÁLISIS DE PERFIL:");
            routine.AppendLine(AnalyzeProfile(profile));
            routine.AppendLine();

            // Agregar das de entrenamiento
            for (int i = 0; i < workoutPlan.Count; i++)
            {
                routine.AppendLine($" DA {i + 1} - {workoutPlan[i].Name.ToUpper()}");
                routine.AppendLine("".PadRight(40, ' '));

                foreach (var exercise in workoutPlan[i].Exercises)
                {
                    routine.AppendLine($" {exercise.Name}");
                    routine.AppendLine($"    {exercise.SetsAndReps}");
                    routine.AppendLine($"    {exercise.Instructions}");

                    // Buscar imagen en la base de datos RAG
                    var imageInfo = imageDatabase.FindExerciseImage(exercise.Name);
                    if (imageInfo != null && imageDatabase.HasImageForExercise(exercise.Name))
                    {
                        routine.AppendLine($"    Imagen disponible: {imageInfo.ExerciseName}");
                        routine.AppendLine($"    Msculos: {string.Join(", ", imageInfo.MuscleGroups)}");
                    }
                    else if (!string.IsNullOrEmpty(exercise.ImageUrl))
                    {
                        routine.AppendLine($"    Imagen: {exercise.ImageUrl}");
                    }
                    else
                    {
                        routine.AppendLine($"    Sin imagen disponible - Agregar en Gestor de Imgenes");
                    }
                    routine.AppendLine();
                }
                routine.AppendLine();
            }

            // Recomendaciones especficas
            routine.AppendLine(" RECOMENDACIONES PERSONALIZADAS:");
            routine.AppendLine("".PadRight(50, ' '));
            var recommendations = GetPersonalizedRecommendations(profile);
            foreach (var rec in recommendations)
            {
                routine.AppendLine($" {rec}");
            }
            routine.AppendLine();

            // Plan de progresin
            routine.AppendLine(" PLAN DE PROGRESIN:");
            routine.AppendLine(GetProgressionPlan(profile));

            routine.AppendLine();
            routine.AppendLine(" RUTINA PERSONALIZADA COMPLETADA!");
            routine.AppendLine(" Adaptada específicamente para tu perfil");
            routine.AppendLine("".PadRight(50, ' '));

            return routine.ToString();
        }

        private List<WorkoutDay> CreatePersonalizedWorkout(UserProfile profile)
        {
            var workoutDays = new List<WorkoutDay>();

            if (profile.Age >= 50 && profile.FitnessLevel == " Principiante")
            {
                // Rutina para adultos mayores principiantes
                workoutDays = CreateSeniorBeginnerWorkout(profile);
            }
            else if (profile.Age >= 40 && profile.FitnessLevel == " Principiante")
            {
                // Rutina para adultos principiantes
                workoutDays = CreateAdultBeginnerWorkout(profile);
            }
            else if (profile.FitnessLevel == " Principiante")
            {
                // Rutina para jvenes principiantes
                workoutDays = CreateYoungBeginnerWorkout(profile);
            }
            else if (profile.FitnessLevel == " Intermedio")
            {
                workoutDays = CreateIntermediateWorkout(profile);
            }
            else // Avanzado
            {
                workoutDays = CreateAdvancedWorkout(profile);
            }

            // Ajustar por nmero de das
            return workoutDays.Take(profile.TrainingDays).ToList();
        }

        private List<WorkoutDay> CreateSeniorBeginnerWorkout(UserProfile profile)
        {
            var workouts = new List<WorkoutDay>();

            // Da 1: Fuerza funcional suave
            workouts.Add(new WorkoutDay
            {
                Name = "Fuerza Funcional",
                Exercises = new List<Exercise>
                {
                    new Exercise
                    {
                        Name = "Sentadillas en silla",
                        SetsAndReps = "2 series x 8-10 repeticiones",
                        Instructions = "Sintate y levntate de una silla sin usar las manos",
                        ImageUrl = "https://example.com/chair-squats.jpg"
                    },
                    new Exercise
                    {
                        Name = "Flexiones en pared",
                        SetsAndReps = "2 series x 5-8 repeticiones",
                        Instructions = "De pie frente a la pared, empuja suavemente",
                        ImageUrl = "https://example.com/wall-pushups.jpg"
                    },
                    new Exercise
                    {
                        Name = "Elevaciones de brazos con peso ligero",
                        SetsAndReps = "2 series x 10-12 repeticiones",
                        Instructions = "Usa botellas de agua (500ml) como peso",
                        ImageUrl = "https://example.com/arm-raises.jpg"
                    },
                    new Exercise
                    {
                        Name = "Caminata estacionaria",
                        SetsAndReps = "5-10 minutos",
                        Instructions = "Camina en el lugar, mantn un ritmo cmodo",
                        ImageUrl = "https://example.com/stationary-walk.jpg"
                    },
                    new Exercise
                    {
                        Name = "Estiramientos suaves",
                        SetsAndReps = "5-10 minutos",
                        Instructions = "Estira cuello, hombros, brazos y piernas",
                        ImageUrl = "https://example.com/gentle-stretches.jpg"
                    }
                }
            });

            // Da 2: Equilibrio y movilidad
            if (profile.TrainingDays >= 2)
            {
                workouts.Add(new WorkoutDay
                {
                    Name = "Equilibrio y Movilidad",
                    Exercises = new List<Exercise>
                    {
                        new Exercise
                        {
                            Name = "Equilibrio en un pie (con apoyo)",
                            SetsAndReps = "2 series x 15-30 segundos cada pie",
                            Instructions = "Apyate en una silla, mantn equilibrio en un pie",
                            ImageUrl = "https://example.com/balance-support.jpg"
                        },
                        new Exercise
                        {
                            Name = "Elevaciones de talones",
                            SetsAndReps = "2 series x 10-15 repeticiones",
                            Instructions = "Levanta los talones, mantn el equilibrio",
                            ImageUrl = "https://example.com/heel-raises.jpg"
                        },
                        new Exercise
                        {
                            Name = "Rotaciones de hombros",
                            SetsAndReps = "2 series x 10 en cada direccin",
                            Instructions = "Crculos suaves hacia adelante y atrs",
                            ImageUrl = "https://example.com/shoulder-rolls.jpg"
                        },
                        new Exercise
                        {
                            Name = "Flexiones de rodillas sentado",
                            SetsAndReps = "2 series x 10-12 repeticiones",
                            Instructions = "Sentado, eleva las rodillas alternadamente",
                            ImageUrl = "https://example.com/seated-knee-lifts.jpg"
                        }
                    }
                });
            }

            return workouts;
        }

        private List<WorkoutDay> CreateYoungBeginnerWorkout(UserProfile profile)
        {
            var workouts = new List<WorkoutDay>();

            workouts.Add(new WorkoutDay
            {
                Name = "Cuerpo Completo",
                Exercises = new List<Exercise>
                {
                    new Exercise
                    {
                        Name = "Sentadillas corporales",
                        SetsAndReps = "3 series x 10-12 repeticiones",
                        Instructions = "Baja hasta que los muslos estn paralelos al suelo",
                        ImageUrl = "https://example.com/bodyweight-squats.jpg"
                    },
                    new Exercise
                    {
                        Name = "Flexiones de rodillas",
                        SetsAndReps = "3 series x 5-8 repeticiones",
                        Instructions = "Flexiones con rodillas apoyadas en el suelo",
                        ImageUrl = "https://example.com/knee-pushups.jpg"
                    },
                    new Exercise
                    {
                        Name = "Plancha",
                        SetsAndReps = "3 series x 15-30 segundos",
                        Instructions = "Mantn el cuerpo recto como una tabla",
                        ImageUrl = "https://example.com/plank.jpg"
                    },
                    new Exercise
                    {
                        Name = "Zancadas estticas",
                        SetsAndReps = "3 series x 8 por pierna",
                        Instructions = "Baja la rodilla trasera sin tocar el suelo",
                        ImageUrl = "https://example.com/static-lunges.jpg"
                    }
                }
            });

            return workouts;
        }

        private List<WorkoutDay> CreateIntermediateWorkout(UserProfile profile)
        {
            var workouts = new List<WorkoutDay>
            {
                new WorkoutDay
                {
                    Name = "Tren Superior",
                    Exercises = new List<Exercise>
                    {
                        new Exercise
                        {
                            Name = "Flexiones estndar",
                            SetsAndReps = "3 series x 8-12 repeticiones",
                            Instructions = "Flexiones completas manteniendo forma correcta",
                            ImageUrl = "https://example.com/pushups.jpg"
                        },
                        new Exercise
                        {
                            Name = "Remo con mancuernas",
                            SetsAndReps = "3 series x 10-12 repeticiones",
                            Instructions = "Inclnate hacia adelante, jala hacia el pecho",
                            ImageUrl = "https://example.com/dumbbell-rows.jpg"
                        },
                        new Exercise
                        {
                            Name = "Press de hombros",
                            SetsAndReps = "3 series x 8-10 repeticiones",
                            Instructions = "Empuja las mancuernas hacia arriba",
                            ImageUrl = "https://example.com/shoulder-press.jpg"
                        }
                    }
                },
                new WorkoutDay
                {
                    Name = "Tren Inferior",
                    Exercises = new List<Exercise>
                    {
                        new Exercise
                        {
                            Name = "Sentadillas con peso",
                            SetsAndReps = "4 series x 10-12 repeticiones",
                            Instructions = "Sostn mancuernas a los lados",
                            ImageUrl = "https://example.com/weighted-squats.jpg"
                        },
                        new Exercise
                        {
                            Name = "Peso muerto con mancuernas",
                            SetsAndReps = "3 series x 8-10 repeticiones",
                            Instructions = "Mantn la espalda recta al bajar",
                            ImageUrl = "https://example.com/dumbbell-deadlift.jpg"
                        }
                    }
                }
            };

            return workouts;
        }

        private List<WorkoutDay> CreateAdvancedWorkout(UserProfile profile)
        {
            // Rutina avanzada con ejercicios ms desafiantes
            return new List<WorkoutDay>
            {
                new WorkoutDay
                {
                    Name = "Fuerza Mxima",
                    Exercises = new List<Exercise>
                    {
                        new Exercise
                        {
                            Name = "Sentadillas con barra",
                            SetsAndReps = "4 series x 6-8 repeticiones",
                            Instructions = "Carga progresiva, tcnica perfecta",
                            ImageUrl = "https://example.com/barbell-squats.jpg"
                        }
                    }
                }
            };
        }

        private List<WorkoutDay> CreateAdultBeginnerWorkout(UserProfile profile)
        {
            var workouts = new List<WorkoutDay>();

            workouts.Add(new WorkoutDay
            {
                Name = "Acondicionamiento Base",
                Exercises = new List<Exercise>
                {
                    new Exercise
                    {
                        Name = "Sentadillas asistidas",
                        SetsAndReps = "2 series x 8-10 repeticiones",
                        Instructions = "Usa una silla como apoyo si es necesario",
                        ImageUrl = "https://example.com/assisted-squats.jpg"
                    },
                    new Exercise
                    {
                        Name = "Flexiones inclinadas",
                        SetsAndReps = "2 series x 6-10 repeticiones",
                        Instructions = "Manos en superficie elevada (escaln, banco)",
                        ImageUrl = "https://example.com/incline-pushups.jpg"
                    },
                    new Exercise
                    {
                        Name = "Caminata en el lugar",
                        SetsAndReps = "10-15 minutos",
                        Instructions = "Eleva las rodillas, mantn ritmo moderado",
                        ImageUrl = "https://example.com/marching.jpg"
                    }
                }
            });

            return workouts;
        }

        private string AnalyzeProfile(UserProfile profile)
        {
            var analysis = new StringBuilder();

            if (profile.Age >= 55)
            {
                analysis.AppendLine(" CONSIDERACIONES ESPECIALES:");
                analysis.AppendLine(" Enfoque en movilidad y equilibrio");
                analysis.AppendLine(" Ejercicios de bajo impacto");
                analysis.AppendLine(" Calentamiento extendido necesario");
                analysis.AppendLine(" Progresin muy gradual");
            }
            else if (profile.Age >= 40)
            {
                analysis.AppendLine(" CONSIDERACIONES ESPECIALES:");
                analysis.AppendLine(" nfasis en prevencin de lesiones");
                analysis.AppendLine(" Tiempo de recuperacin aumentado");
                analysis.AppendLine(" Fortalecimiento del core importante");
            }

            if (profile.FitnessLevel == " Principiante")
            {
                analysis.AppendLine(" Aprendizaje de tcnica fundamental");
                analysis.AppendLine(" Construccin de base cardiovascular");
                analysis.AppendLine(" Desarrollo de fuerza funcional");
            }

            return analysis.ToString();
        }

        private List<string> GetPersonalizedRecommendations(UserProfile profile)
        {
            var recommendations = new List<string>();

            if (profile.Age >= 55)
            {
                recommendations.Add(" Consulta mdica antes de comenzar");
                recommendations.Add(" Comienza muy gradualmente");
                recommendations.Add(" Incluye ejercicios de equilibrio diariamente");
                recommendations.Add(" Mantn hidratacin constante");
                recommendations.Add(" Descansa 48-72h entre sesiones");
            }
            else if (profile.Age >= 40)
            {
                recommendations.Add(" Calentamiento de 10-15 minutos obligatorio");
                recommendations.Add(" Estiramientos post-ejercicio esenciales");
                recommendations.Add(" Permite recuperacin adecuada");
            }

            if (profile.FitnessLevel == " Principiante")
            {
                recommendations.Add(" Enfcate en aprender la tcnica correcta");
                recommendations.Add(" Progresa lentamente en intensidad");
                recommendations.Add(" Considera trabajar con un entrenador al inicio");
            }

            // Recomendaciones por gnero
            if (profile.Gender.Contains("Mujer"))
            {
                recommendations.Add(" Incluye ejercicios de fortalecimiento seo");
                if (profile.Age >= 50)
                {
                    recommendations.Add(" Enfoque especial en fuerza para prevenir osteoporosis");
                }
            }

            return recommendations;
        }

        private string GetProgressionPlan(UserProfile profile)
        {
            var plan = new StringBuilder();

            if (profile.Age >= 55 && profile.FitnessLevel == " Principiante")
            {
                plan.AppendLine(" SEMANA 1-2: Adaptacin inicial");
                plan.AppendLine("    2 das/semana, 20-30 minutos");
                plan.AppendLine("    Movimientos bsicos y equilibrio");
                plan.AppendLine();
                plan.AppendLine(" SEMANA 3-6: Construccin de base");
                plan.AppendLine("    Agregar 1 serie ms a ejercicios bsicos");
                plan.AppendLine("    Aumentar tiempo de caminata gradualmente");
                plan.AppendLine();
                plan.AppendLine(" SEMANA 7+: Mantenimiento y mejora");
                plan.AppendLine("    Considera agregar un da ms si te sientes cmoda");
                plan.AppendLine("    Introduce variaciones de ejercicios bsicos");
            }
            else if (profile.FitnessLevel == " Principiante")
            {
                plan.AppendLine(" SEMANA 1-4: Base de movimientos");
                plan.AppendLine(" SEMANA 5-8: Aumento de volumen");
                plan.AppendLine(" SEMANA 9+: Progresin a intermedio");
            }

            return plan.ToString();
        }

        private Dictionary<string, List<Exercise>> InitializeExerciseDatabase()
        {
            return new Dictionary<string, List<Exercise>>
            {
                ["senior_beginner"] = new List<Exercise>(),
                ["adult_beginner"] = new List<Exercise>(),
                ["young_beginner"] = new List<Exercise>(),
                ["intermediate"] = new List<Exercise>(),
                ["advanced"] = new List<Exercise>()
            };
        }
    }

    public class UserProfile
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string Gender { get; set; } = "";
        public string FitnessLevel { get; set; } = "";
        public int TrainingDays { get; set; }
        public List<string> Goals { get; set; } = new List<string>();
    }

    public class WorkoutDay
    {
        public string Name { get; set; } = "";
        public List<Exercise> Exercises { get; set; } = new List<Exercise>();
    }

    public class Exercise
    {
        public string Name { get; set; } = "";
        public string SetsAndReps { get; set; } = "";
        public string Instructions { get; set; } = "";
        public string ImageUrl { get; set; } = "";
    }
}
