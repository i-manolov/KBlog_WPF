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

namespace KBlog_WPF
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit;
    using Microsoft.Kinect.Toolkit.Controls;

    /// <summary>
    /// Interaction logic for HomeScreen.xaml
    /// </summary>
    public partial class HomeScreen : Window
    {

        public static readonly DependencyProperty PageLeftEnabledProperty = DependencyProperty.Register(
    "PageLeftEnabled", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public static readonly DependencyProperty PageRightEnabledProperty = DependencyProperty.Register(
            "PageRightEnabled", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        private const double ScrollErrorMargin = 0.001;

        private const int PixelScrollByAmount = 20;

        private readonly KinectSensorChooser sensorChooser;

        KinectTileButton colorKinectBtn, infraredKinectBtn, audioKinectBtn, bwKinectBtn, settingsKinectBtn;

        public HomeScreen()
        {
            InitializeComponent();
            // initialize the sensor chooser and UI
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();

            // Bind the sensor chooser's current sensor to the KinectRegion
            var regionSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);

            // get name of user 
            User_DB_Query query = new User_DB_Query();
            userNameLbl.Content += query.get_user_name(ApplicationState.GetValue<string>("user_id"));

            // Clear out placeholder content
            this.wrapPanel.Children.Clear();

            //Add in display content
            for (var index = 0; index < 5; ++index)
            {
                if (index == 0)
                {
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(@"Images\rgb_background.jpg", UriKind.Relative);
                    bi.EndInit();

                    colorKinectBtn = new KinectTileButton
                    {
                        Background = new ImageBrush(bi),
                        Tag = "TAG",
                        Label = "Color Image",
                        Height = 250,
                        Width = 272
                    };
                    this.wrapPanel.Children.Add(colorKinectBtn);
                    colorKinectBtn.Click += new RoutedEventHandler(colorKinectBtn_Click);
                }
                if (index == 1)
                {
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(@"Images\w_and_b.jpg", UriKind.Relative);
                    bi.EndInit();

                    bwKinectBtn = new KinectTileButton
                    {
                        Background = new ImageBrush(bi),
                        Tag = "TAG",
                        Label = "B&W Image",
                        Height = 250,
                        Width = 272
                    };
                    this.wrapPanel.Children.Add(bwKinectBtn);
                    bwKinectBtn.Click += new RoutedEventHandler(bwKinectBtn_Click);
                }

                if (index == 2)
                {
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(@"Images\infrared.png", UriKind.Relative);
                    bi.EndInit();

                    infraredKinectBtn = new KinectTileButton
                    {
                        Background = new ImageBrush(bi),
                        Label = "Infrared Image",
                        Height = 250,
                        Width = 272
                    };
                    this.wrapPanel.Children.Add(infraredKinectBtn);
                    infraredKinectBtn.Click += new RoutedEventHandler(infraredKinectBtn_Click);
                }
                if (index == 3)
                {
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(@"Images\audio2.jpg", UriKind.Relative);
                    bi.EndInit();

                    audioKinectBtn = new KinectTileButton
                    {
                        Background = new ImageBrush(bi),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Tag = "TAG",
                        Label = "Record Audio",
                        Height = 250,
                        Width = 272
                    };
                    this.wrapPanel.Children.Add(audioKinectBtn);
                    audioKinectBtn.Click += new RoutedEventHandler(audioKinectBtn_Click);
                }
                if (index == 4)
                {
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(@"Images\settings.png", UriKind.Relative);
                    bi.EndInit();

                    settingsKinectBtn = new KinectTileButton
                    {
                        Background = new ImageBrush(bi),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Tag = "TAG",
                        Label = "Record Audio",
                        Height = 250,
                        Width = 272
                    };
                    this.wrapPanel.Children.Add(settingsKinectBtn);
                    settingsKinectBtn.Click += new RoutedEventHandler(settingsKinectBtn_Click);
                }
            }


            // Bind listner to scrollviwer scroll position change, and check scroll viewer position
            //this.UpdatePagingButtonState();
            //scrollViewer.ScrollChanged += (o, e) => this.UpdatePagingButtonState();
        }


        /// <summary>
        /// CLR Property Wrappers for PageLeftEnabledProperty
        /// </summary>
        public bool PageLeftEnabled
        {
            get
            {
                return (bool)GetValue(PageLeftEnabledProperty);
            }

            set
            {
                this.SetValue(PageLeftEnabledProperty, value);
            }
        }

        /// <summary>
        /// CLR Property Wrappers for PageRightEnabledProperty
        /// </summary>
        public bool PageRightEnabled
        {
            get
            {
                return (bool)GetValue(PageRightEnabledProperty);
            }

            set
            {
                this.SetValue(PageRightEnabledProperty, value);
            }
        }

        /// <summary>
        /// Called when the KinectSensorChooser gets a new sensor
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="args">event arguments</param>
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
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.sensorChooser.Stop();
        }

        private void stopKinectSensor()
        {
            sensorChooser.Stop();
        }



        /// <summary>
        /// Handle paging right (next button).
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void PageRightButtonClick(object sender, RoutedEventArgs e)
        {
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + PixelScrollByAmount);
        }

        /// <summary>
        /// Handle paging left (previous button).
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void PageLeftButtonClick(object sender, RoutedEventArgs e)
        {
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - PixelScrollByAmount);
        }

        /// <summary>
        /// Change button state depending on scroll viewer position
        /// </summary>
        private void UpdatePagingButtonState()
        {
            this.PageLeftEnabled = scrollViewer.HorizontalOffset > ScrollErrorMargin;
            this.PageRightEnabled = scrollViewer.HorizontalOffset < scrollViewer.ScrollableWidth - ScrollErrorMargin;
        }

        private void colorKinectBtn_Click(object sender, RoutedEventArgs e)
        {
            unregisterEvent();
            sensorChooser.Stop();
            ColorSnapshot colorSnapshot = new ColorSnapshot();
            colorSnapshot.Show();
            this.Close();
        }

        // MOST IMPORTANT FUNCCCCCCCCCCTION 
        private void unregisterEvent()
        {
            this.sensorChooser.KinectChanged -= SensorChooserOnKinectChanged;
        }

        void infraredKinectBtn_Click(object sender, RoutedEventArgs e)
        {
            unregisterEvent();
            sensorChooser.Stop();
            InfraredSnapshot ir = new InfraredSnapshot();
            ir.Show();
            this.Close();
        }

        private void audioKinectBtn_Click(object sender, RoutedEventArgs e)
        {
            unregisterEvent();
            sensorChooser.Stop();
            AudioRecord audioRecord = new AudioRecord();
            audioRecord.Show();
            //stopKinectSensor();
            //Window3 window = new Window3();
            //window.Show();
            this.Close();
        }

        private void bwKinectBtn_Click(object sender, RoutedEventArgs e)
        {
            unregisterEvent();
            sensorChooser.Stop();
            BlackAndWhiteSnapshot bws = new BlackAndWhiteSnapshot();
            bws.Show();
            this.Close();
        }

        private void settingsKinectBtn_Click(object sender, RoutedEventArgs e)
        {
            unregisterEvent();
            sensorChooser.Stop();
            KinectSettings ks = new KinectSettings();
            ks.Show();
            this.Close();
        }



    }
}
