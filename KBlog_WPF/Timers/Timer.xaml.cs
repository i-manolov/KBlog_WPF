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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect.Toolkit.Controls;
using Microsoft.Kinect.Toolkit;
using System.Threading;
using System.Diagnostics;
namespace KBlog_WPF.Timers
{
    /// <summary>
    /// Interaction logic for Timer.xaml
    /// </summary>
    public partial class Timer : Page
    {
        public Timer()
        {
            InitializeComponent();
        }

        private void ttbCountDown_OnCountDownComplete(object sender, EventArgs e)
        {
            ttbCountDown.Background = Brushes.Green;
            ttbCountDown.Foreground = Brushes.White;
        }
    }
}
