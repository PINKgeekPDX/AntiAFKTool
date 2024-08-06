using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Media.Animation;

namespace AntiAFKTool
{
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {
        private DispatcherTimer? splashTimer;

        public SplashWindow()
        {
            InitializeComponent();
            CenterWindowOnScreen();
            FadeIn();
            StartSplashTimer();
        }

        private void CenterWindowOnScreen()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void StartSplashTimer()
        {
            splashTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(6)
            };
            splashTimer.Tick += SplashTimer_Tick;
            splashTimer.Start();
        }

        private void SplashTimer_Tick(object? sender, EventArgs? e)
        {
            splashTimer?.Stop();
            FadeOut();
        }

        private void ShowMainWindow()
        {
            MainWindow mainWindow = new();
            mainWindow.Show();
            this.Hide();
        }

        private void FadeIn()
        {
            DoubleAnimation fadeIn = new(0, 1, new Duration(TimeSpan.FromSeconds(3)));
            this.BeginAnimation(Window.OpacityProperty, fadeIn);
        }

        private void FadeOut()
        {
            DoubleAnimation fadeOut = new(1, 0, new Duration(TimeSpan.FromSeconds(1)));
            fadeOut.Completed += (s, e) => ShowMainWindow();
            this.BeginAnimation(Window.OpacityProperty, fadeOut);
        }
    }
}