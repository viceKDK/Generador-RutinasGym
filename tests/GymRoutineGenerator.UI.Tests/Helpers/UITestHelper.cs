using System.Drawing;
using System.Windows.Forms;
using GymRoutineGenerator.UI;

namespace GymRoutineGenerator.UI.Tests.Helpers;

/// <summary>
/// Helper class for UI testing with Windows Forms
/// </summary>
public static class UITestHelper
{
    /// <summary>
    /// Creates a test MainForm instance with mocked dependencies
    /// </summary>
    public static MainForm CreateTestMainForm()
    {
        // Since MainForm constructor sets up services, we need to handle this carefully
        return new MainForm();
    }

    /// <summary>
    /// Creates a test RoutinePreviewForm with sample content
    /// </summary>
    public static RoutinePreviewForm CreateTestPreviewForm(string content = "Test routine content", string clientName = "Test Client")
    {
        return new RoutinePreviewForm(content, clientName);
    }

    /// <summary>
    /// Simulates a button click event
    /// </summary>
    public static void SimulateButtonClick(Button button)
    {
        if (button == null) throw new ArgumentNullException(nameof(button));

        // Simulate the click event
        button.PerformClick();
    }

    /// <summary>
    /// Sets text in a TextBox control
    /// </summary>
    public static void SetTextBoxValue(TextBox textBox, string value)
    {
        if (textBox == null) throw new ArgumentNullException(nameof(textBox));

        textBox.Text = value;
        // TextChanged event will be triggered automatically by setting Text property
    }

    /// <summary>
    /// Sets value in a NumericUpDown control
    /// </summary>
    public static void SetNumericUpDownValue(NumericUpDown control, decimal value)
    {
        if (control == null) throw new ArgumentNullException(nameof(control));

        control.Value = Math.Max(control.Minimum, Math.Min(control.Maximum, value));
    }

    /// <summary>
    /// Sets selected index in a ComboBox
    /// </summary>
    public static void SetComboBoxSelection(ComboBox comboBox, int index)
    {
        if (comboBox == null) throw new ArgumentNullException(nameof(comboBox));

        if (index >= 0 && index < comboBox.Items.Count)
        {
            comboBox.SelectedIndex = index;
        }
    }

    /// <summary>
    /// Sets TrackBar value
    /// </summary>
    public static void SetTrackBarValue(TrackBar trackBar, int value)
    {
        if (trackBar == null) throw new ArgumentNullException(nameof(trackBar));

        trackBar.Value = Math.Max(trackBar.Minimum, Math.Min(trackBar.Maximum, value));
    }

    /// <summary>
    /// Checks items in a CheckedListBox
    /// </summary>
    public static void SetCheckedListBoxItems(CheckedListBox checkedListBox, params int[] indices)
    {
        if (checkedListBox == null) throw new ArgumentNullException(nameof(checkedListBox));

        foreach (int index in indices)
        {
            if (index >= 0 && index < checkedListBox.Items.Count)
            {
                checkedListBox.SetItemChecked(index, true);
            }
        }
    }

    /// <summary>
    /// Waits for a form to be shown and ready for interaction
    /// </summary>
    public static async Task WaitForFormReady(Form form, int timeoutMs = 5000)
    {
        if (form == null) throw new ArgumentNullException(nameof(form));

        var startTime = DateTime.Now;

        while (!form.Visible || !form.IsHandleCreated)
        {
            if ((DateTime.Now - startTime).TotalMilliseconds > timeoutMs)
            {
                throw new TimeoutException($"Form was not ready within {timeoutMs}ms");
            }

            await Task.Delay(50);
            Application.DoEvents();
        }
    }

    /// <summary>
    /// Simulates form loading and handle creation
    /// </summary>
    public static void InitializeFormForTesting(Form form)
    {
        if (form == null) throw new ArgumentNullException(nameof(form));

        // Create the handle to initialize the form
        var handle = form.Handle;

        // Process any pending messages
        Application.DoEvents();
    }

    /// <summary>
    /// Safely disposes of a form after testing
    /// </summary>
    public static void SafeDisposeForm(Form form)
    {
        if (form != null && !form.IsDisposed)
        {
            try
            {
                if (form.InvokeRequired)
                {
                    form.Invoke(new Action(() => form.Dispose()));
                }
                else
                {
                    form.Dispose();
                }
            }
            catch
            {
                // Ignore disposal errors in tests
            }
        }
    }

    /// <summary>
    /// Finds a control by name within a container
    /// </summary>
    public static T? FindControl<T>(Control container, string name) where T : Control
    {
        if (container == null) return null;

        var controls = container.Controls.Find(name, true);
        return controls.FirstOrDefault() as T;
    }

    /// <summary>
    /// Asserts that a control is visible and enabled
    /// </summary>
    public static void AssertControlState(Control control, bool shouldBeVisible = true, bool shouldBeEnabled = true)
    {
        if (control == null) throw new ArgumentNullException(nameof(control));

        control.Visible.Should().Be(shouldBeVisible, $"Control {control.Name} visibility should be {shouldBeVisible}");
        control.Enabled.Should().Be(shouldBeEnabled, $"Control {control.Name} enabled state should be {shouldBeEnabled}");
    }

    /// <summary>
    /// Validates that required fields are filled
    /// </summary>
    public static void FillRequiredFields(MainForm form)
    {
        InitializeFormForTesting(form);

        // Find and fill required fields
        var nameTextBox = FindControl<TextBox>(form, "nameTextBox");
        if (nameTextBox != null)
        {
            SetTextBoxValue(nameTextBox, "Test User");
        }

        var ageControl = FindControl<NumericUpDown>(form, "ageNumericUpDown");
        if (ageControl != null)
        {
            SetNumericUpDownValue(ageControl, 25);
        }

        var genderCombo = FindControl<ComboBox>(form, "genderComboBox");
        if (genderCombo != null)
        {
            SetComboBoxSelection(genderCombo, 0);
        }

        var goalsListBox = FindControl<CheckedListBox>(form, "goalsCheckedListBox");
        if (goalsListBox != null)
        {
            SetCheckedListBoxItems(goalsListBox, 0); // Select first goal
        }
    }

    /// <summary>
    /// Creates sample routine content for testing
    /// </summary>
    public static string CreateSampleRoutineContent()
    {
        return @"👤 INFORMACIÓN DEL CLIENTE:
Nombre: Test User
Edad: 25 años
Género: Masculino

🎯 OBJETIVOS:
• Pérdida de peso

📋 RUTINA DE ENTRENAMIENTO:

DÍA 1 - TREN SUPERIOR
1. Press de banca - 4 series x 8-10 reps
2. Remo con barra - 4 series x 8-10 reps
3. Press militar - 3 series x 10-12 reps

DÍA 2 - TREN INFERIOR
1. Sentadillas - 4 series x 10-12 reps
2. Peso muerto - 4 series x 6-8 reps
3. Zancadas - 3 series x 12 reps por pierna

✅ ¡Rutina lista para exportar a Word!";
    }
}