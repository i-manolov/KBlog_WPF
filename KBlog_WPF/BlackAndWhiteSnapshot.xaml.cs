using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using System.IO;
using System.Globalization;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using System.Windows.Threading;

namespace KBlog_WPF
{
    /// <summary>
    /// Interaction logic for BlackAndWhiteSnapshot.xaml
    /// </summary>
    public partial class BlackAndWhiteSnapshot : Window
    {
        private KinectSensor sensor;


        private WriteableBitmap colorBitmap;

        private byte[] colorPixels;

        private readonly KinectSensorChooser sensorChooser;


        public BlackAndWhiteSnapshot()
        {
            InitializeComponent();

            // Dynamically set background images for Buttons Screenshot, Home, Exit
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(@"Images\camera2.png", UriKind.Relative);
            bi.EndInit();

            snapshotBtn.Background = new ImageBrush(bi);
            snapshotBtn.Height = 150;
            snapshotBtn.Width = 150;
            snapshotBtn.HorizontalAlignment = HorizontalAlignment.Center;
            snapshotBtn.VerticalAlignment = VerticalAlignment.Bottom;
            snapshotBtn.ToolTip = "Press to take a picture.";

            BitmapImage bi1 = new BitmapImage();
            bi1.BeginInit();
            bi1.UriSource = new Uri(@"Images\exit2.png", UriKind.Relative);
            bi1.EndInit();

            exitBtn.Background = new ImageBrush(bi1);
            exitBtn.Height = 100;
            exitBtn.Width = 100;
            exitBtn.HorizontalAlignment = HorizontalAlignment.Right;
            exitBtn.VerticalAlignment = VerticalAlignment.Bottom;
            exitBtn.ToolTip = "Press to exit the application";

            BitmapImage bi2 = new BitmapImage();
            bi2.BeginInit();
            bi2.UriSource = new Uri(@"Images\Home-icon.png", UriKind.Relative);
            bi2.EndInit();

            homeBtn.Background = new ImageBrush(bi2);
            homeBtn.Height = 100;
            homeBtn.Width = 100;
            homeBtn.HorizontalAlignment = HorizontalAlignment.Left;
            homeBtn.VerticalAlignment = VerticalAlignment.Bottom;
            homeBtn.ToolTip = "Press to return to Home Screen";

            BitmapImage bi3 = new BitmapImage();
            bi3.BeginInit();
            bi3.UriSource = new Uri(@"Images\save.png", UriKind.Relative);
            bi3.EndInit();

            saveBtn.Background = new ImageBrush(bi3);
            saveBtn.Height = 150;
            saveBtn.Width = 150;
            saveBtn.HorizontalAlignment = HorizontalAlignment.Right;
            saveBtn.VerticalAlignment = VerticalAlignment.Top;
            saveBtn.ToolTip = "Save";
            saveBtn.Visibility = Visibility.Hidden;
            saveBtn.Click += new RoutedEventHandler(saveBtn_Click);

            BitmapImage bi4 = new BitmapImage();
            bi4.BeginInit();
            bi4.UriSource = new Uri(@"Images\delete.png", UriKind.Relative);
            bi4.EndInit();

            cancelBtn.Background = new ImageBrush(bi4);
            cancelBtn.Height = 150;
            cancelBtn.Width = 150;
            cancelBtn.ToolTip = "Delete";
            cancelBtn.HorizontalAlignment = HorizontalAlignment.Left;
            cancelBtn.VerticalAlignment = VerticalAlignment.Top;
            cancelBtn.Visibility = Visibility.Hidden;
            cancelBtn.Click += new RoutedEventHandler(cancelBtn_Click);

            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();

            // Bind the sensor chooser's current sensor to the KinectRegion
            var regionSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);
        }

        private static void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.SkeletonStream.Enable();

                    try
                    {
                        args.NewSensor.DepthStream.Range = DepthRange.Near;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }

        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the color stream to receive color frames
                this.sensor.ColorStream.Enable(ColorImageFormat.RawYuvResolution640x480Fps15);

                // Allocate space to put the pixels we'll receive
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

                // This is the bitmap we'll display on-screen
                this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Gray16, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                this.Image.Source = this.colorBitmap;

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.ColorFrameReady += this.SensorColorFrameReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.statusBarText.Visibility = Visibility.Visible;
                this.statusBarText.Text = Properties.Resources.NoKinectReady;
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's ColorFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * colorFrame.BytesPerPixel,
                        0);
                }
            }
        }

        private void snapshotBtn_Click(object sender, RoutedEventArgs e)
        {
            if (null == this.sensor)
            {
                this.statusBarText.Visibility = Visibility.Visible;
                this.statusBarText.Text = Properties.Resources.ConnectDeviceFirst;
                return;
            }


            snapshotBtn.Visibility = Visibility.Hidden;
            countDownFrame.Content = new Timers.Timer();
            countDownFrame.Visibility = Visibility.Visible;
            countDownFrame.HorizontalAlignment = HorizontalAlignment.Center;

            DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 10);
            dispatcherTimer.Start();

        }


        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // code goes here
            DispatcherTimer timer = sender as DispatcherTimer;
            timer.Stop(); // execute only once;
            if (timer.IsEnabled == true)
            {
                string test = "STILL Ticking, Tick Tock";
            }
            else
            {
                // create a png bitmap encoder which knows how to save a .png file

                BitmapEncoder encoder = new PngBitmapEncoder();
                // create frame from the writable bitmap and add to encoder
                encoder.Frames.Add(BitmapFrame.Create(this.colorBitmap));
                this.Image.Source = encoder.Frames[0];

                string mediaPath = ApplicationState.GetValue<string>("kblog_media");
                string user_path = Path.Combine(mediaPath, ApplicationState.GetValue<string>("user_id"), "BlackAndWhiteSnapshot");
                if (!Directory.Exists(user_path))
                {
                    Directory.CreateDirectory(user_path);
                }
                string fileGUID = Guid.NewGuid().ToString();
                string bwPath = Path.Combine(user_path, fileGUID + ".png");

                using (FileStream fs = new FileStream(bwPath, FileMode.Create))
                {
                    encoder.Save(fs);

                }
                ApplicationState.SetValue("bwPath", bwPath);
                saveSnapshot();
                encoder = null;
            }
        }


        public void saveSnapshot()
        {
            saveBtn.Visibility = Visibility.Visible;
            cancelBtn.Visibility = Visibility.Visible;
            countDownFrame.Visibility = Visibility.Hidden;
            saveLbl.Visibility = Visibility.Visible;

        }



        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            UnregisterEvent();
            ColorSnapshot cs = new ColorSnapshot();
            cs.Show();
            this.Close();

        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            // save file_info in db
            Media_DB_Query media_db_query = new Media_DB_Query();
            media_db_query.saveMedia(ApplicationState.GetValue<string>("user_id"),
                    "BlackAndWhiteSnapshot", ApplicationState.GetValue<string>("bwPath"));


            UnregisterEvent();
            BlackAndWhiteSnapshot bws = new BlackAndWhiteSnapshot();
            bws.Show();
            this.Close();

        }

        private void finishedColorSnapshot()
        {
            saveBtn.Visibility = Visibility.Hidden;
            cancelBtn.Visibility = Visibility.Hidden;
            saveLbl.Visibility = Visibility.Hidden;

            snapshotBtn.Visibility = Visibility.Visible;
        }

        private void homeBtn_Click(object sender, RoutedEventArgs e)
        {
            UnregisterEvent();
            HomeScreen home = new HomeScreen();
            home.Show();
            this.Close();

        }

        private void exitBtn_Click(object sender, RoutedEventArgs e)
        {
            UnregisterEvent();
            this.Close();
        }

        private void UnregisterEvent()
        {

            if (null != this.sensor)
            {
                this.sensor.ColorFrameReady -= this.SensorColorFrameReady;

                this.sensor.Stop();
                this.sensor = null;
            }
        }

    }
}

