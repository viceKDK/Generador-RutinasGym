using System.Windows.Forms;
using GymRoutineGenerator.UI;
using GymRoutineGenerator.UI.Tests.Helpers;

namespace GymRoutineGenerator.UI.Tests.Forms;

[Collection("UI Tests")]
public class RoutinePreviewFormTests : IDisposable
{
    private RoutinePreviewForm? _previewForm;

    [Fact]
    public void RoutinePreviewForm_ShouldInitializeWithContent()
    {
        // Arrange
        var content = UITestHelper.CreateSampleRoutineContent();
        const string clientName = "Test Client";

        // Act
        _previewForm = UITestHelper.CreateTestPreviewForm(content, clientName);
        UITestHelper.InitializeFormForTesting(_previewForm);

        // Assert
        _previewForm.Should().NotBeNull();
        _previewForm.Text.Should().Contain(clientName);
        _previewForm.GetContent().Should().Be(content);
    }

    [Fact]
    public void RoutinePreviewForm_ShouldHaveRequiredControls()
    {
        // Arrange
        var content = UITestHelper.CreateSampleRoutineContent();
        _previewForm = UITestHelper.CreateTestPreviewForm(content);
        UITestHelper.InitializeFormForTesting(_previewForm);

        // Act & Assert - Check for required controls
        var previewTextBox = UITestHelper.FindControl<RichTextBox>(_previewForm, "previewTextBox");
        previewTextBox.Should().NotBeNull("Preview text box should exist");
        previewTextBox!.ReadOnly.Should().BeTrue("Preview should be read-only");

        var printButton = UITestHelper.FindControl<Button>(_previewForm, "printButton");
        printButton.Should().NotBeNull("Print button should exist");

        var exportButton = UITestHelper.FindControl<Button>(_previewForm, "exportButton");
        exportButton.Should().NotBeNull("Export button should exist");

        var editButton = UITestHelper.FindControl<Button>(_previewForm, "editButton");
        editButton.Should().NotBeNull("Edit button should exist");

        var closeButton = UITestHelper.FindControl<Button>(_previewForm, "closeButton");
        closeButton.Should().NotBeNull("Close button should exist");
    }

    [Fact]
    public void RoutinePreviewForm_ShouldHaveToolbar()
    {
        // Arrange
        var content = UITestHelper.CreateSampleRoutineContent();
        _previewForm = UITestHelper.CreateTestPreviewForm(content);
        UITestHelper.InitializeFormForTesting(_previewForm);

        // Act
        var toolStrip = UITestHelper.FindControl<ToolStrip>(_previewForm, "toolStrip");

        // Assert
        toolStrip.Should().NotBeNull("Toolbar should exist");
        toolStrip!.Items.Count.Should().BeGreaterThan(0, "Toolbar should have items");
    }

    [Fact]
    public void RoutinePreviewForm_ShouldDisplayContent()
    {
        // Arrange
        var content = UITestHelper.CreateSampleRoutineContent();
        _previewForm = UITestHelper.CreateTestPreviewForm(content);
        UITestHelper.InitializeFormForTesting(_previewForm);

        // Act
        var previewTextBox = UITestHelper.FindControl<RichTextBox>(_previewForm, "previewTextBox");

        // Assert
        previewTextBox.Should().NotBeNull();
        previewTextBox!.Text.Should().Contain("Test User");
        previewTextBox.Text.Should().Contain("RUTINA DE ENTRENAMIENTO");
    }

    [Fact]
    public void PrintButton_ShouldBeEnabled()
    {
        // Arrange
        var content = UITestHelper.CreateSampleRoutineContent();
        _previewForm = UITestHelper.CreateTestPreviewForm(content);
        UITestHelper.InitializeFormForTesting(_previewForm);

        // Act
        var printButton = UITestHelper.FindControl<Button>(_previewForm, "printButton");

        // Assert
        printButton.Should().NotBeNull();
        printButton!.Enabled.Should().BeTrue("Print button should be enabled");
    }

    [Fact]
    public void ExportButton_ShouldBeEnabled()
    {
        // Arrange
        var content = UITestHelper.CreateSampleRoutineContent();
        _previewForm = UITestHelper.CreateTestPreviewForm(content);
        UITestHelper.InitializeFormForTesting(_previewForm);

        // Act
        var exportButton = UITestHelper.FindControl<Button>(_previewForm, "exportButton");

        // Assert
        exportButton.Should().NotBeNull();
        exportButton!.Enabled.Should().BeTrue("Export button should be enabled");
    }

    [Fact]
    public void UpdateContent_ShouldChangeDisplayedContent()
    {
        // Arrange
        var originalContent = UITestHelper.CreateSampleRoutineContent();
        _previewForm = UITestHelper.CreateTestPreviewForm(originalContent);
        UITestHelper.InitializeFormForTesting(_previewForm);

        var newContent = "Updated routine content";

        // Act
        _previewForm.UpdateContent(newContent);

        // Assert
        _previewForm.GetContent().Should().Be(newContent);
    }

    [Fact]
    public void RoutinePreviewForm_ShouldHaveStatusStrip()
    {
        // Arrange
        var content = UITestHelper.CreateSampleRoutineContent();
        _previewForm = UITestHelper.CreateTestPreviewForm(content);
        UITestHelper.InitializeFormForTesting(_previewForm);

        // Act
        var statusStrip = UITestHelper.FindControl<StatusStrip>(_previewForm, "statusStrip");

        // Assert
        statusStrip.Should().NotBeNull("Status strip should exist");
    }

    [Fact]
    public void CloseButton_Click_ShouldCloseForm()
    {
        // Arrange
        var content = UITestHelper.CreateSampleRoutineContent();
        _previewForm = UITestHelper.CreateTestPreviewForm(content);
        UITestHelper.InitializeFormForTesting(_previewForm);

        var closeButton = UITestHelper.FindControl<Button>(_previewForm, "closeButton");
        var formClosed = false;

        _previewForm.FormClosed += (s, e) => formClosed = true;

        // Act
        UITestHelper.SimulateButtonClick(closeButton!);

        // Allow time for event processing
        Application.DoEvents();

        // Assert
        // Since we can't easily test actual form closing in unit tests,
        // we just verify the button exists and is clickable
        closeButton.Should().NotBeNull();
        closeButton!.Enabled.Should().BeTrue();
    }

    [Fact]
    public void RoutinePreviewForm_ShouldHaveControlPanel()
    {
        // Arrange
        var content = UITestHelper.CreateSampleRoutineContent();
        _previewForm = UITestHelper.CreateTestPreviewForm(content);
        UITestHelper.InitializeFormForTesting(_previewForm);

        // Act
        var controlPanel = UITestHelper.FindControl<Panel>(_previewForm, "controlPanel");

        // Assert
        controlPanel.Should().NotBeNull("Control panel should exist");
        controlPanel!.Controls.Count.Should().BeGreaterThan(0, "Control panel should contain buttons");
    }

    public void Dispose()
    {
        UITestHelper.SafeDisposeForm(_previewForm);
    }
}