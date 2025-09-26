using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace GymRoutineGenerator.UI.Views;

public sealed partial class HistoryView : UserControl
{
    public HistoryView()
    {
        this.InitializeComponent();
    }

    public void LoadHistory()
    {
        try
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var app = System.IO.Path.Combine(folder, "GymRoutineGenerator");
            var file = System.IO.Path.Combine(app, "history.log");
            List.Items?.Clear();
            if (System.IO.File.Exists(file))
            {
                foreach (var line in System.IO.File.ReadAllLines(file))
                {
                    List.Items?.Add(line);
                }
            }
        }
        catch { }
    }
}

