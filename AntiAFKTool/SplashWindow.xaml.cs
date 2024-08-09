using Microsoft.Web.WebView2.Core;
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
            progressbar.NavigationCompleted += Progressbar_NavigationCompleted;
            progressbar.CoreWebView2InitializationCompleted += Progressbar_CoreWebView2InitializationCompleted;
        }

        private void Progressbar_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (progressbar.CoreWebView2 != null)
            {
                progressbar.CoreWebView2.WebMessageReceived += WebView2_WebMessageReceived;
                string htmlContent = GetEmbeddedHtmlContent();
                progressbar.NavigateToString(htmlContent);
            }
        }

        private void WebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            // Handle messages from JavaScript
            string jsMessage = NewMethod(e);
            if (jsMessage == "progressCompleted")
                FadeOut();

            static string NewMethod(CoreWebView2WebMessageReceivedEventArgs e)
            {
                return e.TryGetWebMessageAsString();
            }
        }

        private void Progressbar_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            StartProgressBar();
        }

        private void StartProgressBar()
        {
            _progress = 0;
            _progressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(ProgressBarDuration / 100)
            };
            _progressTimer.Tick += ProgressTimer_Tick;
            _progressTimer.Start();
        }

        private void ProgressTimer_Tick(object? sender, EventArgs e)
        {
            _progress += 0.01;
            SetProgress(_progress, "{\"type\": \"setProgress\", \"value\": ");

            if (_progress >= 1)
            {
                _progressTimer?.Stop();
                progressbar.CoreWebView2?.PostWebMessageAsString("{\"type\":\"progressCompleted\"}");
            }
        }

        private void SetProgress(double progress, string typesetProgressvalue)
        {
            progressbar.CoreWebView2?.PostWebMessageAsString(
                typesetProgressvalue + progress.ToString("F2") + "}");
        }

        private void FadeIn()
        {
            var fadeInAnimation = CreateFadeAnimation(0, 1, FadeInDuration);
            fadeInAnimation.Completed += (s, e) => StartProgressBar();
            BeginAnimation(OpacityProperty, fadeInAnimation);
        }

        private void FadeOut()
        {
            ToggleParticles(false);
            var fadeOutAnimation = CreateFadeAnimation(1, 0, FadeOutDuration);
            fadeOutAnimation.Completed += (s, e) => Dispatcher.Invoke(ShowMainWindow);
            BeginAnimation(OpacityProperty, fadeOutAnimation);
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

        private void ToggleParticles(bool isActive)
        {
            ToggleParticles(isActive, progressbar);
        }

        private static void ToggleParticles(bool isActive, Microsoft.Web.WebView2.Wpf.WebView2 progressbarWeb)
        {
            progressbarWeb.CoreWebView2?.PostWebMessageAsString(
                "{\"type\": \"toggleParticles\", \"isActive\": " + isActive.ToString().ToLower() + "}");
        }

        // This method creates a fade animation
        private static DoubleAnimation CreateFadeAnimation(double from, double to, double durationSeconds)
        {
            return new DoubleAnimation(from, to, new Duration(TimeSpan.FromSeconds(durationSeconds)))
            {
                EasingFunction = new QuadraticEase()
            };
        }

        private static string GetEmbeddedHtmlContent()
        {
            string css = @"
body { margin: 0; padding: 0; overflow: hidden; background: radial-gradient(circle, #1b2735 0%, #090a0f 100%); font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; }
canvas { position: fixed; top: 0; left: 0; }";

            string js = @"
const canvas = document.getElementsByTagName('canvas')[0];
const ctx = canvas.getContext('2d');
let particles = [];
let w, h;
let progress = 0;
let particlesActive = true;

function resizeCanvas() { w = canvas.width = window.innerWidth; h = canvas.height = window.innerHeight; }
window.addEventListener('resize', resizeCanvas);
resizeCanvas();

function ProgressBar() { 
    this.width = 0; this.height = 40; this.opacity = 1; this.x = w * 0.1; this.y = h * 0.5; this.maxWidth = w * 0.8; 
    this.draw = function () { 
        ctx.save(); ctx.globalAlpha = this.opacity; ctx.shadowColor = 'rgba(255, 255, 255, 0.7)'; ctx.shadowBlur = 30; 
        const gradient = ctx.createLinearGradient(this.x, this.y, this.x + this.maxWidth, this.y); 
        gradient.addColorStop(0, 'rgba(173, 216, 230, 0.8)'); gradient.addColorStop(0.5, 'rgba(0, 191, 255, 0.8)'); gradient.addColorStop(1, 'rgba(25, 25, 112, 0.8)'); 
        ctx.fillStyle = gradient; 
        ctx.beginPath(); ctx.moveTo(this.x, this.y); ctx.lineTo(this.x + this.width, this.y); 
        ctx.quadraticCurveTo(this.x + this.width + 25, this.y + this.height / 2, this.x + this.width, this.y + this.height); 
        ctx.lineTo(this.x, this.y + this.height); ctx.closePath(); ctx.fill(); 
        ctx.strokeStyle = 'rgba(255, 255, 255, 0.7)'; ctx.lineWidth = 3; ctx.beginPath(); 
        ctx.moveTo(this.x, this.y); ctx.lineTo(this.x + this.maxWidth, this.y); 
        ctx.quadraticCurveTo(this.x + this.maxWidth + 25, this.y + this.height / 2, this.x + this.maxWidth, this.y + this.height); 
        ctx.lineTo(this.x, this.y + this.height); ctx.closePath(); ctx.stroke(); ctx.restore(); 
    }; 
}

function Particle() { 
    this.x = bar.x + bar.width; this.y = bar.y + bar.height / 2; this.vx = (Math.random() - 0.5) * 12; this.vy = (Math.random() - 0.5) * 12; this.radius = Math.random() * 5 + 2; this.life = 0; this.maxLife = 250 + Math.random() * 150; 
    this.color = `hsl(${Math.random() * 60 + 240}, 100%, 70%)`; 
    this.update = function() { this.x += this.vx; this.y += this.vy; this.vx *= 0.98; this.vy *= 0.98; this.life++; if (this.life > this.maxLife / 2) { this.radius *= 0.98; } }; 
    this.draw = function() { ctx.beginPath(); ctx.arc(this.x, this.y, this.radius, 0, Math.PI * 2); ctx.fillStyle = this.color; ctx.globalAlpha = 1 - (this.life / this.maxLife); ctx.fill(); ctx.globalAlpha = 1; }; 
}

const bar = new ProgressBar();

function updateProgress() { 
    bar.width = progress * bar.maxWidth; 
}

function draw() { 
    reset(); updateProgress(); 
    if (particlesActive && progress < 1) { 
        for (let i = 0; i < 8; i++) { particles.push(new Particle()); } 
    } 
    particles = particles.filter(p => { p.update(); p.draw(); return p.life < p.maxLife && p.radius > 0.2; }); 
    bar.draw(); requestAnimationFrame(draw); 
}

function reset() { 
    ctx.clearRect(0, 0, w, h); 
}

window.chrome.webview.addEventListener('jsMessage', event => { 
    const jsMessage = JSON.parse(event.data); 
    if (jsMessage.type === 'setProgress') { 
        progress = jsMessage.value; 
    } else if (jsMessage.type === 'toggleParticles') { 
        particlesActive = jsMessage.isActive; 
    } 
});

requestAnimationFrame(draw);";

            string html = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Stellar Cosmic Energy Flow</title>
    <style>{css}</style>
</head>
<body>
    <canvas></canvas>
    <script>{js}</script>
</body>
</html>";

            return html;
        }
    }
}