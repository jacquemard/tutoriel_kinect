/// ETML
/// Summary : Class used to detect gestures from a gesture database file
/// Date : 28.05.2015
/// Author : Rémi Jacquemard (jacquemare)

using Microsoft.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;
using System;
using System.Diagnostics;

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
    class GestureDetector:IDisposable
    {
        //Reconnaissance des mouvements
        private VisualGestureBuilderFrameSource gestureSource { get; set; }
        private VisualGestureBuilderFrameReader gestureReader { get; set; }
        //Base de donnée des mouvements à détecter
        private VisualGestureBuilderDatabase gestures;

        /// <summary>
        /// ID du corps détecté
        /// </summary>
        public ulong TrackingID
        {
            get { return this.gestureSource.TrackingId; }
            set { this.gestureSource.TrackingId = value; }
        }

        /// <summary>
        /// Constructeur de la classe GestureDetector
        /// </summary>
        /// <param name="fileName">Le chemin du fichier de base de données (.gbd) dans lequel se trouve les mouvements à détecter</param>
        public GestureDetector(string fileName)
        {
            //Créé la base de donnée en fonction du chemin de fichier
            gestures = new VisualGestureBuilderDatabase(fileName);

            //Initialise la détection de geste
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
            using (VisualGestureBuilderFrame frame = e.FrameReference.AcquireFrame()) //Récupère la frame 
            {
                if(frame != null) //Vérifie que celle-ci n'est pas nul. 
                {
                    if (frame.DiscreteGestureResults != null) //Vérifie qu'il y ai des résultats de type discret (booléen)
                    {
                        foreach (var discreteGesturePair in frame.DiscreteGestureResults) //Explore toute les gestures détectée
                        {
                            //L'arguments de l'évenement à envoyer
                            GestureDetectedEventArgs gestureArgs = new GestureDetectedEventArgs(discreteGesturePair.Key, discreteGesturePair.Value);

                            //Vérifie si l'image actuel correspond à un geste dans la base de donnée et envoie l'événement correspondant
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


        //####################################################################### EVENEMENTS ############################################################
        /// <summary>
        /// Déclenché lorsqu'un geste a été détecté pour la première fois
        /// </summary>
        public event GestureDetectedEventHandler GestureFirstDetected;

        /// <summary>
        /// Déclenché lorsqu'un geste est détecté 
        /// </summary>
        public event GestureDetectedEventHandler GestureDetected;
        public delegate void GestureDetectedEventHandler(object sender, GestureDetectedEventArgs args);

        /// <summary>
        /// Arguments de l'évenement de détection de mouvement. 
        /// </summary>
        public class GestureDetectedEventArgs : EventArgs
        {
            /// <summary>
            /// Le geste détecté
            /// </summary>
            public Gesture Gesture { get; private set; }

            /// <summary>
            /// Le résultat de la détection (indique par exemple quel est la marge d'erreur  
            /// </summary>
            public DiscreteGestureResult GestureResult { get; private set; }

            /// <summary>
            /// Constructeur de GestureDetectedEventArgs
            /// </summary>
            /// <param name="gestureDetected">Le mouvement qui a été détecté</param>
            /// <param name="gestureResult">Le résultat de cette détection</param>
            public GestureDetectedEventArgs(Gesture gestureDetected, DiscreteGestureResult gestureResult)
            {
                this.Gesture = gestureDetected;
                this.GestureResult = gestureResult;
            }
        }

        /// <summary>
        /// Effectue toute les opérations de fermeture
        /// </summary>
        public void Dispose()
        {
            this.gestureSource.Dispose();
            this.gestures.Dispose();
        }
    }
}
