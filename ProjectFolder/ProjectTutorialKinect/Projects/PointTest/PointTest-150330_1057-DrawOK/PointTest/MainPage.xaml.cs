using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Kinect;
using WindowsPreview.Kinect;
using WindowsPreview.Kinect.Input;
using System.Diagnostics;
using System.ComponentModel;
using Windows.UI.Xaml.Shapes;
using Windows.UI;

// Pour en savoir plus sur le modèle d'élément Page vierge, consultez la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace PointTest
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {


        //Positions
        private double _posX = 500;
        public double posX
        { 
            get { return _posX; } 
            set 
            {
                _posX = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs("posX"));
            } 
        }

        private double _posY = 500;
        public double posY
        {
            get { return _posY; }
            set
            {
                _posY = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs("posY"));
            }
        }

        //---------- Kinect
        private KinectSensor kinect;

        private KinectCoreWindow kinectCore;

        //-------------------

        //----- Drawing
        private Point? lastPosition = null;

        private Brush whiteBrush = new SolidColorBrush(Colors.SlateGray);
        private Brush blueBrush = new SolidColorBrush(Color.FromArgb(255, 0, 162, 232));
        private Brush drawBrush;
        //-----------


        public MainPage()
        {
            this.InitializeComponent();

            this.DataContext = this;


            //Quand la page est chargée
            this.Loaded += MainPage_Loaded;
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            drawBrush = whiteBrush;

            //Création de kinect
            this.kinect = KinectSensor.GetDefault();

            this.kinectCore = KinectCoreWindow.GetForCurrentThread();

            kinectCore.PointerEntered += kinectCore_PointerEntered;
            kinectCore.PointerMoved += kinectCore_PointerMoved;
            

            this.kinect.Open();


        }


        void kinectCore_PointerEntered(KinectCoreWindow sender, KinectPointerEventArgs args)
        {
            KinectPointerPoint point = args.CurrentPoint;
            if (point.Properties.HandType == HandType.RIGHT)
            {
            }
        }

        void kinectCore_PointerMoved(KinectCoreWindow sender, KinectPointerEventArgs args)
        {
            KinectPointerPoint point = args.CurrentPoint;


            if (point.Properties.HandType == HandType.RIGHT)
            {


                //Diamètre du cercle
                double diameter = 30;
                diameter = diameter * Math.Pow(point.Properties.HandReachExtent + 0.5, 3);
                

                this.posX = point.Position.X * CursorMove.ActualWidth - 15;
                this.posY = point.Position.Y * CursorMove.ActualHeight - 15;

                if(point.Properties.HandReachExtent > 0.5)
                {

                    //Add line
                    if (lastPosition != null)
                    {                        
                        Line line = new Line() { X1 = lastPosition.Value.X, Y1 = lastPosition.Value.Y, X2 = posX+15, Y2 = posY +15, StrokeThickness = diameter, Stroke = this.drawBrush, StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round };
                        Debug.WriteLine(line.Y1);
                        CursorMove.Children.Insert(0, line);
                    }

                    //Update the last position
                    lastPosition = new Point(posX + 15, posY + 15);
                    
                    

                    //Add ellipse --> DRAWING Lines is better
                    //Ellipse form = new Ellipse() { Height = diameter, Width = diameter, Margin = new Thickness(posX + 15 - diameter/2, posY+15-diameter/2, 0, 0), Fill = new SolidColorBrush(Colors.Red), Opacity=1f };
                    //CursorMove.Children.Insert(0,form);


                }
                else
                {
                    lastPosition = null;
                }
                
            }
            

            //INdique qu'on a traité le point
            args.Handled = true;
        }


        public event PropertyChangedEventHandler PropertyChanged;
    }
}
