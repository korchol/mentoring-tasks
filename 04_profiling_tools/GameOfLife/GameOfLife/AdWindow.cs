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
        private readonly BitmapImage[] adImages;
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

            // load images just once
            adImages = new BitmapImage[]
            {
                new BitmapImage(new Uri("ad1.jpg", UriKind.Relative)),
                new BitmapImage(new Uri("ad2.jpg", UriKind.Relative)),
                new BitmapImage(new Uri("ad3.jpg", UriKind.Relative)),
            };
            imgNmb = rnd.Next(0, 2);
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
            Dispose();
            base.OnClosed(e);
        }

        public void Dispose()
        {
            adTimer.Stop();
            adTimer.Tick -= ChangeAds;
            MouseDown -= OnClick;
        }

        public void Unsubscribe()
        {
            adTimer.Tick -= ChangeAds;
        }

        private void ChangeAds(object sender, EventArgs eventArgs)
        {
            
            ImageBrush myBrush = new ImageBrush();
            
            switch (imgNmb)
            {
                case 0:
                    myBrush.ImageSource = adImages[imgNmb];
                    Background = myBrush;
                    link = "http://example.com";
                    imgNmb++;
                    break;
                case 1:
                    myBrush.ImageSource = adImages[imgNmb];
                    Background = myBrush;
                    link = "http://example.com";
                    imgNmb++;
                    break;
                case 2:
                    myBrush.ImageSource = adImages[imgNmb];
                    Background = myBrush;
                    link = "http://example.com";
                    imgNmb = 1;
                    break;
            }
            
        }
    }
}