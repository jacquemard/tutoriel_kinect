using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using Microsoft.Kinect.Wpf.Controls;

namespace KinectDemos
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            KinectRegion.SetKinectRegion(this, kinectRegion);

            var app = ((App)Application.Current);
            app.KinectRegion = kinectRegion;

            this.kinectRegion.KinectSensor = KinectSensor.GetDefault();

            var dataSource = DataSource.GetGroup("Group-1");
            this.itemsControl.ItemsSource = dataSource;
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)e.OriginalSource;
            var dataItem = button.DataContext as DataItem;

            if (dataItem != null && dataItem.NavigationPage != null)
            {
                backButton.Visibility = Visibility.Visible;
                navigationRegion.Content = Activator.CreateInstance(dataItem.NavigationPage);
            }
        }

        /// <summary>
        /// Handle the back button click.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void GoBack(object sender, RoutedEventArgs e)
        {
            backButton.Visibility = Visibility.Hidden;
            navigationRegion.Content = this.kinectRegionGrid;
        }
    }
}