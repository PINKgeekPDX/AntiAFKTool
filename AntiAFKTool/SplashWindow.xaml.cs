using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Threading.Tasks;
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

        private async void InitializeWebView()
        {
            try
            {
                await progressbar.EnsureCoreWebView2Async(null);
                progressbar.CoreWebView2InitializationCompleted += Progressbar_CoreWebView2InitializationCompleted;
                progressbar.NavigationCompleted += Progressbar_NavigationCompleted;
                progressbar.WebMessageReceived += WebView2_WebMessageReceived;
                LogError("WebView2 initialized.");
            }
            catch (Exception ex)
            {
                LogError("Error initializing WebView2: " + ex.Message);
                MessageBox.Show("Error initializing WebView2. Check the log for details.");
            }
        }

        private void Progressbar_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                string htmlContent = GetEmbeddedHtmlContent();
                progressbar.NavigateToString(htmlContent);
                LogError("WebView2 CoreWebView2InitializationCompleted succeeded.");
            }
            else
            {
                LogError("WebView2 initialization failed: " + e.InitializationException?.Message);
                MessageBox.Show("Error initializing WebView2. Check the log for details.");
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

                var jsonDocument = System.Text.Json.JsonDocument.Parse(message);
                if (jsonDocument.RootElement.TryGetProperty("type", out var typeElement))
                {
                    string? messageType = typeElement.GetString();
                    if (messageType != null)
                    {
                        switch (messageType)
                        {
                            case "setProgress":
                                if (jsonDocument.RootElement.TryGetProperty("value", out var valueElement))
                                {
                                    double progressValue = valueElement.GetDouble();
                                    LogError($"Received progress update: {progressValue}");
                                }
                                else
                                {
                                    LogError("Progress message received without a 'value' field.");
                                }
                                break;

                            case "toggleParticles":
                                if (jsonDocument.RootElement.TryGetProperty("isActive", out var isActiveElement))
                                {
                                    bool isActive = isActiveElement.GetBoolean();
                                    LogError($"Received toggleParticles command: {isActive}");
                                }
                                else
                                {
                                    LogError("ToggleParticles message received without an 'isActive' field.");
                                }
                                break;

                            default:
                                LogError($"Unknown message type received: {messageType}");
                                break;
                        }
                    }
                    else
                    {
                        LogError("Received a message with a null 'type' field.");
                    }
                }
                else
                {
                    LogError("Received a message without a 'type' field.");
                }
            }
            catch (System.Text.Json.JsonException jsonEx)
            {
                LogError($"Error parsing JSON message: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                LogError($"Unexpected error processing message: {ex.Message}");
            }
        }

        private void StartProgressBar()
        {
            _progress = 0;

            _progressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(ProgressBarDuration * 10)
            };
            _progressTimer.Tick += ProgressTimer_Tick;
            _progressTimer.Start();
        }

        private void ProgressTimer_Tick(object? sender, EventArgs e)
        {
            _progress += 0.1;
            UpdateProgress(_progress);

            if (_progress >= 1)
            {
                _progressTimer?.Stop();
                FadeOut();
            }
        }

        private async void UpdateProgress(double progress)
        {
            if (progressbar.CoreWebView2 is not null)
            {
                string message = $"{{\"type\": \"setProgress\", \"value\": {progress:F2}}}";
                await progressbar.CoreWebView2.ExecuteScriptAsync($"window.chrome.webview.postMessage({message});");
            }
        }

        private void FadeIn()
        {
            var fadeInAnimation = CreateFadeAnimation(0, 1, FadeInDuration);
            fadeInAnimation.Completed += (s, e) => StartProgressBar();
            BeginAnimation(OpacityProperty, fadeInAnimation);
        }

        private void FadeOut()
        {
            var fadeOutAnimation = CreateFadeAnimation(1, 0, FadeOutDuration);
            fadeOutAnimation.Completed += (s, e) => Dispatcher.Invoke(ShowMainWindow);
            BeginAnimation(OpacityProperty, fadeOutAnimation);
        }

        private void ShowMainWindow()
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
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
            return @"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Stellar Cosmic Energy Flow</title>
    <style>
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
        }
        #restartButton {
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            opacity: 0;
            background: linear-gradient(135deg, rgba(255, 255, 255, 0.2) 0%, rgba(255, 255, 255, 0.5) 100%);
            color: #fff;
            border: 2px solid rgba(255, 255, 255, 0.5);
            padding: 15px 32px;
            text-align: center;
            font-size: 18px;
            cursor: pointer;
            transition: all 0.4s;
            border-radius: 50px;
            box-shadow: 0 0 15px rgba(255, 255, 255, 0.5);
        }
        #restartButton:hover {
            background: linear-gradient(135deg, rgba(255, 255, 255, 0.5) 0%, rgba(255, 255, 255, 0.7) 100%);
            box-shadow: 0 0 25px rgba(255, 255, 255, 0.7);
        }
    </style>
</head>
<body>
    <canvas></canvas>
    <button id='restartButton' onclick='restartAnimation()'>Restart the Cosmos</button>
    <script>
        const canvas = document.getElementsByTagName('canvas')[0];
        const ctx = canvas.getContext('2d');
        const button = document.getElementById('restartButton');
        let particles = [];
        let w, h;

        function resizeCanvas() {
            w = canvas.width = window.innerWidth;
            h = canvas.height = window.innerHeight;
        }
        window.addEventListener('resize', resizeCanvas);
        resizeCanvas();

        const totalDuration = 10000;
        let startTime;
        let lastParticleTime = 0;
        let progress = 0;
        let fadeOut = false;

        function reset() {
            ctx.clearRect(0, 0, w, h);
        }

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
                ctx.moveTo(this.x, this.y, this.y + this.height / 2, this.x + this.maxWidth, this.y + this.height);
                ctx.lineTo(this.x, this.y + this.height);
                ctx.closePath();
                ctx.stroke();

                ctx.restore();
            }
        }

        const bar = new ProgressBar();

        function updateProgress(currentTime) {
            if (!startTime) startTime = currentTime;
            const elapsedTime = currentTime - startTime;

            if (elapsedTime < totalDuration) {
                progress = Math.min(1, elapsedTime / totalDuration);
                bar.width = progress * bar.maxWidth;
            } else {
                progress = 1;
                bar.width = bar.maxWidth;
                fadeOut = true;
                bar.opacity = Math.max(0, 1 - (elapsedTime - totalDuration) / 3000);
            }
        }

        function draw(currentTime) {
            reset();
            updateProgress(currentTime);

            // Generate new particles
            if (currentTime - lastParticleTime > 15 && progress < 1) {
                for (let i = 0; i < 8; i++) {
                    particles.push(new Particle());
                }
                lastParticleTime = currentTime;
            }

            // Update and draw particles
            particles = particles.filter(p => {
                p.update();
                p.draw();
                return p.life < p.maxLife && p.radius > 0.2;
            });

            bar.draw();

            if (progress < 1 || fadeOut) {
                requestAnimationFrame(draw);
            } else {
                button.style.opacity = 1; // Show the restart button
            }
        }

        function restartAnimation() {
            startTime = null;
            lastParticleTime = 0;
            progress = 0;
            fadeOut = false;
            bar.width = 0; // Reset the bar width
            bar.opacity = 1; // Ensure the bar is visible
            particles = []; // Clear existing particles
            button.style.opacity = 0; // Hide the button initially
            requestAnimationFrame(draw); // Start drawing loop again
        }

        button.addEventListener('click', function() {
            button.style.opacity = 0;
            setTimeout(restartAnimation, 500);
        });

        requestAnimationFrame(draw); // Start the animation loop
    </script>
</body>
</html>
";
        }

        private static void LogError(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            try
            {
                string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AntiAFKTool", "error.log");
                string? dirPath = Path.GetDirectoryName(logPath);

                if (!string.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                File.AppendAllText(logPath, $"{DateTime.Now}: {message}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logging failed: {ex.Message}");
            }
        }
    }
}