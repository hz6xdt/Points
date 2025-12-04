using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Foundation;
using Windows.UI;
using WinRT.Interop;

namespace Points
{
    public sealed partial class MainWindow : Window
    {
        private readonly AppWindow? appWindow;
        private readonly Window toolWindow;

        private const int toolWindowWidth = 1024;
        private const int toolWindowHeight = 800;


        // Cancellation token to stop the background loop when window closes
        private CancellationTokenSource? renderLoopCts;
        // Ensure loop starts only once after window is visible/activated
        private bool renderLoopStarted;

        private Color[] CanvasColors = [];

        private bool clearingCanvas = false;
        private bool clearCanvas = false;


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


        public int Frames { get; set; } = 7;
        public int PauseBetweenFrames { get; set; } = 9;

        public int PointsPerCluster { get; set; } = 100;

        public int ClustersPerColor { get; set; } = 100;



        public MainWindow()
        {
            InitializeComponent();
            this.Activated += MainWindow_Activated;

            appWindow = GetAppWindowForWindow(this);
            appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);

            toolWindow = new ToolWindow(this);

            CanvasControlInstance = this.Canvas;


            // Move the tool window to the secondary display (index 1) and center it there before activation
            MoveWindowToMonitorCentered(toolWindow, 1, desiredWidthDips: toolWindowWidth, desiredHeightDips: toolWindowHeight);


            AppWindow toolAppWindow = GetAppWindowForWindow(toolWindow);
            if (toolAppWindow.Presenter is OverlappedPresenter overlappedPresenter)
            {
                overlappedPresenter.IsAlwaysOnTop = true;
            }

            toolWindow.Activate();

            // Disable resizing for the tool window so it can't be resized or maximized
            DisableWindowResize(toolWindow);
        }




        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            // Start the background loop only once, after the window has been activated (visible)
            if (renderLoopStarted)
            {
                return;
            }

            renderLoopStarted = true;
            renderLoopCts = new CancellationTokenSource();

            // Capture the UI Dispatcher for marshaling Invalidate calls to the UI thread
            DispatcherQueue? uiDispatcher = DispatcherQueue.GetForCurrentThread();



            // Run loop on a background task so UI thread is not blocked
            _ = Task.Run(async () =>
            {
                CancellationToken token = renderLoopCts.Token;

                try
                {
                    while (!token.IsCancellationRequested)
                    {

                        for (int f = 0; f < Frames; f++)
                        {
                            // Marshal the Invalidate call to the UI thread
                            uiDispatcher?.TryEnqueue(() => CanvasControlInstance?.Invalidate());
                            // Wait without blocking the UI thread
                            await Task.Delay(TimeSpan.FromMilliseconds(300), token);

                            if (clearingCanvas && f == Frames - 1)
                            {
                                clearCanvas = true;
                            }
                        }

                        int delay = PauseBetweenFrames;

                        if (clearingCanvas)
                        {
                            delay = 1;
                            clearingCanvas = false;
                        }
                        else
                        {
                            clearingCanvas = true;
                        }

                        // Wait without blocking the UI thread
                        await Task.Delay(TimeSpan.FromSeconds(delay), token);
                    }
                }
                catch (OperationCanceledException)
                {
                    // expected on cancellation
                }
            });
        }






        private void Canvas_Draw(CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            CanvasDrawingSession ds = args.DrawingSession;

            int width = (int)sender.ActualWidth;
            int height = (int)sender.ActualHeight;

            //Debug.WriteLine($"Screen Dimensions: {width}, {height}");

            if (CanvasColors.Length != width * height)
            {
                CanvasColors = new Color[width * height];
            }



            Random rand = new();

            if (clearCanvas)
            {
                for (int i = 0; i < CanvasColors.Length; i++)
                {
                    CanvasColors[i] = BackgroundColor;
                }
                clearCanvas = false;
            }
            else if (clearingCanvas)
            {
                for (int i = 0; i < CanvasColors.Length / Frames * 3; i++)
                {
                    int x = rand.Next(4, width - 4);
                    int y = rand.Next(4, height - 4);

                    CanvasColors[x + y * width] = BackgroundColor;
                }
            }
            else
            {
                for (int k = 0; k < Colors.Count; k++)
                {
                    for (int i = 0; i < ClustersPerColor; i++)
                    {
                        int x = rand.Next(4, width - 4);
                        int y = rand.Next(4, height - 4);

                        //Debug.WriteLine($"Start Position: {x},{y}");

                        for (int j = 0; j < PointsPerCluster; j++)
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

                            CanvasColors[x + y * width] = Color.FromArgb(alpha, red, green, blue);
                        }
                    }
                }
            }




            CanvasBitmap bitmap = CanvasBitmap.CreateFromColors(ds, CanvasColors, width, height);


            ds.DrawImage(bitmap, new Rect(0, 0, width, height));
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            // Cancel the background loop and clean up the CanvasControl
            if (renderLoopCts != null && !renderLoopCts.IsCancellationRequested)
            {
                renderLoopCts.Cancel();
                renderLoopCts.Dispose();
                renderLoopCts = null;
            }

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









        // --- Monitor / window placement helpers ---

        /// <summary>
        /// Move and optionally resize the <paramref name="window"/> so it is centered on the specified monitor.
        /// If desiredWidthDips/desiredHeightDips are provided (>0) they are treated as device-independent pixels (DIPs)
        /// and converted to physical pixels using the monitor DPI.
        /// </summary>
        private static void MoveWindowToMonitorCentered(Window window, int monitorIndex, int desiredWidthDips = 0, int desiredHeightDips = 0)
        {
            if (window is null)
            {
                return;
            }

            IntPtr hwnd = WindowNative.GetWindowHandle(window);

            List<MonitorInfo> monitors = GetMonitorInfos();
            if (monitorIndex < 0 || monitorIndex >= monitors.Count)
            {
                return; // invalid monitor index
            }

            MonitorInfo monitor = monitors[monitorIndex];
            RECT target = monitor.Rect;
            int targetWidth = target.Right - target.Left;
            int targetHeight = target.Bottom - target.Top;

            // Determine final window size in physical pixels.
            int winWidth;
            int winHeight;

            if (desiredWidthDips > 0 && desiredHeightDips > 0)
            {
                // Convert requested DIPs to physical pixels using monitor DPI (dpiX/dpiY)
                uint dpiX = monitor.DpiX != 0 ? monitor.DpiX : 96;
                uint dpiY = monitor.DpiY != 0 ? monitor.DpiY : 96;
                winWidth = (int)Math.Max(1, Math.Round(desiredWidthDips * dpiX / 96.0));
                winHeight = (int)Math.Max(1, Math.Round(desiredHeightDips * dpiY / 96.0));
            }
            else if (GetWindowRect(hwnd, out RECT wr))
            {
                winWidth = wr.Right - wr.Left;
                winHeight = wr.Bottom - wr.Top;
            }
            else
            {
                // fallback default in pixels
                winWidth = Math.Min(800, targetWidth);
                winHeight = Math.Min(600, targetHeight);
            }

            // Clamp to monitor size
            winWidth = Math.Min(winWidth, targetWidth);
            winHeight = Math.Min(winHeight, targetHeight);

            // Compute centered origin inside target monitor
            int newX = target.Left + Math.Max(0, (targetWidth - winWidth) / 2);
            int newY = target.Top + Math.Max(0, (targetHeight - winHeight) / 2);

            // Move and resize window to computed origin/size.
            // Use SetWindowPos without SWP_NOSIZE so size is applied.
            SetWindowPos(hwnd, IntPtr.Zero, newX, newY, winWidth, winHeight, SWP_NOZORDER | SWP_SHOWWINDOW);
        }

        /// <summary>
        /// Removes resizing styles so the specified window cannot be resized or maximized.
        /// Call this after the native handle is available (after Activate()).
        /// </summary>
        private static void DisableWindowResize(Window window)
        {
            if (window is null)
            {
                return;
            }

            IntPtr hwnd = WindowNative.GetWindowHandle(window);
            if (hwnd == IntPtr.Zero)
            {
                return;
            }

            const int GWL_STYLE = -16;
            const uint WS_THICKFRAME = 0x00040000;
            const uint WS_MAXIMIZEBOX = 0x00010000;
            const uint SWP_NOMOVE = 0x0002;
            const uint SWP_NOSIZE = 0x0001;
            const uint SWP_NOZORDER = 0x0004;
            const uint SWP_FRAMECHANGED = 0x0020;

            if (IntPtr.Size == 8)
            {
                // x64
                IntPtr stylePtr = GetWindowLongPtr(hwnd, GWL_STYLE);
                long style = stylePtr.ToInt64();
                style &= ~(WS_THICKFRAME | (long)WS_MAXIMIZEBOX);
                _ = SetWindowLongPtr(hwnd, GWL_STYLE, new IntPtr(style));
            }
            else
            {
                // x86
                int style = GetWindowLong(hwnd, GWL_STYLE);
                style &= ~((int)WS_THICKFRAME | (int)WS_MAXIMIZEBOX);
                _ = SetWindowLong(hwnd, GWL_STYLE, style);
            }

            // Force a frame update so the change takes effect immediately
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
        }

        // P/Invoke helpers for style manipulation
        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private struct MonitorInfo
        {
            public IntPtr HMonitor;
            public RECT Rect;
            public uint DpiX;
            public uint DpiY;
        }

        private static List<MonitorInfo> GetMonitorInfos()
        {
            List<MonitorInfo> list = [];
            bool callback(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
            {
                MonitorInfo info = new()
                {
                    HMonitor = hMonitor,
                    Rect = lprcMonitor,
                    DpiX = 0,
                    DpiY = 0
                };

                // Try to get DPI for this monitor (Shcore.dll). If it fails, DpiX/DpiY remain 0 -> fallback to 96 later.
                try
                {
                    if (GetDpiForMonitor(hMonitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out uint dpiX, out uint dpiY) == 0)
                    {
                        info.DpiX = dpiX;
                        info.DpiY = dpiY;
                    }
                }
                catch
                {
                    // ignore - we'll fallback to 96 DPI
                }

                list.Add(info);
                return true;
            }

            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);
            return list;
        }

        private enum MONITOR_DPI_TYPE
        {
            MDT_EFFECTIVE_DPI = 0,
            MDT_ANGULAR_DPI = 1,
            MDT_RAW_DPI = 2,
            MDT_DEFAULT = MDT_EFFECTIVE_DPI
        }

        private delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        [DllImport("Shcore.dll")]
        private static extern int GetDpiForMonitor(IntPtr hmonitor, MONITOR_DPI_TYPE dpiType, out uint dpiX, out uint dpiY);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_SHOWWINDOW = 0x0040;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}
