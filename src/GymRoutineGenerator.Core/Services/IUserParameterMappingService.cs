namespace GymRoutineGenerator.Core.Services;

public interface IUserParameterMappingService
{
    Task<UserRoutineParameters> BuildUserParametersAsync(int userProfileId, CancellationToken cancellationToken = default);
}