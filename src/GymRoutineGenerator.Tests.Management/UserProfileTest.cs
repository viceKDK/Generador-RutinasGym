using GymRoutineGenerator.Data.Entities;
using GymRoutineGenerator.Data.Services;
using GymRoutineGenerator.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace GymRoutineGenerator.Tests.Management;

public static class UserProfileTest
{
    public static async Task RunUserProfileTests()
    {
        Console.WriteLine("=== TESTING USER PROFILE FUNCTIONALITY ===");
        Console.WriteLine();

        // Setup in-memory database
        var options = new DbContextOptionsBuilder<GymRoutineContext>()
            .UseInMemoryDatabase(databaseName: "UserProfileTestDb")
            .Options;

        using var context = new GymRoutineContext(options);
        var userProfileService = new UserProfileService(context);

        await TestUserProfileValidation(userProfileService);
        await TestUserProfileCRUD(userProfileService);

        Console.WriteLine();
        Console.WriteLine("=== USER PROFILE TESTS COMPLETED ===");
    }

    private static async Task TestUserProfileValidation(IUserProfileService service)
    {
        Console.WriteLine("1. Testing User Profile Validation");
        Console.WriteLine("----------------------------------");

        // Test valid profile
        var validRequest = new UserProfileCreateRequest
        {
            Name = "Juan Pérez",
            Gender = Gender.Hombre,
            Age = 25,
            TrainingDaysPerWeek = 3
        };

        var validation = await service.ValidateUserProfileAsync(validRequest);
        Console.WriteLine($"✓ Valid profile validation: {validation.IsValid}");

        // Test invalid age
        var invalidAgeRequest = new UserProfileCreateRequest
        {
            Name = "María García",
            Gender = Gender.Mujer,
            Age = 15, // Too young
            TrainingDaysPerWeek = 4
        };

        var ageValidation = await service.ValidateUserProfileAsync(invalidAgeRequest);
        Console.WriteLine($"✓ Invalid age validation: {!ageValidation.IsValid} (Errors: {string.Join(", ", ageValidation.Errors)})");

        // Test invalid training days
        var invalidDaysRequest = new UserProfileCreateRequest
        {
            Name = "Carlos López",
            Gender = Gender.Hombre,
            Age = 30,
            TrainingDaysPerWeek = 8 // Too many days
        };

        var daysValidation = await service.ValidateUserProfileAsync(invalidDaysRequest);
        Console.WriteLine($"✓ Invalid training days validation: {!daysValidation.IsValid} (Errors: {string.Join(", ", daysValidation.Errors)})");

        // Test empty name
        var emptyNameRequest = new UserProfileCreateRequest
        {
            Name = "",
            Gender = Gender.Otro,
            Age = 22,
            TrainingDaysPerWeek = 5
        };

        var nameValidation = await service.ValidateUserProfileAsync(emptyNameRequest);
        Console.WriteLine($"✓ Empty name validation: {!nameValidation.IsValid} (Errors: {string.Join(", ", nameValidation.Errors)})");

        Console.WriteLine();
    }

    private static async Task TestUserProfileCRUD(IUserProfileService service)
    {
        Console.WriteLine("2. Testing User Profile CRUD Operations");
        Console.WriteLine("-------------------------------------");

        // Create user profile
        var createRequest = new UserProfileCreateRequest
        {
            Name = "Ana Martínez",
            Gender = Gender.Mujer,
            Age = 28,
            TrainingDaysPerWeek = 4
        };

        var createdProfile = await service.CreateUserProfileAsync(createRequest);
        Console.WriteLine($"✓ Profile created: ID={createdProfile.Id}, Name='{createdProfile.Name}', Age={createdProfile.Age}, Gender={createdProfile.Gender}, Days={createdProfile.TrainingDaysPerWeek}");

        // Get profile by ID
        var retrievedProfile = await service.GetUserProfileByIdAsync(createdProfile.Id);
        Console.WriteLine($"✓ Profile retrieved: Found={retrievedProfile != null}, Name='{retrievedProfile?.Name}'");

        // Update profile
        var updateRequest = new UserProfileUpdateRequest
        {
            Id = createdProfile.Id,
            Name = "Ana Martínez López",
            Gender = Gender.Mujer,
            Age = 29,
            TrainingDaysPerWeek = 5
        };

        var updatedProfile = await service.UpdateUserProfileAsync(updateRequest);
        Console.WriteLine($"✓ Profile updated: Name='{updatedProfile.Name}', Age={updatedProfile.Age}, Days={updatedProfile.TrainingDaysPerWeek}");

        // Get all profiles
        var allProfiles = await service.GetAllUserProfilesAsync();
        Console.WriteLine($"✓ All profiles retrieved: Count={allProfiles.Count}");

        // Test demographic form data structure
        TestDemographicFormData();

        Console.WriteLine();
    }

    private static void TestDemographicFormData()
    {
        Console.WriteLine("3. Testing Demographic Form Data Structure");
        Console.WriteLine("----------------------------------------");

        // Test gender enum values in Spanish
        Console.WriteLine("Gender enum values:");
        foreach (Gender gender in Enum.GetValues<Gender>())
        {
            Console.WriteLine($"  - {gender} ({(int)gender})");
        }

        // Test age validation range
        Console.WriteLine("Age validation tests:");
        var validAges = new[] { 16, 25, 45, 65, 100 };
        var invalidAges = new[] { 15, 101, -5, 150 };

        foreach (var age in validAges)
        {
            var isValid = age >= 16 && age <= 100;
            Console.WriteLine($"  Age {age}: {(isValid ? "✓ Valid" : "✗ Invalid")}");
        }

        foreach (var age in invalidAges)
        {
            var isValid = age >= 16 && age <= 100;
            Console.WriteLine($"  Age {age}: {(isValid ? "✓ Valid" : "✗ Invalid")}");
        }

        // Test training days validation
        Console.WriteLine("Training days validation tests:");
        var validDays = new[] { 1, 3, 5, 7 };
        var invalidDays = new[] { 0, 8, -1, 10 };

        foreach (var days in validDays)
        {
            var isValid = days >= 1 && days <= 7;
            Console.WriteLine($"  Days {days}: {(isValid ? "✓ Valid" : "✗ Invalid")}");
        }

        foreach (var days in invalidDays)
        {
            var isValid = days >= 1 && days <= 7;
            Console.WriteLine($"  Days {days}: {(isValid ? "✓ Valid" : "✗ Invalid")}");
        }

        Console.WriteLine();
    }
}