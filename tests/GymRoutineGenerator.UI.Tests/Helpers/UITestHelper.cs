using System.Drawing;
using System.Windows.Forms;
using GymRoutineGenerator.UI;

namespace GymRoutineGenerator.UI.Tests.Helpers;

public static class UITestHelper
{
    public static MainForm CreateTestMainForm()
    {
        return new MainForm();
    }

    public static void SimulateButtonClick(Button button)
    {
        if (button == null) throw new ArgumentNullException(nameof(button));
        button.PerformClick();
    }

    public static void SetTextBoxValue(TextBox textBox, string value)
    {
        if (textBox == null) throw new ArgumentNullException(nameof(textBox));
        textBox.Text = value;
    }

    public static void SetNumericUpDownValue(NumericUpDown control, decimal value)
    {
        if (control == null) throw new ArgumentNullException(nameof(control));
        control.Value = Math.Max(control.Minimum, Math.Min(control.Maximum, value));
    }

    public static void SetComboBoxSelection(ComboBox comboBox, int index)
    {
        if (comboBox == null) throw new ArgumentNullException(nameof(comboBox));

        if (index >= 0 && index < comboBox.Items.Count)
        {
            comboBox.SelectedIndex = index;
        }
    }

    public static void SetTrackBarValue(TrackBar trackBar, int value)
    {
        if (trackBar == null) throw new ArgumentNullException(nameof(trackBar));
        trackBar.Value = Math.Max(trackBar.Minimum, Math.Min(trackBar.Maximum, value));
    }

    public static void SetCheckedListBoxItems(CheckedListBox checkedListBox, params int[] indices)
    {
        if (checkedListBox == null) throw new ArgumentNullException(nameof(checkedListBox));

        foreach (var index in indices)
        {
            if (index >= 0 && index < checkedListBox.Items.Count)
            {
                checkedListBox.SetItemChecked(index, true);
            }
        }
    }

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

    public static void InitializeFormForTesting(Form form)
    {
        if (form == null) throw new ArgumentNullException(nameof(form));
        _ = form.Handle;
        Application.DoEvents();
    }

    public static void SafeDisposeForm(Form? form)
    {
        if (form == null || form.IsDisposed)
        {
            return;
        }

        try
        {
            if (form.InvokeRequired)
            {
                form.Invoke(new Action(form.Dispose));
            }
            else
            {
                form.Dispose();
            }
        }
        catch
        {
            // Ignore disposal errors during tests
        }
    }

    public static T? FindControl<T>(Control container, string name) where T : Control
    {
        if (container == null)
        {
            return null;
        }

        var controls = container.Controls.Find(name, true);
        return controls.FirstOrDefault() as T;
    }

    public static void AssertControlState(Control control, bool shouldBeVisible = true, bool shouldBeEnabled = true)
    {
        if (control == null) throw new ArgumentNullException(nameof(control));

        control.Visible.Should().Be(shouldBeVisible, $"Control {control.Name} visibility should be {shouldBeVisible}");
        control.Enabled.Should().Be(shouldBeEnabled, $"Control {control.Name} enabled state should be {shouldBeEnabled}");
    }

    public static void FillRequiredFields(MainForm form)
    {
        InitializeFormForTesting(form);

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
            SetCheckedListBoxItems(goalsListBox, 0);
        }
    }
}
