using Microsoft.Kinect.Toolkit.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using WindowsPreview.Kinect;
using WindowsPreview.Kinect.Input;
using Windows.System.Threading;
using Windows.UI.Core;

// Pour en savoir plus sur le modèle d'élément Page vierge, consultez la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace PointTest2_NoRegion
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

            
        private InputPointerManager pointerManager = new InputPointerManager();

        //Lien vers l'application actuelle
        private App app = (App)Application.Current;

        //Ancien point à dessiner
        private Point? lastPoint = null;

        //Les personnes vues
        private IList<Body> bodies;

        public MainPage()
        {
            this.InitializeComponent();

            this.Loaded += MainPage_Loaded;
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            //Kinect Handling for drawing - auto using of default kinect sensor
            KinectCoreWindow kinectCore = KinectCoreWindow.GetForCurrentThread();
            kinectCore.PointerMoved += kinectCore_PointerMoved;
            
            //Capture les corps pour les états des mains
            KinectSensor sensor = KinectSensor.GetDefault();
            

            BodyFrameReader bodyReader = sensor.BodyFrameSource.OpenReader();            
            bodyReader.FrameArrived += bodyReader_FrameArrived;
            

            ///Timer qui permet de faire tourner bodyReader en continu. Workaround
            DispatcherTimer t = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(33) };
            t.Tick += (senderTick, args) =>
            {
                bodyReader.IsPaused = false;
                Debug.WriteLine(DateTime.Now + " - Setting Paused false --------------------------------------------------");
                
            };
            t.Start();
            
        }


        /// <summary>
        /// Capture des corps
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void bodyReader_FrameArrived(BodyFrameReader sender, BodyFrameArrivedEventArgs args)
        {
            CoreWindow c = Window.Current.CoreWindow;

            Debug.WriteLine(DateTime.Now + " IS Body FRame NUll ");
            if(args.FrameReference != null)
            {
                using(BodyFrame bodyFrame = args.FrameReference.AcquireFrame())
                {
                    if(bodyFrame != null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];

                        bodyFrame.GetAndRefreshBodyData(this.bodies);


                        foreach (Body body in this.bodies)
                        {
                            if (body.IsTracked)
                            {
                                if (app.kinectRegion.KinectEngagementManager != null && app.kinectRegion.KinectEngagementManager.KinectManualEngagedHands != null)
                                    Debug.WriteLine(app.kinectRegion.KinectEngagementManager.KinectManualEngagedHands);
                                else
                                    Debug.WriteLine("IS NULL --- KINECT REGION");

                                if (body.HandRightState == HandState.Open)
                                {
                                    btnColor.Background = new SolidColorBrush(Colors.Red);
                                }
                                else if (body.HandRightState == HandState.Closed)
                                {
                                    btnColor.Background = new SolidColorBrush(Colors.Blue);
                                }
                            }
                        }


                    }
                }
            }

        }




        /// <summary>
        /// Lorsque une main/pointeur bouge sur l'écran
        /// </summary>
        /// <param name="sender">L'appelant</param>
        /// <param name="args">Les arguments</param>
        void kinectCore_PointerMoved(KinectCoreWindow sender, KinectPointerEventArgs args)
        {
            
            //Le point kinect et le point déduit sur l'interface
            KinectPointerPoint currentPointerPoint = args.CurrentPoint;
            Point currentPixelPoint = new Point()
            {
                X = (int)(currentPointerPoint.Position.X * app.kinectRegion.ActualWidth),
                Y = (int)(currentPointerPoint.Position.Y * app.kinectRegion.ActualHeight)
            };


            if (currentPointerPoint.Properties.IsEngaged) //Vérifie le point actuellement utilisé
            {
                //Dans le cas ou le point est dans le canvas, on peut commencer à dessiner
                if(isPointInCanvas(currentPixelPoint) )
                {
                    //Lorsqu'on est dans le canvas, on cache la main pour pouvoir dessiner
                    // TODO

                    #region dessin


                    //Diamètre du cercle changeant avec la pression
                    double diameter = 30;
                    diameter = diameter * Math.Pow(currentPointerPoint.Properties.HandReachExtent + 0.5, 3);
                
                    //Les deux possiion à dessiner
                    double posX = currentPixelPoint.X - getPointDrawCanvasPosition().X - 15;
                    double posY = currentPixelPoint.Y - getPointDrawCanvasPosition().Y - 15;

                    if (currentPointerPoint.Properties.HandReachExtent > 0.5) // Plus que 50 cm de l'épaule
                    {

                        //Add line
                        if (lastPoint != null)
                        {
                            Line line = new Line() { 
                                X1 = this.lastPoint.Value.X,
                                Y1 = this.lastPoint.Value.Y,
                                X2 = posX + 15,
                                Y2 = posY + 15,

                                StrokeThickness = diameter,
                                Stroke = btnColor.Background,
                                StrokeStartLineCap = PenLineCap.Round,
                                StrokeEndLineCap = PenLineCap.Round 
                            };

                            drawCanvas.Children.Add(line);
                        }

                        //Update the last position
                        this.lastPoint = new Point(posX + 15, posY + 15);

                    }
                    else
                    {
                        //Si on est moins que 0.5cm, on met le dernier point à null pour ne plus dessiner
                        lastPoint = null;
                    }

                    #endregion //----Dessin

                }
                else //Si on est hors du champ de dessin
                {
                    //On affiche le pointeur
                }

            }
            
            //Indique que le point a bien été géré
            args.Handled = true;
        }




        /// <summary>
        /// Return if the point in parameter is in the drawing canvas
        /// </summary>
        /// <param name="positionInPixel">The position Point in Pixel to check</param>
        /// <returns>True if the point is in the canvas, false otherwise </returns>
        private bool isPointInCanvas(Point positionInPixel)
        {
            //Récupère la position du Canvas en absolu
            Point pointDrawCanvasPosition = getPointDrawCanvasPosition();

            //Rectange décrivant le canvas -> Utile pour tester si le point est à l'intérieur
            Rect rectCanvas = new Rect(pointDrawCanvasPosition, new Size(drawCanvas.ActualWidth, drawCanvas.ActualHeight));

            //Retourne si le point est dans le canvas
            if (rectCanvas.Contains(positionInPixel))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Return a point that indicate where is the drawing canvas
        /// </summary>
        /// <returns>A point</returns>
        private Point getPointDrawCanvasPosition()
        {
            return drawCanvas.TransformToVisual(((App)Application.Current).kinectRegion).TransformPoint(new Point(0, 0));
        }

    }
}
