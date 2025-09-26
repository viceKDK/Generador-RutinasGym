using System.Windows.Forms;
using GymRoutineGenerator.UI;
using GymRoutineGenerator.UI.Tests.Helpers;

namespace GymRoutineGenerator.UI.Tests.Forms;

[Collection("UI Tests")]
public class MainFormTests : IDisposable
{
    private MainForm? _mainForm;

    [Fact]
    public void MainForm_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        _mainForm = UITestHelper.CreateTestMainForm();
        UITestHelper.InitializeFormForTesting(_mainForm);

        // Assert
        _mainForm.Should().NotBeNull();
        _mainForm.Text.Should().Contain("Generador de Rutinas");
        _mainForm.WindowState.Should().Be(FormWindowState.Normal);
    }

    [Fact]
    public void MainForm_ShouldHaveRequiredControls()
    {
        // Arrange
        _mainForm = UITestHelper.CreateTestMainForm();
        UITestHelper.InitializeFormForTesting(_mainForm);

        // Act & Assert - Check for required controls
        var nameTextBox = UITestHelper.FindControl<TextBox>(_mainForm, "nameTextBox");
        nameTextBox.Should().NotBeNull("Name text box should exist");

        var generateButton = UITestHelper.FindControl<Button>(_mainForm, "generateButton");
        generateButton.Should().NotBeNull("Generate button should exist");

        var exportButton = UITestHelper.FindControl<Button>(_mainForm, "exportButton");
        exportButton.Should().NotBeNull("Export button should exist");

        var previewButton = UITestHelper.FindControl<Button>(_mainForm, "previewButton");
        previewButton.Should().NotBeNull("Preview button should exist");
    }

    [Fact]
    public void GenerateButton_ShouldBeEnabledInitially()
    {
        // Arrange
        _mainForm = UITestHelper.CreateTestMainForm();
        UITestHelper.InitializeFormForTesting(_mainForm);

        // Act
        var generateButton = UITestHelper.FindControl<Button>(_mainForm, "generateButton");

        // Assert
        generateButton.Should().NotBeNull();
        generateButton!.Enabled.Should().BeTrue("Generate button should be enabled initially");
    }

    [Fact]
    public void ExportButton_ShouldBeDisabledInitially()
    {
        // Arrange
        _mainForm = UITestHelper.CreateTestMainForm();
        UITestHelper.InitializeFormForTesting(_mainForm);

        // Act
        var exportButton = UITestHelper.FindControl<Button>(_mainForm, "exportButton");

        // Assert
        exportButton.Should().NotBeNull();
        exportButton!.Enabled.Should().BeFalse("Export button should be disabled initially");
    }

    [Fact]
    public void PreviewButton_ShouldBeDisabledInitially()
    {
        // Arrange
        _mainForm = UITestHelper.CreateTestMainForm();
        UITestHelper.InitializeFormForTesting(_mainForm);

        // Act
        var previewButton = UITestHelper.FindControl<Button>(_mainForm, "previewButton");

        // Assert
        previewButton.Should().NotBeNull();
        previewButton!.Enabled.Should().BeFalse("Preview button should be disabled initially");
    }

    [Fact]
    public void NameTextBox_ShouldAcceptInput()
    {
        // Arrange
        _mainForm = UITestHelper.CreateTestMainForm();
        UITestHelper.InitializeFormForTesting(_mainForm);
        var nameTextBox = UITestHelper.FindControl<TextBox>(_mainForm, "nameTextBox");

        // Act
        UITestHelper.SetTextBoxValue(nameTextBox!, "Test User");

        // Assert
        nameTextBox!.Text.Should().Be("Test User");
    }

    [Fact]
    public void AgeControl_ShouldAcceptValidValues()
    {
        // Arrange
        _mainForm = UITestHelper.CreateTestMainForm();
        UITestHelper.InitializeFormForTesting(_mainForm);
        var ageControl = UITestHelper.FindControl<NumericUpDown>(_mainForm, "ageNumericUpDown");

        // Act
        UITestHelper.SetNumericUpDownValue(ageControl!, 25);

        // Assert
        ageControl!.Value.Should().Be(25);
    }

    [Fact]
    public void GenderComboBox_ShouldAllowSelection()
    {
        // Arrange
        _mainForm = UITestHelper.CreateTestMainForm();
        UITestHelper.InitializeFormForTesting(_mainForm);
        var genderCombo = UITestHelper.FindControl<ComboBox>(_mainForm, "genderComboBox");

        // Act
        UITestHelper.SetComboBoxSelection(genderCombo!, 0);

        // Assert
        genderCombo!.SelectedIndex.Should().Be(0);
    }

    [Fact]
    public void GoalsListBox_ShouldAllowMultipleSelections()
    {
        // Arrange
        _mainForm = UITestHelper.CreateTestMainForm();
        UITestHelper.InitializeFormForTesting(_mainForm);
        var goalsListBox = UITestHelper.FindControl<CheckedListBox>(_mainForm, "goalsCheckedListBox");

        // Act
        if (goalsListBox != null && goalsListBox.Items.Count > 0)
        {
            UITestHelper.SetCheckedListBoxItems(goalsListBox, 0);

            // Assert
            goalsListBox.CheckedItems.Count.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public void MainForm_ShouldHaveProgressBar()
    {
        // Arrange
        _mainForm = UITestHelper.CreateTestMainForm();
        UITestHelper.InitializeFormForTesting(_mainForm);

        // Act
        var progressBar = UITestHelper.FindControl<ProgressBar>(_mainForm, "progressBar");

        // Assert
        progressBar.Should().NotBeNull("Progress bar should exist");
        progressBar!.Visible.Should().BeFalse("Progress bar should be hidden initially");
    }

    [Fact]
    public void MainForm_ShouldHaveStatusLabel()
    {
        // Arrange
        _mainForm = UITestHelper.CreateTestMainForm();
        UITestHelper.InitializeFormForTesting(_mainForm);

        // Act
        var statusLabel = UITestHelper.FindControl<Label>(_mainForm, "statusLabel");

        // Assert
        statusLabel.Should().NotBeNull("Status label should exist");
    }

    [Fact]
    public void MainForm_ShouldHaveRoutineDisplayArea()
    {
        // Arrange
        _mainForm = UITestHelper.CreateTestMainForm();
        UITestHelper.InitializeFormForTesting(_mainForm);

        // Act
        var routineTextBox = UITestHelper.FindControl<RichTextBox>(_mainForm, "routineDisplayTextBox");

        // Assert
        routineTextBox.Should().NotBeNull("Routine display area should exist");
        routineTextBox!.ReadOnly.Should().BeTrue("Routine display should be read-only");
    }

    [Fact]
    public async Task GenerateButton_Click_ShouldUpdateUI()
    {
        // Arrange
        _mainForm = UITestHelper.CreateTestMainForm();
        UITestHelper.InitializeFormForTesting(_mainForm);
        UITestHelper.FillRequiredFields(_mainForm);

        var generateButton = UITestHelper.FindControl<Button>(_mainForm, "generateButton");

        // Act
        UITestHelper.SimulateButtonClick(generateButton!);

        // Give some time for async operations
        await Task.Delay(100);
        Application.DoEvents();

        // Assert
        generateButton!.Enabled.Should().BeTrue("Generate button should be re-enabled after operation");
    }

    public void Dispose()
    {
        UITestHelper.SafeDisposeForm(_mainForm);
    }
}

/// <summary>
/// Collection definition to ensure UI tests run sequentially
/// </summary>
[CollectionDefinition("UI Tests", DisableParallelization = true)]
public class UITestCollection
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}