using System;
using System.Collections.Generic;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Foundation;
using Windows.UI;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Points
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly AppWindow? appWindow;
        private readonly Window toolWindow;

        private int width;
        private int height;

        public Color BackgroundColor { get; set; } = Color.FromArgb(40, 80, 80, 40);


        public List<Color> Colors = [Color.FromArgb(255, 0x1d, 0x8a, 0x14),
                                     Color.FromArgb(255, 93, 55, 67),
                                     Color.FromArgb(255, 167, 167, 141),
                                     Color.FromArgb(255, 103, 118, 141),
                                     Color.FromArgb(255, 188, 189, 151),
                                     Color.FromArgb(255, 135, 66, 57)
        ];

        [field: global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler", " 3.0.0.2511")]
        public CanvasControl? CanvasControlInstance { get; private set; }



        public MainWindow()
        {
            InitializeComponent();

            appWindow = GetAppWindowForWindow(this);
            appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);

            toolWindow = new ToolWindow(this);

            CanvasControlInstance = this.Canvas;

            AppWindow toolAppWindow = GetAppWindowForWindow(toolWindow);
            if (toolAppWindow.Presenter is OverlappedPresenter overlappedPresenter)
            {
                overlappedPresenter.IsAlwaysOnTop = true;
            }
            toolWindow.Activate();
        }

        private void Canvas_Draw(CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            CanvasDrawingSession ds = args.DrawingSession;
            ds.Clear(BackgroundColor);

            width = (int)sender.ActualWidth;
            height = (int)sender.ActualHeight;

            //Debug.WriteLine($"Screen Dimensions: {width}, {height}");


            Random rand = new();

            Color[] colors = new Color[width * height];


            for (int k = 0; k < Colors.Count; k++)
            {
                for (int i = 0; i < 100; i++)
                {
                    int x = rand.Next(4, width - 4);
                    int y = rand.Next(4, height - 4);

                    //Debug.WriteLine($"Start Position: {x},{y}");

                    for (int j = 0; j < 500; j++)
                    {
                        int xx = rand.Next(-9, 10);
                        int yy = rand.Next(-9, 10);
                        x = Math.Clamp(x + xx, 0, width - 1);
                        y = Math.Clamp(y + yy, 0, height - 1);

                        //Debug.WriteLine($"Point Position: {x},{y}");

                        int rr = rand.Next(-10, 10);
                        int gg = rand.Next(-10, 10);
                        int bb = rand.Next(-10, 10);
                        int aa = rand.Next(-10, 10);

                        byte red = (byte)Math.Clamp(Colors[k].R + rr, 0, 255);
                        byte blue = (byte)Math.Clamp(Colors[k].B + bb, 0, 255);
                        byte green = (byte)Math.Clamp(Colors[k].G + gg, 0, 255);
                        byte alpha = (byte)Math.Clamp(Colors[k].A + aa, 0, 255);

                        colors[x + y * width] = Color.FromArgb(alpha, red, green, blue);
                    }
                }
            }



            CanvasBitmap bitmap = CanvasBitmap.CreateFromColors(ds, colors, width, height);


            ds.DrawImage(bitmap, new Rect(0, 0, width, height));
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            if (this.CanvasControlInstance != null)
            {
                this.CanvasControlInstance.RemoveFromVisualTree();
                this.CanvasControlInstance = null;
            }
        }

        private static AppWindow GetAppWindowForWindow(Window window)
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(window);
            WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(myWndId);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            toolWindow.Close();
            this.Close();
        }
    }
}
