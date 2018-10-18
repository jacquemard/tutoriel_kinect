using Microsoft.Kinect;
using Microsoft.Kinect.Input;
using Microsoft.Kinect.VisualGestureBuilder;
using Microsoft.Office.Interop.PowerPoint;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Office = Microsoft.Office.Core;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;

namespace PowerPointKinect
{
    public partial class KinectPowerPoint
    {

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
        private const float FLT_POINTER_DISTANCE_ENGAGEMENT = 1f;
        private const float FLT_POINTER_DISTANCE_DISENGAGEMENT = 0.7f;
        private readonly object pointerLock = new object(); //Lock pour éviter des crashs sur le pointeur, car deux processus tournent en même temps (kinect et app)
        
        //Chemin de l'addin
        private string originalPath;


        /// <summary>
        /// Le slideshow en cours (mode présentateur, ou pas)
        /// </summary>
        private SlideShowWindow slideShow;
        //private TimeSpan lassoLastDetectedTimestamp;

        /// <summary>
        /// Méthode déclenchée au démarrage de l'addin
        /// </summary>
        /// <param name="sender">L'objet appelant</param>
        /// <param name="e">Les arguments de l'évenements</param>
        private void KinectPowerPoint_Startup(object sender, System.EventArgs e)
        {
            //On crée, copie, etc... les chemins et fichier du projet
            this.initProjectFiles();

            //On démarre tout ce qui est relatif à kinect
            this.initKinect(); 

            //Gestion des composants de la présentation
            this.Application.SlideShowBegin += Application_SlideShowBegin;
            this.Application.SlideShowEnd += Application_SlideShowEnd;
            this.Application.SlideShowNextSlide += Application_SlideShowNextSlide;
            
            this.ribbon = Globals.Ribbons.KinectRibbon; //On récupère le ruban associé à l'application
        }



        //##################################################################################################### EVENEMENT DU SLIDESHOW

        /// <summary>
        /// Méthode déclenchée lorsque le slideshow/diaporama passe à la slide suivante
        /// </summary>
        /// <param name="Wn">La page du slideshow, soit le diaporama en cours. C'est dans cette classe qu'on peut changer manuellement de slide, etc...</param>
        private void Application_SlideShowNextSlide(SlideShowWindow Wn)
        {
            if (this.pointer != null)
                this.deleteCurrentPointer();
            
            if (this.ribbon.chkPointer.Checked)
                this.createPointer(Wn.View.Slide);
        }
        
        /// <summary>
        /// Méthode déclenchée lorsque le slideshow commence (mode diaporama)
        /// </summary>
        /// <param name="Wn">La page du slideshow, soit le diaporama en cours. C'est dans cette classe qu'on peut changer manuellement de slide, etc...</param>
        private void Application_SlideShowBegin(PowerPoint.SlideShowWindow Wn)
        {
            if(this.pointer == null && this.ribbon.chkPointer.Checked)
            {
                this.createPointer(Wn.View.Slide);
            }
            

            this.slideShow = Wn;

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

        /// <summary>
        /// Méthode déclenchée lorsque le slideshow est quitté. 
        /// </summary>
        /// <param name="Pres">La "présentation", c'est à dire la classe contenant toute les slides, qu'on peut éditer, etc... </param>
        private void Application_SlideShowEnd(PowerPoint.Presentation Pres)
        {
            this.slideShow = null;

            //On efface le pointeur à la fin du slideshow
           
            if(this.pointer != null)
                this.deleteCurrentPointer();

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

        //#####################################################################################################-----------------------

        //##################################################################################################### EVENEMENTS KINECT
        /// <summary>
        /// Gère les mouvements détecté
        /// </summary>
        /// <param name="sender">L'objet appelant</param>
        /// <param name="e">Les arguments de l'évenements</param>
        private void handler_GestureFirstDetected(object sender, GestureDetector.GestureDetectedEventArgs e)
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
        /// <param name="sender">L'objet appelant</param>
        /// <param name="e">Les arguments de l'évenements</param>
        private void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (BodyFrame frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {

                    //On met à jour ce tableau
                    frame.GetAndRefreshBodyData(this.bodies);

                    //Compteur de personne suivie
                    int bodyTracked = 0;

                    //On explore les bodies
                    for (int i = 0; i < this.bodies.Count; i++)
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

                                #region Engagement "tendre le bras" avec les joints
                                /*                                                
                                if (KinectCoreWindow.KinectManualEngagedHands.Count == 0) //On vérifie que personne n'est engagé
                                {
                                    //Quand on montre du doigt, le doigt est a une certaine distance du coude. On peut l'utiliser pour activer le pointeur
                                    if(getDistanceBetween(body.Joints[JointType.HandTipRight].Position,body.Joints[JointType.ShoulderRight].Position) > 0.6)
                                    {
                                        KinectCoreWindow.SetKinectOnePersonManualEngagement(new BodyHandPair(body.TrackingId,HandType.RIGHT));
                                    }
                                    else if(getDistanceBetween(body.Joints[JointType.HandTipLeft].Position,body.Joints[JointType.ShoulderLeft].Position) > 0.6)
                                    {
                                        KinectCoreWindow.SetKinectOnePersonManualEngagement(new BodyHandPair(body.TrackingId, HandType.LEFT));
                                    }
                                    
                                }
                                else //Si une personne est engagée, on peut peut-être la désengager
                                {
                                    foreach (BodyHandPair pair in KinectCoreWindow.KinectManualEngagedHands)
                                    {
                                        if (pair.BodyTrackingId == body.TrackingId) // Fait le test uniquement sur la personne engagée
                                        {
                                            Debug.WriteLine("Has maybe to Disengage");
                                            
                                            //Quand on montre du doigt, le doigt est a une certaine distance du coude. On peut l'utiliser pour activer le pointeur
                                            if (pair.HandType == HandType.RIGHT &&
                                                getDistanceBetween(body.Joints[JointType.HandTipRight].Position, body.Joints[JointType.ShoulderRight].Position) < 0.5)
                                            {
                                                KinectCoreWindow.SetKinectOnePersonManualEngagement(null);
                                            }
                                            else if (pair.HandType == HandType.LEFT &&
                                                getDistanceBetween(body.Joints[JointType.HandTipLeft].Position, body.Joints[JointType.ShoulderLeft].Position) < 0.5)
                                            {
                                                KinectCoreWindow.SetKinectOnePersonManualEngagement(null);
                                            }
                                        }
                                    }

                                }*/
                                #endregion

                                #region pointeur avec lasso
                                /*if (KinectCoreWindow.KinectManualEngagedHands.Count == 0) //On vérifie que personne n'est engagé
                                {
                                    if (body.HandRightState == HandState.Lasso)
                                    {
                                        Debug.WriteLine("Lasso à droite");
                                        KinectCoreWindow.SetKinectOnePersonManualEngagement(new BodyHandPair(body.TrackingId, HandType.RIGHT));
                                    }
                                    else if (body.HandLeftState == HandState.Lasso)
                                    {
                                        Debug.WriteLine("Lasso à gauche");
                                        KinectCoreWindow.SetKinectOnePersonManualEngagement(new BodyHandPair(body.TrackingId, HandType.LEFT));
                                    }
                                }
                                else //Si une personne est engagée, on peut peut-être la désengager
                                {
                                    foreach(BodyHandPair pair in KinectCoreWindow.KinectManualEngagedHands)
                                    {
                                        if (pair.BodyTrackingId == body.TrackingId) // Fait le test uniquement sur la personne engagée
                                        {
                                            

                                            if( (pair.HandType == HandType.LEFT && body.HandLeftState != HandState.Lasso) || //Main gauche
                                                (pair.HandType == HandType.RIGHT && body.HandRightState != HandState.Lasso)) //Main droite
                                            {
                                                //Comme le lasso est mal détecté, on fait un timer pour désengager
                                                if ( this.lassoLastDetectedTimestamp + new TimeSpan(0, 0, 1) <= frame.RelativeTime ) //On laisse une seconde
                                                {
                                                    Debug.WriteLine("Rien");
                                                    KinectCoreWindow.SetKinectOnePersonManualEngagement(null);

                                                }
                                            }
                                            else
                                            {
                                                //On vérifie sa main, si elle est lasso, on reset le timestamp 
                                                this.lassoLastDetectedTimestamp = frame.RelativeTime;
                                            }

                                        }

                                    }

                                 
                                }*/
                                #endregion

                            }
                            else
                            {
                                this.bodiesDetectors[i].TrackingID = 0;
                            }
                        }
                    }

                    if (bodyTracked >= 2) //Lorsque un personne au moins est détecté, on l'affiche à l'utilisateur sur le ruban
                    {
                        ribbon.btnSwitch.Label = bodyTracked + " bodies detected";
                    }
                    else if (bodyTracked == 1) //On affiche un message indiquant qu'aucune personne n'est détectée
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

        /// <summary>
        /// Méthode appelée lorsque le pointeur kinect, c'est à dire la main, a bougé.
        /// </summary>
        /// <param name="sender">L'objet appelant</param>
        /// <param name="e">Les arguments de l'évenements</param>
        private void KinectPowerPoint_PointerMoved(object sender, KinectPointerEventArgs e)
        {
            KinectPointerPoint point = e.CurrentPoint;

            //On vérifie si on doit afficher le pointeur----         

            if (this.slideShow != null)
            {

                lock (this.pointerLock)
                {
                    if (this.pointer != null)
                    {
                        #region Engagement "tendre le bras" avec HandReachExtent
                        //*
                        if (KinectCoreWindow.KinectManualEngagedHands.Count == 0) //On vérifie que personne n'est engagé
                        {
                            //Quand on montre du doigt, le doigt est a une certaine distance du coude. On peut l'utiliser pour activer le pointeur
                            if (point.Properties.HandReachExtent > FLT_POINTER_DISTANCE_ENGAGEMENT)
                            {
                                this.pointer.Visible = Office.MsoTriState.msoTrue;
                                KinectCoreWindow.SetKinectOnePersonManualEngagement(new BodyHandPair(point.Properties.BodyTrackingId, point.Properties.HandType));
                            }
                        }
                        else //Si une personne est engagée, on peut peut-être la désengager
                        {
                            if (point.Properties.IsEngaged) //Le point courant doit être engagé pour désengager
                            {
                                if (point.Properties.HandReachExtent <= FLT_POINTER_DISTANCE_DISENGAGEMENT)
                                {
                                    KinectCoreWindow.SetKinectOnePersonManualEngagement(null);
                                    this.pointer.Visible = Office.MsoTriState.msoFalse;
                                }
                            }
                        }
                        //*/

                        //Change la position du curseur en fonction de la position des mains
                        if (KinectCoreWindow.KinectManualEngagedHands.Count > 0 && point.Properties.IsEngaged && this.pointer != null)
                        {
                            PointF position = point.Properties.UnclampedPosition;
                            RectF size = new RectF { X = 0, Y = 0, Width = this.Application.ActivePresentation.SlideMaster.Width, Height = this.Application.ActivePresentation.SlideMaster.Height };


                            //This.pointer == null -> Problème de thread ? :/
                            this.pointer.Left = Math.Min(Math.Max(position.X * size.Width, size.X), size.Width - this.pointer.Width);
                            this.pointer.Top = Math.Min(Math.Max(position.Y * size.Height, size.Y), size.Height - this.pointer.Height);
                            this.pointer.Visible = Office.MsoTriState.msoTrue;
                        }

                        #endregion

                    }

                }
                #region méthode d'engagement "Dans l'écran"
                //On définit qui a l'autorité sur le pointeur (quelle main est engagée)
                //Celui dont la main est dans l'écran est engagé
                /*if(KinectCoreWindow.KinectManualEngagedHands.Count == 0) //Personne n'est engagé
                {                
                    if(isInScreen(e.CurrentPoint.Properties.UnclampedPosition)) //Vérifie que le point est sur l'écran
                    {
                        KinectCoreWindow.SetKinectOnePersonManualEngagement(new BodyHandPair(e.CurrentPoint.Properties.BodyTrackingId, e.CurrentPoint.Properties.HandType));
                        Debug.WriteLine("Engagé");
                    }
                }
                else //Une personne est engagée
                {
                    //Vérifie si on doit désengager--

                    if(!isInScreen(e.CurrentPoint.Properties.UnclampedPosition) &&  // si le point est en dehors de l'écran et que
                        e.CurrentPoint.Properties.IsEngaged)                        // celui-ci est engagé
                    {
                        KinectCoreWindow.SetKinectOnePersonManualEngagement(null);
                        Debug.WriteLine("Désengagé");
                    }
                }*/

                #endregion

            }
        }

        //##################################################################################################### -------------------


        //##################################################################################################### METHODES PRIVEE RELATIVES A L'ADDIN

        /// <summary>
        /// Méthode qui permet d'initialiser les fichiers du projet. Initialisation spécial nécessaire pour que le projet fonctionne pour un addin Office
        /// </summary>
        private void initProjectFiles()
        {
            //Retourne l'assembly courante. L'assembly courante est un fichier dll quelque part sur le disque dur qui est actuellement en cours d'exécution dans PowerPoint
            Assembly assemblyInfo = Assembly.GetExecutingAssembly();

            //De cette assembly, on en retire le chemin d'installation, là où les fichiers originaux sont stoqués, et le chemin où le fichier est actuellement, c'est à dire dans un dossier de cache de PowerPoint
            //En effet, PowerPoint n'utilise pas directement les fichiers originaux, mais les copies dans un répertoire spécial avant de les utiliser
            //Source : https://robindotnet.wordpress.com/2010/07/11/how-do-i-programmatically-find-the-deployed-files-for-a-vsto-add-in/
            
            Uri uriCodeBase = new Uri(assemblyInfo.CodeBase);
            this.originalPath = Path.GetDirectoryName(uriCodeBase.LocalPath.ToString()); //On en retrouve le chemin original, qu'on garde en mémoire pour l'addin.


            //Copie du fichier dll nécessaire à la détection des mouvements au bon chemin. Celui-ci contient les algorithmes de détection d'une gesture de type Discrete, soit AdaBoostTech
            //PowerPoint copie chaque fichier dll dans des dossiers séparés dans le cache de PowerPoint. On retrouve donc le fichier dll (l'assembly) qui contient les méthodes, classes, etc... qui gère la détection de mouvement.
            //Ce fichier dll (Microsoft.Kinect.VisualGestureBuilder.DLL) doit avoir dans son même dossier un sous-dossier "vgbtechs", contenant le fichier AdaBoostTech.dll. 
            //Cette structure de fichier est demandée par l'assembly VisualGestureBuilder elle-même, et il n'est pas possible de modifier celle-ci.

            string pathToCopy = Path.GetDirectoryName(Assembly.GetAssembly(typeof(VisualGestureBuilderFrameSource)).Location); //On retrouve le chemin du dossier contenant la dll Microsoft.Kinect.VisualGestureBuilder.dll

            if (!File.Exists(pathToCopy + "\\vgbtechs\\AdaBoostTech.dll")) //Si le fichier existe déjà, on a pas besoin de le recopier.
            {   
                Directory.CreateDirectory(pathToCopy + "\\vgbtechs"); //On crée le dossier vgbtechs
                File.Copy(originalPath + "\\vgbtechs\\AdaBoostTech.dll", pathToCopy + "\\vgbtechs\\AdaBoostTech.dll"); //On copie le fichier AdaBoostTech.dll à partir de l'emplacement original à l'emplacement voulu
            } 
        }

        /// <summary>
        /// Méthode qui permet d'initialiser tout ce qui concerne kinect
        /// </summary>
        private void initKinect()
        {
            //La Kinect actuellement raccordée au PC
            KinectSensor kinectSensor = KinectSensor.GetDefault();           

            //Commence la lecture des mouvements-------------------------------  
            
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
                GestureDetector handler = new GestureDetector(this.originalPath+"\\SwitchKinect.gbd"); //On récupère le fichier contenant les mouvements à partir de l'emplacement original
                handler.GestureFirstDetected += handler_GestureFirstDetected;

                //Pour chaques corps détectable, on crée un détecteur
                this.bodiesDetectors[i] = handler;
            }
          
            //----------------

            //Détection de la position des mains
            KinectCoreWindow.GetForCurrentThread().PointerMoved += KinectPowerPoint_PointerMoved;
           
        }

        /// <summary>
        /// Retourne si le point passé en paramètre est dans l'écran
        /// </summary>
        /// <param name="point">Un point de la position de kinect (%) 0.0->1.0 </param>
        /// <returns>Vrai si le point passé en paramètre est dans l'écran, faux sinon</returns>
        private bool isInScreen(PointF point)
        {
            return point.X > 0 && point.X < 1 && point.Y > 0 && point.Y < 1;            
        }

        /// <summary>
        /// Méthode permetant de calculer la distance entre 2 points en 3D
        /// </summary>
        /// <param name="point1">Le premier point</param>
        /// <param name="point2">Le deuxième point</param>
        /// <returns>La distance en mètre</returns>
        private double getDistanceBetween(CameraSpacePoint point1, CameraSpacePoint point2)
        {
            //Les différences de distances des deux points
            double deltaX = point1.X - point2.X;
            double deltaY = point1.Y - point2.Y;
            double deltaZ = point1.Z - point2.Z;

            //Calcule avec pythagore
            return Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2) + Math.Pow(deltaZ, 2));
        }

        /// <summary>
        /// Crée un pointeur sur la slide donnée. Le pointeur ne doit pas être déjà crée. <see cref="deleteCurrentPointer()"/>
        /// </summary>
        /// <param name="slide"></param>
        private void createPointer(Slide slide)
        {
            lock (this.pointerLock)
            {
                if (this.pointer == null)
                {
                    this.pointer = slide.Shapes.AddShape(Office.MsoAutoShapeType.msoShapeOval, 0, 0, 13, 13);
                    this.pointer.Visible = Office.MsoTriState.msoFalse;
                    this.pointer.ShapeStyle = Office.MsoShapeStyleIndex.msoShapeStylePreset12;
                    this.pointer.Fill.Transparency = 0.2f;
                }
                else
                {
                    throw new Exception("The pointer is already set");
                }
            }
        }

        /// <summary>
        /// Supprime le pointeur actuel. Le supprime de la slide en cours, et la variable "global" du pointeur est mise à null
        /// </summary>
        private void deleteCurrentPointer()
        {
            lock (this.pointerLock)
            {
                if (this.pointer != null)
                {
                    this.pointer.Delete();
                    this.pointer = null;
                }
                else
                {
                    throw new NullReferenceException("The current pointer is not set");
                }
            }

        }

        //#####################################################################################################----------------------

        
        /// <summary>
        /// Déclenché lorsque l'addin se ferme
        /// </summary>
        /// <param name="sender">L'objet appelant</param>
        /// <param name="e">Les arguments de l'évenements</param>
        private void KinectPowerPoint_Shutdown(object sender, System.EventArgs e)
        {
            //On ferme le sensor Kinect
            KinectSensor.GetDefault().Close();
        }

        #region Code généré par VSTO

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(KinectPowerPoint_Startup);
            this.Shutdown += new System.EventHandler(KinectPowerPoint_Shutdown);
        }
        
        #endregion
    }
}
