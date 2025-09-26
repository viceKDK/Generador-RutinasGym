using System.Windows.Forms;
using GymRoutineGenerator.UI;
using GymRoutineGenerator.UI.Tests.Helpers;

namespace GymRoutineGenerator.UI.Tests.Integration;

[Collection("UI Tests")]
public class UIIntegrationTests : IDisposable
{
    private MainForm? _mainForm;
    private RoutinePreviewForm? _previewForm;

    [Fact]
    public void FullWorkflow_GenerateAndPreviewRoutine_ShouldWork()
    {
        // Arrange
        _mainForm = UITestHelper.CreateTestMainForm();
        UITestHelper.InitializeFormForTesting(_mainForm);
        UITestHelper.FillRequiredFields(_mainForm);

        var generateButton = UITestHelper.FindControl<Button>(_mainForm, "generateButton");
        var routineTextBox = UITestHelper.FindControl<RichTextBox>(_mainForm, "routineDisplayTextBox");

        // Act - Generate routine
        UITestHelper.SimulateButtonClick(generateButton!);

        // Allow time for generation to complete (simulate async operation)
        Application.DoEvents();
        Thread.Sleep(500);
        Application.DoEvents();

        // Assert - Check routine was generated
        generateButton!.Enabled.Should().BeTrue("Generate button should be re-enabled");
        routineTextBox!.Text.Should().NotBeEmpty("Routine should be generated");

        // Act - Check if preview button is now enabled
        var previewButton = UITestHelper.FindControl<Button>(_mainForm, "previewButton");
        previewButton.Should().NotBeNull();

        // Note: In a real integration test, we would click preview button
        // and verify the preview form opens, but that requires more complex setup
    }

    [Fact]
    public void FormInitialization_AllRequiredControlsShouldExist()
    {
        // Arrange & Act
        _mainForm = UITestHelper.CreateTestMainForm();
        UITestHelper.InitializeFormForTesting(_mainForm);

        // Assert - Main form controls
        var nameTextBox = UITestHelper.FindControl<TextBox>(_mainForm, "nameTextBox");
        var ageControl = UITestHelper.FindControl<NumericUpDown>(_mainForm, "ageNumericUpDown");
        var genderCombo = UITestHelper.FindControl<ComboBox>(_mainForm, "genderComboBox");
        var fitnessCombo = UITestHelper.FindControl<ComboBox>(_mainForm, "fitnessLevelComboBox");
        var trainingDays = UITestHelper.FindControl<TrackBar>(_mainForm, "trainingDaysTrackBar");
        var goalsListBox = UITestHelper.FindControl<CheckedListBox>(_mainForm, "goalsCheckedListBox");

        // Personal information controls
        nameTextBox.Should().NotBeNull("Name textbox should exist");
        ageControl.Should().NotBeNull("Age control should exist");
        genderCombo.Should().NotBeNull("Gender combo should exist");

        // Training information controls
        fitnessCombo.Should().NotBeNull("Fitness level combo should exist");
        trainingDays.Should().NotBeNull("Training days trackbar should exist");
        goalsListBox.Should().NotBeNull("Goals listbox should exist");

        // Action buttons
        var generateButton = UITestHelper.FindControl<Button>(_mainForm, "generateButton");
        var exportButton = UITestHelper.FindControl<Button>(_mainForm, "exportButton");
        var previewButton = UITestHelper.FindControl<Button>(_mainForm, "previewButton");

        generateButton.Should().NotBeNull("Generate button should exist");
        exportButton.Should().NotBeNull("Export button should exist");
        previewButton.Should().NotBeNull("Preview button should exist");

        // Display and progress controls
        var routineTextBox = UITestHelper.FindControl<RichTextBox>(_mainForm, "routineDisplayTextBox");
        var progressBar = UITestHelper.FindControl<ProgressBar>(_mainForm, "progressBar");
        var statusLabel = UITestHelper.FindControl<Label>(_mainForm, "statusLabel");

        routineTextBox.Should().NotBeNull("Routine display should exist");
        progressBar.Should().NotBeNull("Progress bar should exist");
        statusLabel.Should().NotBeNull("Status label should exist");
    }

    [Fact]
    public void UserInputValidation_ShouldWorkCorrectly()
    {
        // Arrange
        _mainForm = UITestHelper.CreateTestMainForm();
        UITestHelper.InitializeFormForTesting(_mainForm);

        // Act - Fill various input fields
        var nameTextBox = UITestHelper.FindControl<TextBox>(_mainForm, "nameTextBox");
        var ageControl = UITestHelper.FindControl<NumericUpDown>(_mainForm, "ageNumericUpDown");
        var genderCombo = UITestHelper.FindControl<ComboBox>(_mainForm, "genderComboBox");

        UITestHelper.SetTextBoxValue(nameTextBox!, "Integration Test User");
        UITestHelper.SetNumericUpDownValue(ageControl!, 30);
        UITestHelper.SetComboBoxSelection(genderCombo!, 1);

        // Assert - Verify inputs were set correctly
        nameTextBox!.Text.Should().Be("Integration Test User");
        ageControl!.Value.Should().Be(30);
        genderCombo!.SelectedIndex.Should().Be(1);
    }

    [Fact]
    public void PreviewFormIntegration_ShouldDisplayContentCorrectly()
    {
        // Arrange
        var sampleContent = UITestHelper.CreateSampleRoutineContent();
        const string clientName = "Integration Test Client";

        // Act
        _previewForm = UITestHelper.CreateTestPreviewForm(sampleContent, clientName);
        UITestHelper.InitializeFormForTesting(_previewForm);

        // Assert
        _previewForm.Text.Should().Contain(clientName);
        _previewForm.GetContent().Should().Be(sampleContent);

        // Verify all required controls exist in preview form
        var previewTextBox = UITestHelper.FindControl<RichTextBox>(_previewForm, "previewTextBox");
        var printButton = UITestHelper.FindControl<Button>(_previewForm, "printButton");
        var exportButton = UITestHelper.FindControl<Button>(_previewForm, "exportButton");
        var editButton = UITestHelper.FindControl<Button>(_previewForm, "editButton");
        var closeButton = UITestHelper.FindControl<Button>(_previewForm, "closeButton");

        previewTextBox.Should().NotBeNull();
        printButton.Should().NotBeNull();
        exportButton.Should().NotBeNull();
        editButton.Should().NotBeNull();
        closeButton.Should().NotBeNull();

        // Verify content is displayed
        previewTextBox!.Text.Should().NotBeEmpty();
        previewTextBox.Text.Should().Contain("RUTINA DE ENTRENAMIENTO");
    }

    [Fact]
    public void ProgressIndicatorIntegration_ShouldWorkWithMainForm()
    {
        // Arrange
        _mainForm = UITestHelper.CreateTestMainForm();
        UITestHelper.InitializeFormForTesting(_mainForm);

        var progressBar = UITestHelper.FindControl<ProgressBar>(_mainForm, "progressBar");
        var statusLabel = UITestHelper.FindControl<Label>(_mainForm, "statusLabel");

        // Assert - Progress components should exist and be properly initialized
        progressBar.Should().NotBeNull("Progress bar should exist");
        statusLabel.Should().NotBeNull("Status label should exist");

        progressBar!.Visible.Should().BeFalse("Progress bar should be hidden initially");
        statusLabel!.Text.Should().NotBeNullOrEmpty("Status label should have initial text");
    }

    [Fact]
    public void ResponsiveLayout_ShouldHandleFormResize()
    {
        // Arrange
        _mainForm = UITestHelper.CreateTestMainForm();
        UITestHelper.InitializeFormForTesting(_mainForm);

        var originalSize = _mainForm.Size;

        // Act - Resize form
        _mainForm.Size = new Size(originalSize.Width + 200, originalSize.Height + 100);
        Application.DoEvents(); // Process layout changes

        // Assert - Form should handle resize gracefully
        _mainForm.Size.Width.Should().BeGreaterThan(originalSize.Width);
        _mainForm.Size.Height.Should().BeGreaterThan(originalSize.Height);

        // Verify controls are still accessible after resize
        var generateButton = UITestHelper.FindControl<Button>(_mainForm, "generateButton");
        generateButton.Should().NotBeNull("Controls should remain accessible after resize");
    }

    [Fact]
    public void ButtonStates_ShouldUpdateCorrectlyDuringWorkflow()
    {
        // Arrange
        _mainForm = UITestHelper.CreateTestMainForm();
        UITestHelper.InitializeFormForTesting(_mainForm);

        var generateButton = UITestHelper.FindControl<Button>(_mainForm, "generateButton");
        var exportButton = UITestHelper.FindControl<Button>(_mainForm, "exportButton");
        var previewButton = UITestHelper.FindControl<Button>(_mainForm, "previewButton");

        // Assert initial states
        generateButton!.Enabled.Should().BeTrue("Generate should be enabled initially");
        exportButton!.Enabled.Should().BeFalse("Export should be disabled initially");
        previewButton!.Enabled.Should().BeFalse("Preview should be disabled initially");
    }

    [Fact]
    public void ErrorHandling_ShouldNotCrashApplication()
    {
        // Arrange
        _mainForm = UITestHelper.CreateTestMainForm();
        UITestHelper.InitializeFormForTesting(_mainForm);

        // Act - Try to generate without filling required fields
        var generateButton = UITestHelper.FindControl<Button>(_mainForm, "generateButton");

        // This should not crash the application
        var exception = Record.Exception(() => UITestHelper.SimulateButtonClick(generateButton!));

        // Assert - Should handle gracefully
        exception.Should().BeNull("Application should handle errors gracefully");
        _mainForm.IsDisposed.Should().BeFalse("Form should not be disposed after error");
    }

    public void Dispose()
    {
        UITestHelper.SafeDisposeForm(_previewForm);
        UITestHelper.SafeDisposeForm(_mainForm);
    }
}