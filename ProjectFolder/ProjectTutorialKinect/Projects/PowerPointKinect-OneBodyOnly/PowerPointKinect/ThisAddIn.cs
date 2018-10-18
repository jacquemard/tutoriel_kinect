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
        private IList<VisualGestureBuilderFrameReader> gestureReaders;
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

            //Création des détections pour chaque corps possible (->6 personne en même temps)
            for(int i = 0;i<this.bodies.Count;i++)
            {
                VisualGestureBuilderFrameReader gestureReaderForBody = this.gestureSource.OpenReader();
                gestureReaderForBody.FrameArrived += gestureReader_FrameArrived;
                this.gestureReaders.Add(gestureReaderForBody);
               
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
                    foreach (Body body in bodies)
                    {
                        if (body != null)
                        {
                            if (body.IsTracked)
                            {
                                //Si un corps est détecté, on le suit
                                this.gestureSource.TrackingId = body.TrackingId;
                            }
                        }
                    }


                }
            }
        }

       
        /// <summary>
        /// Méthode pour gérer les mouvements reçu. Appelée 30 fois par seconde.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void gestureReader_FrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
        {
            using(VisualGestureBuilderFrame frame = e.FrameReference.AcquireFrame())
            {
                if(frame != null && frame.DiscreteGestureResults != null)
                {
                    foreach(var result in frame.DiscreteGestureResults)
                    {
                        if (result.Key.Name == "GoRight2")
                        {                            
                            if(result.Value.Detected && lastDetected == false && this.slideShowEnabled)
                            {
                                //On lance la prochaine slide
                                this.Application.ActivePresentation.SlideShowWindow.View.Next();
                                lastDetected = true;
                            }
                            else if(lastDetected == true && result.Value.Detected == false)
                            {
                                lastDetected = false;
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
