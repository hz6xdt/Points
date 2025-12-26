using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Points;

public partial class ToolsViewModel : ObservableObject
{
    public ToolWindow? ToolWindow { get; set; }
    public MainWindow? MainWindow { get; set; }

    public bool isBackgroundColorSelected = false;


    [ObservableProperty]
    public partial ObservableCollection<ColorItem> Colors { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<string> ColorsListNames { get; set; } = [];

    public Dictionary<string, (List<ColorItem> colors, ColorItem backgroundColor)> ColorsList { get; set; } = [];



    [ObservableProperty]
    public partial ColorItem? BackgroundColor { get; set; }

    [ObservableProperty]
    public partial int PauseBetweenRuns { get; set; }

    [ObservableProperty]
    public partial int PointsPerCluster { get; set; }

    [ObservableProperty]
    public partial int ClustersPerColor { get; set; }


    [ObservableProperty]
    public partial ColorItem? SelectedColor { get; set; }

    [ObservableProperty]
    public partial int SelectedColorIndex { get; set; } = -1;


    [ObservableProperty]
    public partial string SelectedColorListName { get; set; }

    [ObservableProperty]
    public partial int SelectedColorListIndex { get; set; } = -1;



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
            isBackgroundColorSelected = false;

            if (SelectedColorListIndex != -1)
            {
                MainWindow?.ColorsList[SelectedColorListName].backgroundColor = ToolWindow.SelectedColor;
            }
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

            if (SelectedColorIndex != -1)
            {
                MainWindow?.ColorsList[SelectedColorListName].colors = Colors.Select(c => c.Color).ToList();
            }
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
    public void RemoveColorListName()
    {
        if (SelectedColorListIndex != -1)
        {
            MainWindow?.ColorsList.Remove(SelectedColorListName);
            ColorsListNames.Remove(SelectedColorListName);
        }
    }

    [RelayCommand]
    public void SelectColorList()
    {
        if (ToolWindow == null || SelectedColorListName == null || MainWindow == null)
        {
            return;
        }

        SelectedColorIndex = -1;
        SelectedColor = null;

        if (MainWindow.ColorsList.TryGetValue(SelectedColorListName, out ColorsListEntry? colorItems))
        {
            Colors = new ObservableCollection<ColorItem>(colorItems.colors.Select(c => new ColorItem { Color = c }));
            BackgroundColor = new ColorItem { Color = colorItems.backgroundColor };
            PauseBetweenRuns = colorItems.PauseBetweenRuns;
            PointsPerCluster = colorItems.PointsPerCluster;
            ClustersPerColor = colorItems.ClustersPerColor;

            MainWindow.Colors = Colors.Select(c => c.Color).ToList();
            MainWindow.BackgroundColor = BackgroundColor.Color;
            MainWindow.PauseBetweenRuns = PauseBetweenRuns;
            MainWindow.PointsPerCluster = PointsPerCluster;
            MainWindow.ClustersPerColor = ClustersPerColor;

            MainWindow.CanvasControlInstance?.Invalidate();
        }

        ToolWindow.FocusSaveColorListButton();
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
    public void SetPauseBetweenRuns()
    {
        if (MainWindow == null)
        {
            return;
        }
        MainWindow.PauseBetweenRuns = PauseBetweenRuns;
        ToolWindow?.FocusPointsPerClusterNumberBox();
    }

    [RelayCommand]
    public void SetPointsPerCluster()
    {
        if (MainWindow == null)
        {
            return;
        }
        MainWindow.PointsPerCluster = PointsPerCluster;
        ToolWindow?.FocusClustersPerColorNumberBox();

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
        ToolWindow?.FocusSaveColorListButton();

        MainWindow?.CanvasControlInstance?.Invalidate();
    }


    [RelayCommand]
    public void SaveColorList()
    {
        if (ToolWindow == null)
        {
            return;
        }

        ContentDialog saveDialog = new()
        {
            XamlRoot = ToolWindow.Content.XamlRoot,
            Title = "Save Colors",
            Content = new StackPanel
            {
                Children =
                                    {
                                        new TextBlock { Text = "Save these colors as:", Margin = new Thickness(0,0,0,10) },
                                        new TextBox { Text = SelectedColorListName ?? string.Empty, Width=250 }
                                    }
            },
            CloseButtonText = "Cancel",
            PrimaryButtonText = "Save",
            DefaultButton = ContentDialogButton.Primary
        };
        saveDialog.PrimaryButtonClick += (s, e) =>
        {
            if (saveDialog.Content is StackPanel stackPanel)
            {
                foreach (UIElement child in stackPanel.Children)
                {
                    if (child is TextBox inputTextBox)
                    {
                        ColorsList[inputTextBox.Text] = (Colors.ToList(), BackgroundColor!);
                        MainWindow?.ColorsList[inputTextBox.Text] = new ColorsListEntry
                        {
                            colors = Colors.Select(c => c.Color).ToList(),
                            backgroundColor = BackgroundColor!.Color,
                            PauseBetweenRuns = PauseBetweenRuns,
                            PointsPerCluster = PointsPerCluster,
                            ClustersPerColor = ClustersPerColor
                        };

                        if (!ColorsListNames.Contains(inputTextBox.Text))
                        {
                            ColorsListNames.Add(inputTextBox.Text);
                        }
                    }
                }
            }
        };
        _ = saveDialog.ShowAsync();
    }


    [RelayCommand]
    public void AddColorListName()
    {
        ContentDialog addDialog = new()
        {
            XamlRoot = ToolWindow!.Content.XamlRoot,
            Title = "Add Color List",
            Content = new StackPanel
            {
                Children =
                                    {
                                        new TextBlock { Text = "Enter a name for the new color list:", Margin = new Thickness(0,0,0,10) },
                                        new TextBox { Width=250 }
                                    }
            },
            CloseButtonText = "Cancel",
            PrimaryButtonText = "Add",
            DefaultButton = ContentDialogButton.Primary
        };
        addDialog.PrimaryButtonClick += (s, e) =>
        {
            if (addDialog.Content is StackPanel stackPanel)
            {
                foreach (UIElement child in stackPanel.Children)
                {
                    if (child is TextBox inputTextBox)
                    {
                        if (!ColorsListNames.Contains(inputTextBox.Text))
                        {
                            ColorsListNames.Add(inputTextBox.Text);
                            ColorsList[inputTextBox.Text] = (Colors.ToList(), BackgroundColor!);
                            MainWindow?.ColorsList[inputTextBox.Text] = new ColorsListEntry
                            {
                                colors = Colors.Select(c => c.Color).ToList(),
                                backgroundColor = BackgroundColor!.Color,
                                PauseBetweenRuns = PauseBetweenRuns,
                                PointsPerCluster = PointsPerCluster,
                                ClustersPerColor = ClustersPerColor
                            };
                            SelectedColorListIndex = ColorsListNames.Count - 1;
                            SelectedColorListName = inputTextBox.Text;
                        }
                    }
                }
            }
        };

        _ = addDialog.ShowAsync();
    }


    public void ColorSelectionChanged(object sender, Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs e)
    {
        if (ToolWindow == null || SelectedColor == null)
        {
            return;
        }

        ToolWindow.SelectedColor = SelectedColor.Color;
    }


    public void ColorListSelectionChanged(object sender, Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs e)
    {
        if (ToolWindow == null || SelectedColorListName == null)
        {
            return;
        }

        SelectColorList();
    }




    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Event Signature")]
    public void PauseBetweenRunsValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs e)
    {
        ToolWindow?.FocusPauseBetweenRunsResetButton();
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Event Signature")]
    public void PointsPerClusterValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs e)
    {
        ToolWindow?.FocusPointsPerClusterResetButton();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Event Signature")]
    public void ClustersPerColorValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs e)
    {
        ToolWindow?.FocusClustersPerColorResetButton();
    }
}
