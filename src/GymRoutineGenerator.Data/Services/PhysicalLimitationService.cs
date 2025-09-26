using Microsoft.EntityFrameworkCore;
using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Entities;

namespace GymRoutineGenerator.Data.Services;

public class PhysicalLimitationService : IPhysicalLimitationService
{
    private readonly GymRoutineContext _context;

    public PhysicalLimitationService(GymRoutineContext context)
    {
        _context = context;
    }

    public async Task<List<UserPhysicalLimitation>> SetUserPhysicalLimitationsAsync(int userProfileId, List<PhysicalLimitationRequest> limitations)
    {
        // Validate user profile exists
        var userProfile = await _context.UserProfiles.FindAsync(userProfileId);
        if (userProfile == null)
        {
            throw new ArgumentException($"Perfil de usuario con ID {userProfileId} no encontrado");
        }

        // Validate limitation types
        foreach (var limitation in limitations)
        {
            if (!Enum.IsDefined(typeof(LimitationType), limitation.LimitationType))
            {
                throw new ArgumentException($"Tipo de limitación no válido: {limitation.LimitationType}");
            }
        }

        // Clear existing limitations
        await ClearUserPhysicalLimitationsAsync(userProfileId);

        // Create new limitations
        var newLimitations = limitations.Select(req => new UserPhysicalLimitation
        {
            UserProfileId = userProfileId,
            LimitationType = req.LimitationType,
            Description = req.Description ?? GetDefaultDescription(req.LimitationType),
            CustomRestrictions = req.CustomRestrictions,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        _context.UserPhysicalLimitations.AddRange(newLimitations);
        await _context.SaveChangesAsync();

        return await GetUserPhysicalLimitationsAsync(userProfileId);
    }

    public async Task<List<UserPhysicalLimitation>> GetUserPhysicalLimitationsAsync(int userProfileId)
    {
        return await _context.UserPhysicalLimitations
            .Where(upl => upl.UserProfileId == userProfileId)
            .OrderBy(upl => upl.LimitationType)
            .ToListAsync();
    }

    public async Task<bool> ClearUserPhysicalLimitationsAsync(int userProfileId)
    {
        var existingLimitations = await _context.UserPhysicalLimitations
            .Where(upl => upl.UserProfileId == userProfileId)
            .ToListAsync();

        if (existingLimitations.Any())
        {
            _context.UserPhysicalLimitations.RemoveRange(existingLimitations);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<List<Exercise>> SearchExercisesForExclusionAsync(string searchTerm, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new List<Exercise>();
        }

        var normalizedSearchTerm = searchTerm.Trim().ToLower();

        return await _context.Exercises
            .Where(e => e.IsActive &&
                       (e.SpanishName.ToLower().Contains(normalizedSearchTerm) ||
                        e.Name.ToLower().Contains(normalizedSearchTerm)))
            .OrderBy(e => e.SpanishName)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IntensityRecommendation> GetRecommendedIntensityAsync(List<LimitationType> limitations)
    {
        await Task.CompletedTask; // Async placeholder

        if (limitations.Count == 0)
        {
            return new IntensityRecommendation
            {
                RecommendedLevel = 3,
                RecommendationReason = "Sin limitaciones reportadas - intensidad normal recomendada",
                Precautions = new List<string> { "Siempre caliente antes del ejercicio", "Escuche a su cuerpo" }
            };
        }

        var criticalConditions = new[]
        {
            LimitationType.ProblemasCardivasculares,
            LimitationType.LesionReciente,
            LimitationType.Embarazo
        };

        if (limitations.Any(l => criticalConditions.Contains(l)))
        {
            return new IntensityRecommendation
            {
                RecommendedLevel = 1,
                RecommendationReason = "Condiciones médicas críticas detectadas - intensidad muy baja recomendada",
                Precautions = new List<string>
                {
                    "Supervisión médica requerida",
                    "Pare inmediatamente si experimenta dolor",
                    "Monitore signos vitales constantemente",
                    "Consulte con su médico antes de continuar"
                }
            };
        }

        if (limitations.Count >= 3)
        {
            return new IntensityRecommendation
            {
                RecommendedLevel = 2,
                RecommendationReason = "Múltiples limitaciones detectadas - intensidad baja recomendada",
                Precautions = new List<string>
                {
                    "Progrese gradualmente",
                    "Evite movimientos que causen dolor",
                    "Considere ejercicios de bajo impacto",
                    "Consulte con un profesional del fitness"
                }
            };
        }

        return new IntensityRecommendation
        {
            RecommendedLevel = 2,
            RecommendationReason = "Limitaciones físicas detectadas - intensidad reducida recomendada",
            Precautions = new List<string>
            {
                "Modifique ejercicios según sea necesario",
                "Evite movimientos específicos relacionados con la limitación",
                "Mantenga buena forma en todos los ejercicios"
            }
        };
    }

    public async Task<SafetyGuidelines> GetSafetyGuidelinesAsync(List<LimitationType> limitations)
    {
        await Task.CompletedTask; // Async placeholder

        var guidelines = new SafetyGuidelines
        {
            MedicalDisclaimer = "Esta información no reemplaza el consejo médico profesional. Consulte siempre con un médico antes de comenzar cualquier programa de ejercicio."
        };

        if (limitations.Count == 0)
        {
            guidelines.GeneralPrecautions.AddRange(new[]
            {
                "Caliente adecuadamente antes del ejercicio",
                "Mantenga una hidratación adecuada",
                "Escuche a su cuerpo y descanse cuando sea necesario",
                "Use la técnica correcta en todos los ejercicios"
            });

            return guidelines;
        }

        // Add general precautions for any limitations
        guidelines.GeneralPrecautions.AddRange(new[]
        {
            "Consulte con un profesional médico antes de comenzar",
            "Pare inmediatamente si experimenta dolor",
            "Caliente más tiempo y con menor intensidad",
            "Progrese gradualmente en intensidad y duración"
        });

        // Add specific guidelines based on limitation types
        foreach (var limitation in limitations.Distinct())
        {
            AddSpecificGuidelines(guidelines, limitation);
        }

        return guidelines;
    }

    private void AddSpecificGuidelines(SafetyGuidelines guidelines, LimitationType limitation)
    {
        switch (limitation)
        {
            case LimitationType.ProblemasEspalda:
                guidelines.ExercisesToAvoid.AddRange(new[]
                {
                    "Peso muerto con técnica incorrecta",
                    "Sentadillas profundas sin supervisión",
                    "Flexiones hacia adelante extremas",
                    "Ejercicios que requieran torsión de columna"
                });
                guidelines.RecommendedModifications.AddRange(new[]
                {
                    "Use soporte lumbar cuando sea necesario",
                    "Mantenga la columna neutra en todos los ejercicios",
                    "Fortalezca los músculos del core gradualmente"
                });
                break;

            case LimitationType.ProblemasRodilla:
                guidelines.ExercisesToAvoid.AddRange(new[]
                {
                    "Saltos de alto impacto",
                    "Sentadillas muy profundas",
                    "Lunges con peso excesivo",
                    "Correr en superficies duras"
                });
                guidelines.RecommendedModifications.AddRange(new[]
                {
                    "Use ejercicios de bajo impacto",
                    "Limite el rango de movimiento en sentadillas",
                    "Fortalezca los músculos que soportan la rodilla"
                });
                break;

            case LimitationType.ProblemasHombro:
                guidelines.ExercisesToAvoid.AddRange(new[]
                {
                    "Press militar con peso completo",
                    "Elevaciones laterales por encima de 90°",
                    "Fondos en paralelas",
                    "Dominadas con agarre muy amplio"
                });
                guidelines.RecommendedModifications.AddRange(new[]
                {
                    "Limite el rango de movimiento en ejercicios de hombro",
                    "Fortalezca el manguito rotador",
                    "Use bandas elásticas para ejercicios de rehabilitación"
                });
                break;

            case LimitationType.ProblemasCardivasculares:
                guidelines.ExercisesToAvoid.AddRange(new[]
                {
                    "Ejercicios de muy alta intensidad",
                    "Entrenamientos HIIT sin supervisión",
                    "Ejercicios que causen falta de aire extrema",
                    "Levantamiento de peso muy pesado"
                });
                guidelines.RecommendedModifications.AddRange(new[]
                {
                    "Monitoree la frecuencia cardíaca constantemente",
                    "Mantenga la intensidad en zona aeróbica",
                    "Descanse más tiempo entre series"
                });
                break;

            case LimitationType.Embarazo:
                guidelines.ExercisesToAvoid.AddRange(new[]
                {
                    "Ejercicios boca abajo después del primer trimestre",
                    "Ejercicios de alto impacto",
                    "Ejercicios que requieran equilibrio complejo",
                    "Levantamiento de peso pesado"
                });
                guidelines.RecommendedModifications.AddRange(new[]
                {
                    "Use ejercicios de bajo impacto",
                    "Evite sobrecalentamiento",
                    "Manténgase hidratada",
                    "Adapte ejercicios según el trimestre"
                });
                break;

            case LimitationType.Artritis:
                guidelines.ExercisesToAvoid.AddRange(new[]
                {
                    "Ejercicios de alto impacto en articulaciones afectadas",
                    "Movimientos repetitivos excesivos",
                    "Ejercicios durante brotes de dolor",
                    "Peso excesivo en articulaciones comprometidas"
                });
                guidelines.RecommendedModifications.AddRange(new[]
                {
                    "Use ejercicios acuáticos cuando sea posible",
                    "Aplique calor antes del ejercicio",
                    "Incluya ejercicios de rango de movimiento",
                    "Evite ejercitarse durante inflamación aguda"
                });
                break;
        }
    }

    private string GetDefaultDescription(LimitationType limitationType)
    {
        return limitationType switch
        {
            LimitationType.ProblemasEspalda => "Problemas de espalda - requiere cuidado especial con ejercicios de columna",
            LimitationType.ProblemasRodilla => "Problemas de rodilla - evitar ejercicios de alto impacto",
            LimitationType.ProblemasHombro => "Problemas de hombro - limitar rango de movimiento en ejercicios de hombro",
            LimitationType.ProblemasCardivasculares => "Problemas cardiovasculares - mantener intensidad controlada",
            LimitationType.LesionReciente => "Lesión reciente - requiere período de recuperación y rehabilitación",
            LimitationType.Embarazo => "Embarazo - adaptar ejercicios según trimestre y condición",
            LimitationType.Artritis => "Artritis - evitar ejercicios de alto impacto en articulaciones afectadas",
            LimitationType.Personalizada => "Limitación personalizada - ver restricciones específicas",
            _ => "Limitación física general"
        };
    }
}