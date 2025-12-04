using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Points
{
    public partial class ToolsViewModel : ObservableObject
    {
        public ToolWindow? ToolWindow { get; set; }
        public MainWindow? MainWindow { get; set; }

        public bool isBackgroundColorSelected = false;


        [ObservableProperty]
        public partial ObservableCollection<ColorItem> Colors { get; set; } = [];

        [ObservableProperty]
        public partial ColorItem? BackgroundColor { get; set; }

        [ObservableProperty]
        public partial int PauseBetweenFrames { get; set; }

        [ObservableProperty]
        public partial int PointsPerCluster { get; set; }

        [ObservableProperty]
        public partial int ClustersPerColor { get; set; }


        [ObservableProperty]
        public partial ColorItem? SelectedColor { get; set; }

        [ObservableProperty]
        public partial int SelectedColorIndex { get; set; } = -1;


        [RelayCommand]
        public void AddColor()
        {
            if (ToolWindow == null)
            {
                return;
            }

            if (isBackgroundColorSelected)
            {
                BackgroundColor?.Color = ToolWindow.SelectedColor;
                MainWindow?.BackgroundColor = ToolWindow.SelectedColor;
            }
            else
            {
                if (SelectedColor != null)
                {
                    Colors[SelectedColorIndex].Color = ToolWindow.SelectedColor;
                }
                else
                {
                    Colors.Add(new ColorItem { Color = ToolWindow.SelectedColor });
                }
                Colors = new ObservableCollection<ColorItem>(Colors);
                MainWindow?.Colors = Colors.Select(c => c.Color).ToList();
            }

            MainWindow?.CanvasControlInstance?.Invalidate();
        }

        [RelayCommand]
        public void RemoveColor()
        {
            if (SelectedColor != null)
            {
                Colors.Remove(SelectedColor);
            }

            MainWindow?.Colors = Colors.Select(c => c.Color).ToList();
            MainWindow?.CanvasControlInstance?.Invalidate();
        }

        [RelayCommand]
        public void SelectBackgroundColor()
        {
            if (ToolWindow == null || BackgroundColor == null)
            {
                return;
            }

            SelectedColorIndex = -1;
            SelectedColor = null;

            ToolWindow.SelectedColor = BackgroundColor.Color;

            isBackgroundColorSelected = true;
        }

        [RelayCommand]
        public void SetPauseBetweenFrames()
        {
            if (MainWindow == null)
            {
                return;
            }
            MainWindow.PauseBetweenFrames = PauseBetweenFrames;
        }

        [RelayCommand]
        public void SetPointsPerCluster()
        {
            if (MainWindow == null)
            {
                return;
            }
            MainWindow.PointsPerCluster = PointsPerCluster;

            MainWindow?.CanvasControlInstance?.Invalidate();
        }

        [RelayCommand]
        public void SetClustersPerColor()
        {
            if (MainWindow == null)
            {
                return;
            }
            MainWindow.ClustersPerColor = ClustersPerColor;

            MainWindow?.CanvasControlInstance?.Invalidate();
        }

        public void ColorSelectionChanged(object sender, Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs e)
        {
            isBackgroundColorSelected = false;

            if (ToolWindow == null || SelectedColor == null)
            {
                return;
            }

            ToolWindow.SelectedColor = SelectedColor.Color;
        }
    }
}
