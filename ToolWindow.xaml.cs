using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Points
{
    public sealed partial class ToolWindow : Window
    {
        public ToolsViewModel? ViewModel;

        private readonly MainWindow mainWindow;


        public Color SelectedColor
        {
            get
            {
                return ColorPicker.Color;
            }
            set
            {
                ColorPicker.Color = value;
            }
        }


        public ToolWindow(MainWindow mainWindow)
        {
            ViewModel = App.HostContainer?.Services.GetService<ToolsViewModel>();

            InitializeComponent();

            this.mainWindow = mainWindow;

            ViewModel?.Colors = new ObservableCollection<ColorItem>(mainWindow.Colors.Select(c => new ColorItem { DisplayColor = new SolidColorBrush(c) }));
            ViewModel?.BackgroundColor = new ColorItem { DisplayColor = new SolidColorBrush(mainWindow.BackgroundColor) };
            ViewModel?.PauseBetweenRuns = mainWindow.PauseBetweenRuns;
            ViewModel?.PointsPerCluster = mainWindow.PointsPerCluster;
            ViewModel?.ClustersPerColor = mainWindow.ClustersPerColor;

            ViewModel?.ToolWindow = this;
            ViewModel?.MainWindow = mainWindow;
        }

        public void FocusPauseBetweenRunsResetButton()
        {
            PauseBetweenRunsResetButton.Focus(FocusState.Programmatic);
        }

        public void FocusPointsPerClusterResetButton()
        {
            PointsPerClusterResetButton.Focus(FocusState.Programmatic);
        }

        public void FocusClustersPerColorResetButton()
        {
            ClustersPerColorResetButton.Focus(FocusState.Programmatic);
        }
    }
}
