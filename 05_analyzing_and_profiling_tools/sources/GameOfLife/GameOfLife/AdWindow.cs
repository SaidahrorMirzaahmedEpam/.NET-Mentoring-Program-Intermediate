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
        private DispatcherTimer adTimer;
        private int imgNmb;     // the number of the image currently shown
        private string link;    // the URL where the currently shown ad leads to
        
    
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
        
        protected override void OnClosed(EventArgs e)
        {
            // Stop timer and unsubscribe to allow GC
            if (adTimer != null)
            {
                adTimer.Stop();
                adTimer.Tick -= ChangeAds;
                adTimer = null;
            }

            // Remove background image reference so the BitmapImage can be collected
            try
            {
                Background = null;
            }
            catch { }

            base.OnClosed(e);
        } 

        public void Unsubscribe()
        {
            if (adTimer != null)
            {
                adTimer.Tick -= ChangeAds;
                adTimer.Stop();
            }
        }

        private void ChangeAds(object sender, EventArgs eventArgs)
        {
            ImageBrush myBrush = new ImageBrush();

            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri($"ad{imgNmb}.jpg", UriKind.Relative);
            bmp.CacheOption = BitmapCacheOption.OnLoad; // ensure file not locked
            bmp.EndInit();
            bmp.Freeze();

            myBrush.ImageSource = bmp;
            Background = myBrush;

            switch (imgNmb)
            {
                case 1:
                    link = "http://example.com";
                    imgNmb++;
                    break;
                case 2:
                    link = "http://example.com";
                    imgNmb++;
                    break;
                case 3:
                    link = "http://example.com";
                    imgNmb = 1;
                    break;
            }
        }
    }
}