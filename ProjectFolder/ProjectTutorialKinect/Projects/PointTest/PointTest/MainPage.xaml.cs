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
using Quobject.SocketIoClientDotNet.Client;
using Newtonsoft.Json.Linq;
using Microsoft.Kinect.Xaml.Controls;
using Microsoft.Kinect.Toolkit.Input;

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

        // SocketIO
        Socket socketIO;
        

        public MainPage()
        {
            this.InitializeComponent();

            this.DataContext = this;


            //Quand la page est chargée
            this.Loaded += MainPage_Loaded;
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            drawBrush = blueBrush;

            //Création de kinect
            this.kinect = KinectSensor.GetDefault();

            this.kinectCore = KinectCoreWindow.GetForCurrentThread();

            kinectCore.PointerEntered += kinectCore_PointerEntered;
            kinectCore.PointerMoved += kinectCore_PointerMoved;
            
            this.kinect.Open();
            


            //Bouton de couleur
            this.btnColorChoice.Background = drawBrush;
            this.btnColorChoice.Width = this.btnColorChoice.ActualHeight;

            byte[] bytColor = BitConverter.GetBytes(0xFFFFEE);
            Debug.WriteLine(bytColor[2]);
            

            //SocketIO ---- TEST pour joé
            #region connectJoe
            socketIO = IO.Socket("http://127.0.0.1:1337");

            socketIO.On(Socket.EVENT_CONNECT, () =>
            {
                Debug.WriteLine("Connected to NodeJS Server");

                socketIO.On("srvCanvasClick", (data) =>
                {
                    Debug.WriteLine(data);
                });
            });
            #endregion

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
                        CursorMove.Children.Insert(0, line);
                    }

                    //Update the last position
                    this.lastPosition = new Point(posX + 15, posY + 15);
                    
                    
                    //Add ellipse --> DRAWING Lines is better
                    //Ellipse form = new Ellipse() { Height = diameter, Width = diameter, Margin = new Thickness(posX + 15 - diameter/2, posY+15-diameter/2, 0, 0), Fill = new SolidColorBrush(Colors.Red), Opacity=1f };
                    //CursorMove.Children.Insert(0,form);


                    //--- TEST --- Connect with Joé
                    #region connectJoe
                    JObject position = new JObject();
                    position["x"] = (int)Math.Round(point.Position.X * 900,0);
                    position["y"] = (int)Math.Round(point.Position.Y * 600,0);
                    position["color"] = "#00A8E2";

                    socketIO.Emit("usrCanvasClick", position);
                    #endregion
                    //<----- FIN TEST
                }
                else
                {
                    lastPosition = null;
                }
                
            }           
 
            //Changement de couleur avec la main gauche --> N'est pas une bonne façon de faire
            /*
            if(point.Properties.HandType == HandType.LEFT)
            {
                //Modifie la couleur qui sera mise                
                //Pour avoir toute les couleurs, transformation d'un entier en une couleur
                int intColor = (int)(0xFFFFFF * point.Position.Y); //Toute la gamme de couleur
                byte[] bytColor = BitConverter.GetBytes(intColor); //Retourne un tableau de byte correspondant

                Debug.WriteLine(intColor);

                Color colToPaint = new Color();
                colToPaint.A = 255;
                colToPaint.B = (byte)(intColor>>16 & 0xFF);
                colToPaint.G = (byte)(intColor>>8 & 0xFF);
                colToPaint.R = (byte)(intColor & 0xFF);
                
                this.btnColorChoice.Background = new SolidColorBrush(colToPaint);

                if (point.Properties.HandReachExtent > 0.5)
                {
                    this.drawBrush = this.btnColorChoice.Background;
                    storyChangedColor.Begin();
                }
            }*/

            //Indique qu'on a traité le point
            args.Handled = true;
        }


        public event PropertyChangedEventHandler PropertyChanged;
    }
}
