using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Points
{
    public partial class ColorItem : ObservableObject
    {
        [ObservableProperty]
        public partial SolidColorBrush? DisplayColor { get; set; }

        public Color Color
        {
            get
            {
                return DisplayColor != null ? DisplayColor.Color : Colors.Transparent;
            }
            set
            {
                DisplayColor = new SolidColorBrush(value);
            }
        }
    }
}
