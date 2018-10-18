using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Office = Microsoft.Office.Core;
using Microsoft.Kinect;
using Microsoft.Kinect.Input;
using Microsoft.Kinect.VisualGestureBuilder;
using System.Diagnostics;
using Microsoft.Office.Tools;

namespace PowerPointKinect
{
    public partial class ThisAddIn
    {

        //Reconnaissance des mouvements
        private VisualGestureBuilderFrameSource gestureSource { get; set; }
        private VisualGestureBuilderFrameReader gestureReader { get; set; }
        //Base de donnée des mouvements à détecter
        private VisualGestureBuilderDatabase gestures { get; set; }

        //Reconnaissance des corps
        private BodyFrameReader bodyReader { get; set; }
        private IList<Body> bodies;
        private IList<GestureDetector> bodiesDetectors;

        /// <summary>
        /// Booléen indiquant si on a détecté précédement le geste
        /// </summary>
        private bool lastDetected = false;

        /// <summary>
        /// Booléen indiquant si on est en mode présentation
        /// </summary>
        private bool slideShowEnabled = false;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            //On démarre tout ce qui est relatif à kinect
            this.initGesture();

            //Gestion des composants de la présentation
            this.Application.SlideShowBegin += Application_SlideShowBegin;
            this.Application.SlideShowEnd += Application_SlideShowEnd;

        }

        void Application_SlideShowEnd(PowerPoint.Presentation Pres)
        {
            this.slideShowEnabled = false;
        }

        void Application_SlideShowBegin(PowerPoint.SlideShowWindow Wn)
        {
            this.slideShowEnabled = true;
        }

        private void initGesture()
        {
            KinectSensor kinectSensor = KinectSensor.GetDefault();
            this.gestureSource = new VisualGestureBuilderFrameSource(kinectSensor, 0);

            //Création de la base de donnée des mouvements et ajout des mouvements à la source des mouvements
            this.gestures = new VisualGestureBuilderDatabase("SwitchKinect.gbd");
            this.gestureSource.AddGestures(gestures.AvailableGestures);

            //TODO déploiement automatique du dll AdaBoostTech.dll (détection de mouvement)

            //Commence la lecture des mouvements

            //Création de la détection des bodies
            this.bodyReader = kinectSensor.BodyFrameSource.OpenReader();
            this.bodyReader.FrameArrived += BodyReader_FrameArrived;

            //Tableau contenant les corps détectés
            this.bodies = new Body[bodyReader.BodyFrameSource.BodyCount];
            //Tableau contenant les détecteurs de mouvements associés
            this.bodiesDetectors = new GestureDetector[this.bodies.Count];

            //Crée les détecteurs de mouvement correspondants
            for (int i = 0; i < bodies.Count; i++)
            {
                GestureDetector handler = new GestureDetector("SwitchKinect.gbd");
                handler.GestureFirstDetected += handler_GestureFirstDetected;

                //Pour chaques corps détectable, on crée un détecteur
                this.bodiesDetectors[i] = handler;
            }
            
           
        }

        void handler_GestureFirstDetected(object o, GestureDetector.GestureDetectedEventArgs e)
        {
            if (e.Gesture.Name == "GoRight2")
            {
                if (this.slideShowEnabled)
                {
                    //On lance la prochaine slide
                    this.Application.ActivePresentation.SlideShowWindow.View.Next();
                    lastDetected = true;
                }
            }
        }

        private void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (BodyFrame frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {

                    //On met à jour ce tableau
                    frame.GetAndRefreshBodyData(this.bodies);

                    //On explore les bodies
                    for (int i = 0; i < this.bodies.Count;i++ )
                    {
                        Body body = this.bodies[i];
                        if (body != null)
                        {
                            if (body.IsTracked)
                            {
                                //Si un corps est détecté, on écoute ses mouvements
                                this.bodiesDetectors[i].TrackingID = body.TrackingId;
                            }
                        }
                    }


                }
            }
        }

       
       

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }

        #region Code généré par VSTO

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
