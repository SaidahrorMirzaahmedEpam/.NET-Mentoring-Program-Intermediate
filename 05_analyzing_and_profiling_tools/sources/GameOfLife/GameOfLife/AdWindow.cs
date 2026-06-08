using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace GameOfLife
{
    class AdWindow : Window
    {
        private readonly DispatcherTimer adTimer;
        private int imgNmb;
        private string link;

        // FIX #3: Cache brushes once — no new ImageBrush/BitmapImage on every tick
        private static readonly ImageBrush[] _cachedBrushes = CreateBrushes();

        private static ImageBrush[] CreateBrushes()
        {
            var uris = new[] { "ad1.jpg", "ad2.jpg", "ad3.jpg" };
            var brushes = new ImageBrush[3];
            for (int k = 0; k < 3; k++)
            {
                brushes[k] = new ImageBrush(new BitmapImage(new Uri(uris[k], UriKind.Relative)));
                brushes[k].Freeze(); // Freeze makes WPF brush immutable and faster to render
            }
            return brushes;
        }

        public AdWindow(Window owner)
        {
            Random rnd = new Random();
            Owner = owner;
            Width = 350;
            Height = 100;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.ToolWindow;
            Title = "Support us by clicking the ads";
            Cursor = Cursors.Hand;
            ShowActivated = false;
            MouseDown += OnClick;

            imgNmb = rnd.Next(1, 3);
            ChangeAds(this, new EventArgs());

            // Run the timer that changes the ad's image
            adTimer = new DispatcherTimer();
            adTimer.Interval = TimeSpan.FromSeconds(3);
            adTimer.Tick += ChangeAds;
            adTimer.Start();
        }

        private void OnClick(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(link);
            Close();
        }

        // FIX #1: Stop the timer and unsubscribe BOTH event handlers on close.
        // Previously Unsubscribe() was commented out, and even that only removed
        // adTimer.Tick — MouseDown was never removed.
        protected override void OnClosed(EventArgs e)
        {
            adTimer.Stop();
            adTimer.Tick -= ChangeAds;
            MouseDown -= OnClick;
            base.OnClosed(e);
        }

        // FIX #3: Use cached brush — zero allocations per tick
        private void ChangeAds(object sender, EventArgs eventArgs)
        {
            Background = _cachedBrushes[imgNmb - 1];
            link = "http://example.com";
            imgNmb = (imgNmb % 3) + 1;
        }
    }
}
