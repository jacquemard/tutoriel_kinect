using Microsoft.Kinect.VisualGestureBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.IO;

namespace PowerPointKinect
{

    /// <summary>
    /// Permet de détecter les mouvements d'un corps
    /// Les mouvements sont détectés à partir d'une base de données
    /// Deux évenements sont disponibles:
    /// <list type="bullet">
    ///     <item>GestureFirstDetected</item>
    ///     <item>GestureDetected</item>
    /// </list>
    /// </summary>
    class GestureDetector
    {
        //Reconnaissance des mouvements
        private VisualGestureBuilderFrameSource gestureSource { get; set; }
        private VisualGestureBuilderFrameReader gestureReader { get; set; }
        //Base de donnée des mouvements à détecter
        private VisualGestureBuilderDatabase gestures;

        //Dernier mouvement détecté

        //ID du corps détecté
        public ulong TrackingID
        {
            get { return this.gestureSource.TrackingId; }
            set { this.gestureSource.TrackingId = value; }
        }

        public GestureDetector(string fileName)
        {
            gestures = new VisualGestureBuilderDatabase(fileName);

            initGesture();
        }

        /// <summary>
        /// Initialise le système de mouvement
        /// </summary>
        private void initGesture()
        {
            //Création de la source des images de mouvement (~30x/sec)
            this.gestureSource = new VisualGestureBuilderFrameSource(KinectSensor.GetDefault(), 0);
            //Ajout des mouvements à cette sources
            this.gestureSource.AddGestures(gestures.AvailableGestures);
            
            // Commence la lecture
            this.gestureReader = this.gestureSource.OpenReader();
            this.gestureReader.FrameArrived += gestureReader_FrameArrived;
        }


        /// <summary>
        /// Appelé 30 fois par seconde et gérant les frames de gestures
        /// </summary>
        private void gestureReader_FrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
        {
            using (VisualGestureBuilderFrame frame = e.FrameReference.AcquireFrame())
            {
                if(frame != null) 
                {
                    if (frame.DiscreteGestureResults != null) //Vérifie qu'il y ai des résultats
                    {
                        foreach (var discreteGesturePair in frame.DiscreteGestureResults)
                        {
                            //L'arguments de l'évenement à envoyer
                            GestureDetectedEventArgs gestureArgs = new GestureDetectedEventArgs(discreteGesturePair.Key, discreteGesturePair.Value);

                            //Vérifie si l'image actuel correspond à un geste dans la base de donnée et envoie l'évenement correspondant
                            if (discreteGesturePair.Value.FirstFrameDetected)
                                if(GestureFirstDetected != null)
                                    GestureFirstDetected(this, gestureArgs);
                            if (discreteGesturePair.Value.Detected)
                                if(GestureDetected != null)
                                    GestureDetected(this, gestureArgs);
                        }
                    }
                }
            }

        }


        //Evénements pouvant être suivis
        /// <summary>
        /// Déclenché lorsqu'un geste a été détecté pour la première fois
        /// </summary>
        public event GestureDetectedEventHandler GestureFirstDetected;

        /// <summary>
        /// Déclenché lorsqu'un geste est détecté 
        /// </summary>
        public event GestureDetectedEventHandler GestureDetected;
        public delegate void GestureDetectedEventHandler(object o, GestureDetectedEventArgs e);

        public class GestureDetectedEventArgs : EventArgs
        {
            public Gesture Gesture { get; private set; }
            public DiscreteGestureResult GestureResult { get; private set; }

            
            public GestureDetectedEventArgs(Gesture gestureDetected, DiscreteGestureResult gestureResult)
            {
                this.Gesture = gestureDetected;
                this.GestureResult = gestureResult;

            }
        }




    }
}
