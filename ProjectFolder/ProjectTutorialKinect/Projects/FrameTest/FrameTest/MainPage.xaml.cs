using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using WindowsPreview.Kinect;

// Pour en savoir plus sur le modèle d'élément Page vierge, consultez la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace FrameTest
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        //Kinect-----------------------
        IList<Body> bodies;
        KinectSensor kinect;
        MultiSourceFrameReader reader = null;
        //-----------------------------

        private WriteableBitmap bitmap = null;

        public MainPage()
        {
            
            


            this.InitializeKinect();

            this.InitializeComponent();

            this.kinectImage.Source = this.bitmap;


            //this.kinectImage.Source = this.bitmap;

            //if (this.kinectImage.Source == null)
            //  this.kinectImage.Source = b;

        }

        private void InitializeKinect()
        {
            this.kinect = KinectSensor.GetDefault();

            this.reader = kinect.OpenMultiSourceFrameReader(FrameSourceTypes.BodyIndex | FrameSourceTypes.Color | FrameSourceTypes.Body);
            this.reader.MultiSourceFrameArrived += reader_MultiSourceFrameArrived;

            //Décris le format de l'image
            FrameDescription frameDescription = this.kinect.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            this.bitmap = new WriteableBitmap(frameDescription.Width, frameDescription.Height);


            kinect.Open();

        }


        void reader_MultiSourceFrameArrived(MultiSourceFrameReader sender, MultiSourceFrameArrivedEventArgs args)
        {
            using(MultiSourceFrame frame = args.FrameReference.AcquireFrame())
            {

                if(frame != null)
                {
                    //Récupération des images
                    using (BodyFrame bodyFrame = frame.BodyFrameReference.AcquireFrame())                    //################# BODYFRAME                    
                    using (BodyIndexFrame bodyIndexFrame = frame.BodyIndexFrameReference.AcquireFrame())    //################# BODY INDEX                        
                    using (ColorFrame colorFrame = frame.ColorFrameReference.AcquireFrame())                 //################# COLOR
                    { 

                        if (bodyIndexFrame != null && bodyFrame != null && colorFrame != null)
                        {

                            //Initilisation et mise à jour du tableau de body
                            if (this.bodies == null)
                                    this.bodies = new Body[bodyFrame.BodyCount];
                            bodyFrame.GetAndRefreshBodyData(this.bodies);


                            //WriteableBitmap bitmap = (WriteableBitmap)this.kinectImage.Source;

                            //Tableau contenant les données bodyIndex
                            byte[] bodyIndexArray = new byte[bodyIndexFrame.FrameDescription.LengthInPixels*bodyIndexFrame.FrameDescription.BytesPerPixel]; 
                            bodyIndexFrame.CopyFrameDataToArray(bodyIndexArray);


                            colorFrame.CopyConvertedFrameDataToBuffer(bitmap.PixelBuffer, ColorImageFormat.Bgra);

                            
                            foreach(Body b in this.bodies)
                            {
                                ColorSpacePoint[] colorPoint = new ColorSpacePoint[1];
                                kinect.CoordinateMapper.MapCameraPointsToColorSpace(new CameraSpacePoint[]{b.Joints[JointType.HandLeft].Position},colorPoint);

                                try
                                {
                                    Canvas.SetLeft(ellipse, colorPoint[0].X);
                                    Canvas.SetTop(ellipse, colorPoint[0].Y);
                                }catch(Exception e)
                                {

                                }
                            }
        
                        }
                       
                    }

                }
            }
        }

        






    }
}
