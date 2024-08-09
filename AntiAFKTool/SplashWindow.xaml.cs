using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace AntiAFKTool
{
    public partial class SplashWindow : Window
    {
        private const double FadeInDuration = 3.0;
        private const double FadeOutDuration = 2.0;
        private const double ProgressBarDuration = 10.0;

        private double _progress;
        private DispatcherTimer? _progressTimer;

        public SplashWindow()
        {
            InitializeComponent();
            CenterWindowOnScreen();
            InitializeWebView();
            FadeIn();
        }

        private void CenterWindowOnScreen()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void InitializeWebView()
        {
            try
            {
                progressbar.CoreWebView2InitializationCompleted += Progressbar_CoreWebView2InitializationCompleted;
                progressbar.NavigationCompleted += Progressbar_NavigationCompleted;
                progressbar.WebMessageReceived += WebView2_WebMessageReceived;
            }
            catch (Exception ex)
            {
                LogError("Error initializing WebView2: " + ex.Message);
                MessageBox.Show("Error initializing WebView2. Check the log for details.");
            }
        }

        private void Progressbar_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            try
            {
                if (progressbar.CoreWebView2 != null)
                {
                    string htmlContent = GetEmbeddedHtmlContent();
                    progressbar.NavigateToString(htmlContent);
                }
            }
            catch (Exception ex)
            {
                LogError("Error during WebView2 initialization: " + ex.Message);
                MessageBox.Show("Error during WebView2 initialization. Check the log for details.");
            }
        }

        private void Progressbar_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            StartProgressBar();
        }

        private void WebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var message = e.WebMessageAsJson;
                // Handle message from JavaScript if needed
            }
            catch (Exception ex)
            {
                LogError("Error processing message from web content: " + ex.Message);
                MessageBox.Show("Error processing message from web content. Check the log for details.");
            }
        }

        private void StartProgressBar()
        {
            try
            {
                _progress = 0;

                _progressTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(ProgressBarDuration / 100)
                };
                _progressTimer.Tick += ProgressTimer_Tick;
                _progressTimer.Start();
            }
            catch (Exception ex)
            {
                LogError("Error starting progress bar: " + ex.Message);
                MessageBox.Show("Error starting progress bar. Check the log for details.");
            }
        }

        private void ProgressTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                _progress += 0.01;
                SetProgress(_progress);

                if (_progress >= 1)
                {
                    _progressTimer?.Stop();
                    FadeOut();
                }
            }
            catch (Exception ex)
            {
                LogError("Error updating progress bar: " + ex.Message);
                MessageBox.Show("Error updating progress bar. Check the log for details.");
            }
        }

        private void SetProgress(double progress)
        {
            try
            {
                progressbar.CoreWebView2?.PostWebMessageAsString(
                    "{\"type\": \"setProgress\", \"value\": " + progress.ToString("F2") + "}");
            }
            catch (Exception ex)
            {
                LogError("Error setting progress: " + ex.Message);
                MessageBox.Show("Error setting progress. Check the log for details.");
            }
        }

        private void ToggleParticles(bool isActive)
        {
            try
            {
                progressbar.CoreWebView2?.PostWebMessageAsString(
                    "{\"type\": \"toggleParticles\", \"isActive\": " + isActive.ToString().ToLower() + "}");
            }
            catch (Exception ex)
            {
                LogError("Error toggling particles: " + ex.Message);
                MessageBox.Show("Error toggling particles. Check the log for details.");
            }
        }

        private void FadeIn()
        {
            try
            {
                var fadeInAnimation = CreateFadeAnimation(0, 1, FadeInDuration);
                fadeInAnimation.Completed += (s, e) => StartProgressBar();
                BeginAnimation(OpacityProperty, fadeInAnimation);
            }
            catch (Exception ex)
            {
                LogError("Error during fade-in: " + ex.Message);
                MessageBox.Show("Error during fade-in. Check the log for details.");
            }
        }

        private void FadeOut()
        {
            try
            {
                ToggleParticles(false);
                var fadeOutAnimation = CreateFadeAnimation(1, 0, FadeOutDuration);
                fadeOutAnimation.Completed += (s, e) => Dispatcher.Invoke(ShowMainWindow);
                BeginAnimation(OpacityProperty, fadeOutAnimation);
            }
            catch (Exception ex)
            {
                LogError("Error during fade-out: " + ex.Message);
                MessageBox.Show("Error during fade-out. Check the log for details.");
            }
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
                LogError("Error opening main window: " + ex.Message);
                MessageBox.Show("Error opening main window. Check the log for details.");
                Close();
            }
        }

        private static DoubleAnimation CreateFadeAnimation(double from, double to, double durationSeconds)
        {
            return new DoubleAnimation(from, to, new Duration(TimeSpan.FromSeconds(durationSeconds)))
            {
                EasingFunction = new QuadraticEase()
            };
        }

        private static string GetEmbeddedHtmlContent()
        {
            var htmlBuilder = new System.Text.StringBuilder();

            // CSS
            string css = @"
            body { 
                margin: 0; 
                padding: 0; 
                overflow: hidden; 
                background: radial-gradient(circle, #1b2735 0%, #090a0f 100%); 
                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
            }
            canvas { 
                position: fixed; 
                top: 0; 
                left: 0; 
            }";

            // JS
            string js = @"
            const canvas = document.getElementsByTagName('canvas')[0];
            const ctx = canvas.getContext('2d');
            let particles = [];
            let w, h;
            let progress = 0;
            let fadeOut = false;
            let particlesActive = true;
            function resizeCanvas() { w = canvas.width = window.innerWidth; h = canvas.height = window.innerHeight; }
            window.addEventListener('resize', resizeCanvas);
            resizeCanvas();
            function ProgressBar() { 
                this.width = 0; 
                this.height = 40; 
                this.opacity = 1; 
                this.x = w * 0.1; 
                this.y = h * 0.5; 
                this.maxWidth = w * 0.8; 
                this.draw = function () { 
                    ctx.save(); 
                    ctx.globalAlpha = this.opacity; 
                    ctx.shadowColor = 'rgba(255, 255, 255, 0.7)'; 
                    ctx.shadowBlur = 30; 
                    const gradient = ctx.createLinearGradient(this.x, this.y, this.x + this.maxWidth, this.y); 
                    gradient.addColorStop(0, 'rgba(173, 216, 230, 0.8)'); 
                    gradient.addColorStop(0.5, 'rgba(0, 191, 255, 0.8)'); 
                    gradient.addColorStop(1, 'rgba(25, 25, 112, 0.8)'); 
                    ctx.fillStyle = gradient; 
                    ctx.beginPath(); 
                    ctx.moveTo(this.x, this.y); 
                    ctx.lineTo(this.x + this.width, this.y); 
                    ctx.quadraticCurveTo(this.x + this.width + 25, this.y + this.height / 2, this.x + this.width, this.y + this.height); 
                    ctx.lineTo(this.x, this.y + this.height); 
                    ctx.closePath(); 
                    ctx.fill(); 
                    ctx.strokeStyle = 'rgba(255, 255, 255, 0.7)'; 
                    ctx.lineWidth = 3; 
                    ctx.beginPath(); 
                    ctx.moveTo(this.x,";

            // HTML Content
            htmlBuilder.AppendLine("<!DOCTYPE html>");
            htmlBuilder.AppendLine("<html lang='en'>");
            htmlBuilder.AppendLine("<head>");
            htmlBuilder.AppendLine("<meta charset='UTF-8'>");
            htmlBuilder.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            htmlBuilder.AppendLine("<title>Stellar Cosmic Energy Flow</title>");
            htmlBuilder.AppendLine("<style>" + css + "</style>");
            htmlBuilder.AppendLine("</head>");
            htmlBuilder.AppendLine("<body>");
            htmlBuilder.AppendLine("<canvas></canvas>");
            htmlBuilder.AppendLine("<script>" + js + "</script>");
            htmlBuilder.AppendLine("</body>");
            htmlBuilder.AppendLine("</html>");

            return htmlBuilder.ToString();
        }

        private void LogError(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                logMethod();
                return;
            }

            try
            {
                string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AntiAFKTool", "error.log");
                DirectoryInfo directoryInfo = Directory.CreateDirectory(Path.GetDirectoryName(logPath));
                File.AppendAllText(logPath, DateTime.Now + ": " + message + Environment.NewLine);
            }
            catch
            {
                // In case logging fails, do not throw additional exceptions.
            }

            static void logMethod()
            {
                throw new ArgumentException($"{nameof(message)} is null or empty.", nameof(message));
            }
        }
    }
}
