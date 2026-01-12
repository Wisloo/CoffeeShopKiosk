using System;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace CoffeeShopKiosk.Views
{
    public partial class ThemeVisualsControl : UserControl
    {
        private DispatcherTimer _spawnTimer;
        private Random _rnd = new Random();
        private string _currentTheme = "Cafe";
        private int _maxParticles = 6; // even fewer particles for subtle effect

        public ThemeVisualsControl()
        {
            InitializeComponent();
            _spawnTimer = new DispatcherTimer();
            _spawnTimer.Interval = TimeSpan.FromMilliseconds(350);
            _spawnTimer.Tick += SpawnTimer_Tick;
        }

        public void SetTheme(string theme)
        {
            _currentTheme = (theme ?? "Cafe").ToLower();
            if (_currentTheme == "rain")
            {
                // faster spawns for rain
                _spawnTimer.Interval = TimeSpan.FromMilliseconds(220);
                _spawnTimer.Start();
            }
            else if (_currentTheme == "cafe")
            {
                // very slow, gentle particles for cafe
                _spawnTimer.Interval = TimeSpan.FromMilliseconds(1400);
                _spawnTimer.Start();
            }
            else
            {
                StopEffects();
            }
        }

        private void StartRain()
        {
            _spawnTimer.Start();
        }

        public void StopEffects()
        {
            _spawnTimer.Stop();
            DropCanvas.Children.Clear();
        }

        private void SpawnTimer_Tick(object sender, EventArgs e)
        {
            if (_currentTheme == "rain")
            {
                SpawnRainDrop();
            }
            else if (_currentTheme == "cafe")
            {
                SpawnCafeParticle();
            }
        }

        private void SpawnRainDrop()
        {
            // limit drops
            if (DropCanvas.Children.Count > _maxParticles) return;

            var width = Math.Max(100, (int)DropCanvas.ActualWidth);
            var startX = _rnd.NextDouble() * width;
            var size = 2 + _rnd.NextDouble() * 5; // narrow raindrop

            var drop = new Rectangle
            {
                Width = size,
                Height = size * 4,
                Fill = new SolidColorBrush(Color.FromArgb(200, 160, 200, 230)),
                RadiusX = (float)size / 2,
                RadiusY = (float)size / 2,
                RenderTransform = new TranslateTransform()
            };

            Canvas.SetLeft(drop, startX);
            Canvas.SetTop(drop, -20);
            DropCanvas.Children.Add(drop);

            var duration = TimeSpan.FromSeconds(1.6 + _rnd.NextDouble()*1.4);
            var anim = new DoubleAnimation
            {
                From = -20,
                To = DropCanvas.ActualHeight + 40,
                Duration = new Duration(duration),
                EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseIn }
            };

            var opacityAnim = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(duration)
            };

            anim.Completed += (s, ev) =>
            {
                DropCanvas.Children.Remove(drop);
            };

            (drop.RenderTransform as TranslateTransform).BeginAnimation(TranslateTransform.YProperty, anim);
            drop.BeginAnimation(UIElement.OpacityProperty, opacityAnim);
        }

        private void SpawnCafeParticle()
        {
            // fewer, softer particles
            if (DropCanvas.Children.Count > _maxParticles) return;
            var width = Math.Max(100, (int)DropCanvas.ActualWidth);
            var startX = _rnd.NextDouble() * width;
            var size = 6 + _rnd.NextDouble() * 8; // modest soft particle

            var el = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = new SolidColorBrush(Color.FromArgb(40, 255, 245, 230)), // very subtle warm cream
                Opacity = 0.35,
                RenderTransform = new TranslateTransform()
            };

            // start near bottom, float upward
            Canvas.SetLeft(el, startX);
            var startY = DropCanvas.ActualHeight - 40 - _rnd.NextDouble() * 80;
            Canvas.SetTop(el, startY);
            DropCanvas.Children.Add(el);

            var duration = TimeSpan.FromSeconds(3.5 + _rnd.NextDouble() * 2.5);

            var anim = new DoubleAnimation
            {
                From = startY,
                To = -40,
                Duration = new Duration(duration),
                EasingFunction = new SineEase() { EasingMode = EasingMode.EaseOut }
            };

            var opacityAnim = new DoubleAnimation
            {
                From = 0.6,
                To = 0.0,
                Duration = new Duration(duration)
            };

            // gentle horizontal drift
            var drift = new DoubleAnimation
            {
                From = 0,
                To = (_rnd.NextDouble() - 0.5) * 60,
                Duration = new Duration(duration)
            };

            anim.Completed += (s, ev) =>
            {
                DropCanvas.Children.Remove(el);
            };

            var tt = el.RenderTransform as TranslateTransform;
            tt.BeginAnimation(TranslateTransform.YProperty, anim);
            tt.BeginAnimation(TranslateTransform.XProperty, drift);
            el.BeginAnimation(UIElement.OpacityProperty, opacityAnim);
        }
    }
}