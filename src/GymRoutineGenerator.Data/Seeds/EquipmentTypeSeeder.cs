using GymRoutineGenerator.Data.Context;
using GymRoutineGenerator.Data.Entities;

namespace GymRoutineGenerator.Data.Seeds;

public static class EquipmentTypeSeeder
{
    public static void SeedData(GymRoutineContext context)
    {
        if (context.EquipmentTypes.Any())
            return; // Already seeded

        var equipmentTypes = new List<EquipmentType>
        {
            new EquipmentType
            {
                Name = "Bodyweight",
                SpanishName = "Peso Corporal",
                Description = "Ejercicios que utilizan solo el peso del cuerpo",
                IsRequired = false
            },
            new EquipmentType
            {
                Name = "Free Weights",
                SpanishName = "Pesas Libres",
                Description = "Mancuernas, barras, discos",
                IsRequired = true
            },
            new EquipmentType
            {
                Name = "Machines",
                SpanishName = "Máquinas",
                Description = "Máquinas de gimnasio con poleas y pesas",
                IsRequired = true
            },
            new EquipmentType
            {
                Name = "Resistance Bands",
                SpanishName = "Bandas Elásticas",
                Description = "Bandas de resistencia y ligas",
                IsRequired = true
            },
            new EquipmentType
            {
                Name = "Kettlebells",
                SpanishName = "Kettlebells",
                Description = "Pesas rusas para ejercicios funcionales",
                IsRequired = true
            },
            new EquipmentType
            {
                Name = "Pull-up Bar",
                SpanishName = "Barra de Dominadas",
                Description = "Barra fija para ejercicios de tracción",
                IsRequired = true
            },
            new EquipmentType
            {
                Name = "Medicine Ball",
                SpanishName = "Balón Medicinal",
                Description = "Pelota pesada para ejercicios funcionales",
                IsRequired = true
            },
            new EquipmentType
            {
                Name = "TRX",
                SpanishName = "TRX",
                Description = "Sistema de entrenamiento en suspensión",
                IsRequired = true
            }
        };

        context.EquipmentTypes.AddRange(equipmentTypes);
        context.SaveChanges();
    }
}