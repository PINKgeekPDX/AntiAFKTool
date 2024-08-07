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
    public partial class SplashWindow : Window
    {
        private const double FadeInDuration = 3.0;
        private const double FadeOutDuration = 1.0;
        private const double SplashDuration = 6.0;

        private readonly DispatcherTimer _splashTimer;

        public SplashWindow()
        {
            InitializeComponent();
            CenterWindowOnScreen();
            _splashTimer = CreateAndStartSplashTimer();
            FadeIn();
        }

        private void CenterWindowOnScreen()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private DispatcherTimer CreateAndStartSplashTimer()
        {
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(SplashDuration)
            };
            timer.Tick += SplashTimer_Tick!;
            timer.Start();
            return timer;
        }

        private void SplashTimer_Tick(object? sender, EventArgs e)
        {
            _splashTimer.Stop();
            FadeOut();
        }

        private void ShowMainWindow()
        {
            try
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening main window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void FadeIn()
        {
            BeginAnimation(OpacityProperty, CreateFadeAnimation(0, 1, FadeInDuration));
        }

        private void FadeOut()
        {
            var fadeOutAnimation = CreateFadeAnimation(1, 0, FadeOutDuration);
            fadeOutAnimation.Completed += (s, e) => Dispatcher.Invoke(ShowMainWindow);
            BeginAnimation(OpacityProperty, fadeOutAnimation);
        }

        private static DoubleAnimation CreateFadeAnimation(double from, double to, double durationSeconds)
        {
            return new DoubleAnimation(from, to, new Duration(TimeSpan.FromSeconds(durationSeconds)))
            {
                EasingFunction = new QuadraticEase()
            };
        }

        protected override void OnClosed(EventArgs e)
        {
            _splashTimer.Stop();
            base.OnClosed(e);
        }
    }
}