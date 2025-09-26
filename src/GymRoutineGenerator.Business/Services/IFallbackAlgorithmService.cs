using GymRoutineGenerator.Core.Enums;

namespace GymRoutineGenerator.Business.Services;

public interface IFallbackAlgorithmService
{
    Task<string> GenerateBasicRoutineAsync(Gender gender, int age, int trainingDays, List<EquipmentType> equipment);
}