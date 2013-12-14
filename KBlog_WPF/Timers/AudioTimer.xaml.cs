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

namespace KBlog_WPF.Timers
{
    /// <summary>
    /// Interaction logic for AudioTimer.xaml
    /// </summary>
    public partial class AudioTimer : Page
    {
        public AudioTimer()
        {
            InitializeComponent();
        }

        private void ttbCountDown_OnCountDownComplete(object sender, EventArgs e)
        {
            ttbCountDown.Background = Brushes.Red;
            ttbCountDown.Foreground = Brushes.White;
        }
    }
}
