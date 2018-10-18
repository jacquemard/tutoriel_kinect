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
using Microsoft.Office.Interop.PowerPoint;
using System.Runtime.InteropServices;

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

        //Rubban
        private KinectRibbon ribbon;

        //Transition--------------
        private PpEntryEffect kinectEntryEffect = PpEntryEffect.ppEffectPushLeft; //Transition utilisée
        private float kinectTransitionDuration = 0.5f; //Temp utilisé
        //Tableau de sauvegarde des slides
        private SlideShowTransitionBackup[] transitionsBackup;
        class SlideShowTransitionBackup
        {
            public PpEntryEffect EntryEffect { get; set; }
            public float Duration { get; set; }
        }

        //Pointeur
        private Shape pointer;
        private PointF pointerPosition = new PointF();
        

        /// <summary>
        /// Le slideshow en cours (mode présentateur). Null si pas en mode présentateur
        /// </summary>
        private SlideShowWindow slideShow;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            //On démarre tout ce qui est relatif à kinect
            this.initGesture();

            //Gestion des composants de la présentation
            this.Application.SlideShowBegin += Application_SlideShowBegin;
            this.Application.SlideShowEnd += Application_SlideShowEnd;
            
            
            this.ribbon = Globals.Ribbons.KinectRibbon;
        }


        void Application_SlideShowBegin(PowerPoint.SlideShowWindow Wn)
        {
            this.slideShow = Wn;

            this.pointer = Wn.View.Slide.Shapes.AddShape(Office.MsoAutoShapeType.msoShapeOval, 0, 0, 13, 13);
            this.pointer.ShapeStyle = Office.MsoShapeStyleIndex.msoShapeStylePreset12;
            this.pointer.Fill.Transparency = 0.2f;

            Wn.View.PointerType = PpSlideShowPointerType.ppSlideShowPointerNone;

            //Si on souhaite que les transitions soient optimisé pour kinect via la checkbox du ruban
            if (this.ribbon.chkTransition.Checked == true)
            {

                //Sauvegarde les slides pour les modifications effectuée pour kinect. Tableau commencant à 1, le 0 est vide (Slide de 1 -> x)
                this.transitionsBackup = new SlideShowTransitionBackup[this.Application.ActivePresentation.Slides.Count + 1];

                for (int i = 1; i <= this.Application.ActivePresentation.Slides.Count; i++)
                {
                    Slide currentSlide = this.Application.ActivePresentation.Slides[i];

                    //Sauvegarde de la slide
                    this.transitionsBackup[i] = new SlideShowTransitionBackup
                    {
                        EntryEffect = currentSlide.SlideShowTransition.EntryEffect,
                        Duration = currentSlide.SlideShowTransition.Duration
                    };

                    //Modifie les transitions
                    currentSlide.SlideShowTransition.Duration = kinectTransitionDuration;
                    currentSlide.SlideShowTransition.EntryEffect = kinectEntryEffect;
                }

            }

        }

        void Application_SlideShowEnd(PowerPoint.Presentation Pres)
        {
            this.slideShow = null;

             //Si on souhaite que les transitions soient optimisé pour kinect via la checkbox du ruban
            if (this.ribbon.chkTransition.Checked == true)
            {
                //Récupère les transitions
                for (int i = 1; i <= this.Application.ActivePresentation.Slides.Count; i++)
                {
                    //Récupération de la slide
                    this.Application.ActivePresentation.Slides[i].SlideShowTransition.EntryEffect = transitionsBackup[i].EntryEffect;
                    this.Application.ActivePresentation.Slides[i].SlideShowTransition.Duration = transitionsBackup[i].Duration;
                }
            }

        }


        private void initGesture()
        {
            KinectSensor kinectSensor = KinectSensor.GetDefault();
            this.gestureSource = new VisualGestureBuilderFrameSource(kinectSensor, 0);


            //Commence la lecture des mouvements------------------------------

            //Création de la base de donnée des mouvements et ajout des mouvements à la source des mouvements
            this.gestures = new VisualGestureBuilderDatabase("SwitchKinect.gbd");
            this.gestureSource.AddGestures(gestures.AvailableGestures);

            //TODO déploiement automatique du dll AdaBoostTech.dll (détection de mouvement), utiliser DLLImport ?


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
          
            //----------------

            //Détection de la position des mains
            KinectCoreWindow.GetForCurrentThread().PointerMoved += ThisAddIn_PointerMoved;
            
           
        }

        void ThisAddIn_PointerMoved(object sender, KinectPointerEventArgs e)
        {
            //if (this.slideShow != null)
            {
                //Change la position du curseur en fonction de la position des mains
                if (e.CurrentPoint.Properties.HandType == HandType.RIGHT)
                {
                    Win32.SetCursorPos((int)(e.CurrentPoint.Position.X * 1920), (int)(e.CurrentPoint.Position.Y * 1080));

                    Debug.WriteLine(e.CurrentPoint.Properties.HandReachExtent);

                    if (e.CurrentPoint.Properties.HandReachExtent > 0.5f)
                    {
                        //TODO lancer le clique down + ctrl -> pointeur
                        Win32.sendDown();
                    }
                    else
                    {
                        //TODO lancer le clique up -> pointeur
                        Win32.sendUp();
                    }

                }

                //TODO erreur application occupée lors du clique droit sur le slide en cours
                //this.pointerPosition.X = e.CurrentPoint.Properties.UnclampedPosition.X * this.Application.ActivePresentation.SlideMaster.Width;
                //this.pointerPosition.Y = e.CurrentPoint.Properties.UnclampedPosition.Y * this.Application.ActivePresentation.SlideMaster.Height;
                //this.slideShow.View.PointerType = PpSlideShowPointerType.ppSlideShowPointerAutoArrow;
            }
        }


        /// <summary>
        /// Gère les mouvements détecté
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        void handler_GestureFirstDetected(object o, GestureDetector.GestureDetectedEventArgs e)
        {
            if (this.slideShow != null) //Vérifie qu'on est bien en mode présentateur
            {
                if (e.Gesture.Name == "OutIn_Right" || e.Gesture.Name == "InOut_Left") //Si on veut aller à droite (slide suivante) : geste de la main droite ou de la main gauche
                {
                    //On lance la prochaine slide
                    this.slideShow.View.Next();

                    
                }
                else if (e.Gesture.Name == "OutIn_Left" || e.Gesture.Name == "InOut_Right") //Si on veut aller à gauche (précédente slide)
                {
                    this.slideShow.View.Previous();
                }

            }

        }

        /// <summary>
        /// Déclenché lorsqu'une frame de corps arrive (30fps)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            if (this.pointer != null)
            {
                //this.pointer.IncrementLeft(8);
                //this.pointer.Left += 8;
                //this.pointer.Left = this.pointerPosition.X;
                //this.pointer.Top = this.pointerPosition.Y;
                //this.pointer.Left = 0;
                //this.pointer.Top = 0;

                Debug.WriteLine(this.pointerPosition.X);
            }

            using (BodyFrame frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    //On met à jour ce tableau
                    frame.GetAndRefreshBodyData(this.bodies);
            
                    //Compteur de personne suivie
                    int bodyTracked = 0;                    

                    //On explore les bodies
                    for (int i = 0; i < this.bodies.Count;i++ )
                    {
                        Body body = this.bodies[i];
                        if (body != null)
                        {
                            if (body.IsTracked)
                            {
                                //Si un corps est suivis, on écoute ses mouvements
                                this.bodiesDetectors[i].TrackingID = body.TrackingId;

                                //Incrémente le compteur de personne
                                bodyTracked += 1;


                            }
                        }
                    }

                    if(bodyTracked >= 2) //Lorsque un personne au moins est détecté, on l'affiche à l'utilisateur sur le ruban
                    {
                        ribbon.btnSwitch.Label = bodyTracked + " bodies detected";
                    }
                    else if(bodyTracked == 1) //On affiche un message indiquant qu'aucune personne n'est détectée
                    {
                        ribbon.btnSwitch.Label = bodyTracked + " body detected";
                    }
                    else
                    {
                        ribbon.btnSwitch.Label = "No body detected";
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
