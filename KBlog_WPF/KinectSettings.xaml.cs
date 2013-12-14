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
using System.Windows.Shapes;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using System.IO;

namespace KBlog_WPF
{
    /// <summary>
    /// Interaction logic for KinectSettings.xaml
    /// </summary>
    public partial class KinectSettings : Window
    {
        private KinectSensor sensor;
        private readonly KinectSensorChooser sensorChooser;

        private WriteableBitmap colorBitmap;

        private byte[] colorPixels;

        public static readonly DependencyProperty PageUpEnabledProperty = DependencyProperty.Register(
 "PageUpEnabled", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public static readonly DependencyProperty PageDownEnabledProperty = DependencyProperty.Register(
            "PageDownEnabled", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public KinectSettings()
        {
            InitializeComponent();
            BitmapImage bi1 = new BitmapImage();
            bi1.BeginInit();
            bi1.UriSource = new Uri(@"Images\exit2.png", UriKind.Relative);
            bi1.EndInit();

            exitBtn.Background = new ImageBrush(bi1);
            exitBtn.Height = 100;
            exitBtn.Width = 100;
            exitBtn.ToolTip = "Press to exit the application";

            BitmapImage bi2 = new BitmapImage();
            bi2.BeginInit();
            bi2.UriSource = new Uri(@"Images\Home-icon.png", UriKind.Relative);
            bi2.EndInit();

            homeBtn.Background = new ImageBrush(bi2);
            homeBtn.Height = 100;
            homeBtn.Width = 100;
            homeBtn.ToolTip = "Press to return to Home Screen";

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

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {

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
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                // Allocate space to put the pixels we'll receive
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

                // This is the bitmap we'll display on-screen
                this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                this.Image.Source = this.colorBitmap;

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.ColorFrameReady += this.SensorColorFrameReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                    sliderAngle.Value = this.sensor.ElevationAngle;
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                //this.statusBarText.Text = Properties.Resources.NoKinectReady;
            }
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }

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
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }

        private void btnSetAngle_Click(object sender, RoutedEventArgs e)
        {
            this.sensor.ElevationAngle = (int)sliderAngle.Value;
        }

        private void sliderAngle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblSliderValue.Content = (int)sliderAngle.Value;
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

        private void PageDownButtonClick(object sender, RoutedEventArgs e)
        {
            sliderAngle.Value -= 1;
        }

        private void PageUpButtonClick(object sender, RoutedEventArgs e)
        {
            sliderAngle.Value += 1;
        }
    }
}
