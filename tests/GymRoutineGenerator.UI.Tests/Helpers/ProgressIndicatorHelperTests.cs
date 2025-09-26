using System.Drawing;
using System.Windows.Forms;
using GymRoutineGenerator.UI;

namespace GymRoutineGenerator.UI.Tests.Helpers;

[Collection("UI Tests")]
public class ProgressIndicatorHelperTests : IDisposable
{
    private Form? _testForm;
    private ProgressBar? _progressBar;
    private Label? _statusLabel;
    private ProgressIndicatorHelper? _progressHelper;

    [Fact]
    public void ProgressIndicatorHelper_ShouldInitializeCorrectly()
    {
        // Arrange
        SetupTestForm();

        // Act
        _progressHelper = new ProgressIndicatorHelper(_progressBar!, _statusLabel!, _testForm!);

        // Assert
        _progressHelper.Should().NotBeNull();
    }

    [Fact]
    public async Task ShowProgress_ShouldMakeProgressBarVisible()
    {
        // Arrange
        SetupTestForm();
        _progressHelper = new ProgressIndicatorHelper(_progressBar!, _statusLabel!, _testForm!);

        // Act
        await _progressHelper.ShowProgress();

        // Assert
        _progressBar!.Visible.Should().BeTrue("Progress bar should be visible after ShowProgress");
    }

    [Fact]
    public async Task HideProgress_ShouldHideProgressBar()
    {
        // Arrange
        SetupTestForm();
        _progressHelper = new ProgressIndicatorHelper(_progressBar!, _statusLabel!, _testForm!);

        // First show, then hide
        await _progressHelper.ShowProgress();
        await _progressHelper.HideProgress();

        // Assert
        _progressBar!.Visible.Should().BeFalse("Progress bar should be hidden after HideProgress");
        _progressBar.Value.Should().Be(0, "Progress value should be reset to 0");
    }

    [Fact]
    public async Task UpdateStatus_ShouldChangeStatusLabelText()
    {
        // Arrange
        SetupTestForm();
        _progressHelper = new ProgressIndicatorHelper(_progressBar!, _statusLabel!, _testForm!);
        const string testMessage = "Test status message";
        var testColor = Color.Blue;

        // Act
        await _progressHelper.UpdateStatus(testMessage, testColor);

        // Assert
        _statusLabel!.Text.Should().Be(testMessage);
        _statusLabel.ForeColor.Should().Be(testColor);
    }

    [Fact]
    public void SetMarqueeMode_ShouldChangeProgressBarStyle()
    {
        // Arrange
        SetupTestForm();
        _progressHelper = new ProgressIndicatorHelper(_progressBar!, _statusLabel!, _testForm!);

        // Act
        _progressHelper.SetMarqueeMode();

        // Assert
        _progressBar!.Style.Should().Be(ProgressBarStyle.Marquee);
        _progressBar.MarqueeAnimationSpeed.Should().Be(50);
    }

    [Fact]
    public void SetContinuousMode_ShouldChangeProgressBarStyle()
    {
        // Arrange
        SetupTestForm();
        _progressHelper = new ProgressIndicatorHelper(_progressBar!, _statusLabel!, _testForm!);

        // Act
        _progressHelper.SetContinuousMode();

        // Assert
        _progressBar!.Style.Should().Be(ProgressBarStyle.Continuous);
        _progressBar.MarqueeAnimationSpeed.Should().Be(0);
    }

    [Fact]
    public async Task SimulateProgress_ShouldUpdateProgressValue()
    {
        // Arrange
        SetupTestForm();
        _progressHelper = new ProgressIndicatorHelper(_progressBar!, _statusLabel!, _testForm!);
        const string statusText = "Simulating progress...";
        var statusColor = Color.Orange;

        // Act
        await _progressHelper.SimulateProgress(100, statusText, statusColor);

        // Assert
        _progressBar!.Value.Should().Be(100, "Progress should reach 100%");
        _statusLabel!.Text.Should().Be(statusText);
        _statusLabel.ForeColor.Should().Be(statusColor);
    }

    [Fact]
    public async Task ShowProgressWithSteps_ShouldExecuteAllSteps()
    {
        // Arrange
        SetupTestForm();
        _progressHelper = new ProgressIndicatorHelper(_progressBar!, _statusLabel!, _testForm!);

        var steps = new[] { "Step 1", "Step 2", "Step 3" };
        var executedSteps = new List<int>();

        // Act
        await _progressHelper.ShowProgressWithSteps(steps, async (stepIndex) =>
        {
            executedSteps.Add(stepIndex);
            await Task.Delay(10); // Small delay to simulate work
        });

        // Assert
        executedSteps.Should().HaveCount(3, "All steps should be executed");
        executedSteps.Should().ContainInOrder(0, 1, 2).And.NotBeEmpty();
        _progressBar!.Value.Should().Be(steps.Length, "Progress should complete all steps");
    }

    [Fact]
    public async Task PulseProgress_ShouldAnimateProgressBar()
    {
        // Arrange
        SetupTestForm();
        _progressHelper = new ProgressIndicatorHelper(_progressBar!, _statusLabel!, _testForm!);
        var originalHeight = _progressBar!.Height;

        // Act
        await _progressHelper.PulseProgress(1);

        // Assert
        _progressBar.Height.Should().Be(originalHeight, "Progress bar should return to original height after pulse");
    }

    [Fact]
    public void ProgressSteps_ShouldHaveDefinedSteps()
    {
        // Act & Assert
        ProgressSteps.RoutineGeneration.Should().NotBeEmpty("Routine generation steps should be defined");
        ProgressSteps.RoutineGeneration.Should().HaveCountGreaterThan(5, "Should have multiple routine generation steps");

        ProgressSteps.DocumentExport.Should().NotBeEmpty("Document export steps should be defined");
        ProgressSteps.DocumentExport.Should().HaveCountGreaterThan(3, "Should have multiple export steps");

        ProgressSteps.DataValidation.Should().NotBeEmpty("Data validation steps should be defined");
        ProgressSteps.AIProcessing.Should().NotBeEmpty("AI processing steps should be defined");
    }

    [Fact]
    public void ProgressSteps_RoutineGeneration_ShouldHaveDescriptiveSteps()
    {
        // Act
        var steps = ProgressSteps.RoutineGeneration;

        // Assert
        steps.Should().Contain(step => step.Contains("usuario"), "Should have user-related step");
        steps.Should().Contain(step => step.Contains("ejercicios"), "Should have exercises-related step");
        steps.Should().Contain(step => step.Contains("IA") || step.Contains("recomendaciones"), "Should have AI-related step");
    }

    [Fact]
    public void ProgressSteps_DocumentExport_ShouldHaveDescriptiveSteps()
    {
        // Act
        var steps = ProgressSteps.DocumentExport;

        // Assert
        steps.Should().Contain(step => step.Contains("contenido"), "Should have content preparation step");
        steps.Should().Contain(step => step.Contains("formato"), "Should have formatting step");
        steps.Should().Contain(step => step.Contains("archivo"), "Should have file saving step");
    }

    private void SetupTestForm()
    {
        _testForm = new Form
        {
            Size = new Size(400, 300),
            WindowState = FormWindowState.Normal
        };

        _progressBar = new ProgressBar
        {
            Size = new Size(200, 25),
            Location = new Point(10, 10),
            Visible = false
        };

        _statusLabel = new Label
        {
            Size = new Size(200, 20),
            Location = new Point(10, 50),
            Text = "Ready"
        };

        _testForm.Controls.Add(_progressBar);
        _testForm.Controls.Add(_statusLabel);

        UITestHelper.InitializeFormForTesting(_testForm);
    }

    public void Dispose()
    {
        UITestHelper.SafeDisposeForm(_testForm);
    }
}