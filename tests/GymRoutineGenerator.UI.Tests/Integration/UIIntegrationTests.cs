using System.Windows.Forms;
using GymRoutineGenerator.UI;
using GymRoutineGenerator.UI.Tests.Helpers;

namespace GymRoutineGenerator.UI.Tests.Integration;

[Collection("UI Tests")]
public class UIIntegrationTests : IDisposable
{
    private MainForm? _mainForm;

    [Fact]
    public void FullWorkflow_GenerateAndPreviewRoutine_ShouldWork()
    {
        _mainForm = UITestHelper.CreateTestMainForm();
        UITestHelper.InitializeFormForTesting(_mainForm);
        UITestHelper.FillRequiredFields(_mainForm);

        var generateButton = UITestHelper.FindControl<Button>(_mainForm, "generateButton");
        var routineTextBox = UITestHelper.FindControl<RichTextBox>(_mainForm, "routineDisplayTextBox");

        UITestHelper.SimulateButtonClick(generateButton!);

        Application.DoEvents();
        Thread.Sleep(500);
        Application.DoEvents();

        generateButton!.Enabled.Should().BeTrue();
        routineTextBox!.Text.Should().NotBeEmpty();

        var previewButton = UITestHelper.FindControl<Button>(_mainForm, "previewButton");
        previewButton.Should().NotBeNull();
    }

    [Fact]
    public void FormInitialization_AllRequiredControlsShouldExist()
    {
        _mainForm = UITestHelper.CreateTestMainForm();
        UITestHelper.InitializeFormForTesting(_mainForm);

        UITestHelper.FindControl<TextBox>(_mainForm, "nameTextBox").Should().NotBeNull();
        UITestHelper.FindControl<NumericUpDown>(_mainForm, "ageNumericUpDown").Should().NotBeNull();
        UITestHelper.FindControl<ComboBox>(_mainForm, "genderComboBox").Should().NotBeNull();
        UITestHelper.FindControl<ComboBox>(_mainForm, "fitnessLevelComboBox").Should().NotBeNull();
        UITestHelper.FindControl<TrackBar>(_mainForm, "trainingDaysTrackBar").Should().NotBeNull();
        UITestHelper.FindControl<CheckedListBox>(_mainForm, "goalsCheckedListBox").Should().NotBeNull();

        UITestHelper.FindControl<Button>(_mainForm, "generateButton").Should().NotBeNull();
        UITestHelper.FindControl<Button>(_mainForm, "exportButton").Should().NotBeNull();
        UITestHelper.FindControl<Button>(_mainForm, "previewButton").Should().NotBeNull();

        UITestHelper.FindControl<RichTextBox>(_mainForm, "routineDisplayTextBox").Should().NotBeNull();
        UITestHelper.FindControl<ProgressBar>(_mainForm, "progressBar").Should().NotBeNull();
        UITestHelper.FindControl<Label>(_mainForm, "statusLabel").Should().NotBeNull();
    }

    [Fact]
    public void UserInputValidation_ShouldWorkCorrectly()
    {
        _mainForm = UITestHelper.CreateTestMainForm();
        UITestHelper.InitializeFormForTesting(_mainForm);

        var nameTextBox = UITestHelper.FindControl<TextBox>(_mainForm, "nameTextBox");
        var ageControl = UITestHelper.FindControl<NumericUpDown>(_mainForm, "ageNumericUpDown");
        var genderCombo = UITestHelper.FindControl<ComboBox>(_mainForm, "genderComboBox");

        UITestHelper.SetTextBoxValue(nameTextBox!, "Integration Test User");
        UITestHelper.SetNumericUpDownValue(ageControl!, 30);
        UITestHelper.SetComboBoxSelection(genderCombo!, 1);

        nameTextBox!.Text.Should().Be("Integration Test User");
        ageControl!.Value.Should().Be(30);
        genderCombo!.SelectedIndex.Should().Be(1);
    }

    [Fact]
    public void ProgressIndicatorIntegration_ShouldWorkWithMainForm()
    {
        _mainForm = UITestHelper.CreateTestMainForm();
        UITestHelper.InitializeFormForTesting(_mainForm);

        var progressBar = UITestHelper.FindControl<ProgressBar>(_mainForm, "progressBar");
        var statusLabel = UITestHelper.FindControl<Label>(_mainForm, "statusLabel");

        progressBar.Should().NotBeNull();
        statusLabel.Should().NotBeNull();

        progressBar!.Visible.Should().BeFalse();
        statusLabel!.Text.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ResponsiveLayout_ShouldHandleFormResize()
    {
        _mainForm = UITestHelper.CreateTestMainForm();
        UITestHelper.InitializeFormForTesting(_mainForm);

        var originalSize = _mainForm.Size;

        _mainForm.Size = new Size(originalSize.Width + 200, originalSize.Height + 100);
        Application.DoEvents();

        _mainForm.Size.Width.Should().BeGreaterThan(originalSize.Width);
        _mainForm.Size.Height.Should().BeGreaterThan(originalSize.Height);

        UITestHelper.FindControl<Button>(_mainForm, "generateButton").Should().NotBeNull();
    }

    [Fact]
    public void ButtonStates_ShouldUpdateCorrectlyDuringWorkflow()
    {
        _mainForm = UITestHelper.CreateTestMainForm();
        UITestHelper.InitializeFormForTesting(_mainForm);

        var generateButton = UITestHelper.FindControl<Button>(_mainForm, "generateButton");
        var exportButton = UITestHelper.FindControl<Button>(_mainForm, "exportButton");
        var previewButton = UITestHelper.FindControl<Button>(_mainForm, "previewButton");

        generateButton!.Enabled.Should().BeTrue();
        exportButton!.Enabled.Should().BeFalse();
        previewButton!.Enabled.Should().BeFalse();
    }

    [Fact]
    public void ErrorHandling_ShouldNotCrashApplication()
    {
        _mainForm = UITestHelper.CreateTestMainForm();
        UITestHelper.InitializeFormForTesting(_mainForm);

        var generateButton = UITestHelper.FindControl<Button>(_mainForm, "generateButton");

        var exception = Record.Exception(() => UITestHelper.SimulateButtonClick(generateButton!));

        exception.Should().BeNull();
        _mainForm.IsDisposed.Should().BeFalse();
    }

    public void Dispose()
    {
        UITestHelper.SafeDisposeForm(_mainForm);
    }
}
