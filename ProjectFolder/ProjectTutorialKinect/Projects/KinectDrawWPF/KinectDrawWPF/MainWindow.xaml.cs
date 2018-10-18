using KinectDrawWPF.Helper;
using Microsoft.Kinect;
using Microsoft.Kinect.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KinectDrawWPF
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Classe contenant des informations du curseur. Implémente INotifyPropertyChanged pour que l'interface graphique soit notifiée lorsqu'un évenement change
        /// </summary>
        private class CursorInfo : INotifyPropertyChanged
        {
            //Positions du curseau. Original: 500
            private double _posX = 500;
            public double PosX
            {
                get { return _posX; }
                set
                {
                    _posX = value;
                    NotifyPropertyChanged(); //Le nom de l'attribut est passé automatiquement
                }
            }

            private double _posY = 500;
            public double PosY
            {
                get { return _posY; }
                set
                {
                    _posY = value;
                    NotifyPropertyChanged();
                }
            }
            //Taille du curseur. Taille minimal: 30px
            private double _diameter = 30;
            public double Diameter
            {
                get { return _diameter; }
                set
                {
                    if (value >= 30)
                        _diameter = value;
                    else
                        _diameter = 30;

                    NotifyPropertyChanged();
                }
            }

            //-------------------

            //Notification aux composants xaml --------
            private void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
            public event PropertyChangedEventHandler PropertyChanged;
        }
        private CursorInfo cursorInfo = new CursorInfo();
        
        //Kinect---------------
        /// <summary>
        /// Interface de kinect qui permet de gérer l'interaction utilisateur-machine
        /// </summary>
        private KinectCoreWindow kinectCore; 

        /// <summary>
        /// La liste des personnes détectées par kinect
        /// </summary>
        private IList<Body> bodies;

        //--------------------------------

        /// <summary>
        /// Les couleurs pouvant être utilisées
        /// </summary>
        private List<Brush> colorBrushes = new List<Brush>();
         
        //Le pinceau utilisé actuellement
        private Brush _usedBrush;
        /// <summary>
        /// Le pinceau utilisé actuellement
        /// </summary>
        public Brush UsedBrush
        {
            get { return _usedBrush; }
            set
            {
                if (value != _usedBrush)
                {
                    _usedBrush = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("UsedBrush"));
                    }
                }
            }
        }

        //Booleen indiquant si le panel de choix de couleur est ouvert
        private bool isPanelOpen = false;

        /// <summary>
        /// Le dernier point dessiné. Celui-ci permet de tirer une ligne depuis le dernier point jusqu'au point actuel. Peut être nul. Il l'est, par exemple, lorsqu'on a pas commencé à dessiner
        /// </summary>
        private Point? lastPoint = null;

        //-----------------

        /// <summary>
        /// Constructeur de la fenêtre. On y appelle les diverses initialisation
        /// </summary>
        public MainWindow()
        {
            //Initialisation des composants, gérée par WPF.
            InitializeComponent();

            //Création de la liste des couleurs qui sera au final affichée dans le panel de sélection
            this.InitializeColorList();

            //Le contexte des données liées----
            this.DataContext = this;                                    //Contexte général
            this.cursorCanvas.DataContext = this.cursorInfo;            //Contexte pour le curseau. Utilise la classe CursorInfo créée
            this.colorChoicePanel.ItemsSource = this.colorBrushes;      //Liste des couleurs pour le panel de choix
            
            //Initialisation de tout ce qui est relatif à Kinect
            this.InitializeKinect();
            
        }

        /// <summary>
        /// Initialise la liste des couleurs
        /// </summary>
        private void InitializeColorList()
        {
            //Entier de granularité : indique le nombre de couleur qui sera introduite dans la grille à partir de toute la gamme possible
            int colorGranularity = 50;

            for(int i = 0;i < colorGranularity;i++) //On crée un certain nombre de couleur 
            {
                //Création d'une couleur à partir de la teinte:
                //  Teinte: 1.0 / colorGranularity * i : la teinte doit être entrée en % dans la classe ColorRGB (0->1). On crée des paliers qui vont de 0->1 en fonction de colorGranularity
                //  Saturation : 1 : Couleur vive
                //  Luminosité : 0.5 : Couleur moyenne entre noir et blanc (luminosité 0->noir, luminosité 1->blanc)
                ColorRGB colorRGB = ColorRGB.FromHSL(1.0 / colorGranularity * i, 1, 0.5); 
                SolidColorBrush brush = new SolidColorBrush(colorRGB); //Création du pinceau représentant une couleur
                this.colorBrushes.Add(brush); //Ajout de ce pinceau à la liste

            }

        }

        /// <summary>
        /// Initialise tout ce qui est relatif à Kinect
        /// </summary>
        private void InitializeKinect()
        {
            //Récupère le détecteur kinect et l'ouvre
            KinectSensor kinect = KinectSensor.GetDefault();
            kinect.Open();
            
            //Pointeur kinect
            this.kinectCore = KinectCoreWindow.GetForCurrentThread();
            kinectCore.PointerMoved += MainWindow_PointerMoved;

            //Détection des corps
            kinect.BodyFrameSource.OpenReader().FrameArrived += BodyReader_BodyFrameArrived;

        }

        /// <summary>
        /// Méthode déclenchée ~30 fois par seconde par Kinect. On y trouve les infos mise à jour relatives à la position des corps, etc...
        /// </summary>
        /// <param name="sender">L'objet appelant</param>
        /// <param name="e">Les paramètres d'une frame de body</param>
        private void BodyReader_BodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (BodyFrame frame = e.FrameReference.AcquireFrame()) //Récupère l'image 
            {
                if (frame != null) //L'image peut être nulle
                {
                    if (this.bodies == null)
                        this.bodies = new Body[frame.BodyCount]; //Création du tableau de bodies si celui-ci n'existe pas

                    frame.GetAndRefreshBodyData(this.bodies); //Met à jour ce tableau

                    

                    Body engagedBody = this.CurrentEngagedBody;
                    
                    if(engagedBody != null)
                    {
                        //Récupèration de la main du body engagée
                        HandType engagedHandType = KinectCoreWindow.KinectManualEngagedHands[0].HandType;

                        //Vérification de l'état de la main
                        if (engagedHandType == HandType.RIGHT) //La main droite est engagée
                        {
                            //On ouvre ou on ferme le panel en fonction de l'état de la main
                            if (engagedBody.HandRightState == HandState.Closed)
                                OpenColorChoice();
                            else if (engagedBody.HandRightState != HandState.Unknown) //L'état Unknown (inconnu) de la main est ignoré, car celui-ci peut arriver alors que la main est fermée
                                CloseColorChoice();
                        }
                        else if (engagedHandType == HandType.LEFT) //La main gauche est engagée
                        {
                            if (engagedBody.HandLeftState == HandState.Closed)
                                OpenColorChoice();
                            else if (engagedBody.HandLeftState != HandState.Unknown)
                                CloseColorChoice();
                        }
                    }

                  


                }//FIN - frame != null

            }//FIN - using frame
        }//FIN - BodyFrameArrived

        /// <summary>
        /// Récupère le body qui est actuellement engagé
        /// </summary>
        private Body CurrentEngagedBody
        {
            get
            {
                if(KinectCoreWindow.KinectManualEngagedHands.Count > 0) //Une personne est engagée
                {
                    //Récupère l'id de la personne engagée
                    ulong bodyTrackingId = KinectCoreWindow.KinectManualEngagedHands[0].BodyTrackingId;
                    Body engagedBody = this.bodies.Where(body => body.TrackingId == bodyTrackingId).First(); //Recherche dans la liste des bodies lequel a l'id de celui qui est engagé

                    return engagedBody;
                }
                else //Personne n'est engagé
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Ouvre le panel de choix de couleur
        /// </summary>
        private void OpenColorChoice()
        {
            if (!isPanelOpen) //Vérifie que le panel soit fermé
            {
                //Montre le panel et cache le curseur via les animations
                (this.Resources["PanelShow"] as Storyboard).Begin();
                (this.Resources["CursorHide"] as Storyboard).Begin();
                //Indique que le panel est ouvert
                isPanelOpen = true;          
            }
        }

        /// <summary>
        /// Ferme le panel de choix de couleur
        /// </summary>
        private void CloseColorChoice()
        {
            if (isPanelOpen) //Vérifie que le panel est bien ouvert
            {
                //Montre le panel et cache le curseur via les animations
                (this.Resources["PanelHide"] as Storyboard).Begin();
                (this.Resources["CursorShow"] as Storyboard).Begin();
                //Indique que le panel et fermé
                isPanelOpen = false;
            }
        }

        /// <summary>
        /// Méthode déclenchée ~30 fois par seconde par Kinect. On y trouve les infos mise à jour relatives à la position des mains/curseur kinect
        /// </summary>
        /// <param name="sender">L'objet appelant</param>
        /// <param name="e">Les paramètres d'une frame de body</param>
        private void MainWindow_PointerMoved(object sender, KinectPointerEventArgs e)
        {
            #region Méthode d'engagement "Dans l'écran"
            //On définit qui a l'autorité sur le pointeur (quelle main est engagée)
            //Celui dont la main est dans l'écran est engagé
            if (KinectCoreWindow.KinectManualEngagedHands.Count == 0) //Personne n'est engagé
            {
                if (IsInScreen(e.CurrentPoint.Properties.UnclampedPosition)) //Vérifie que le point est sur l'écran
                {
                    KinectCoreWindow.SetKinectOnePersonManualEngagement(new BodyHandPair(e.CurrentPoint.Properties.BodyTrackingId, e.CurrentPoint.Properties.HandType));
                    (this.Resources["CursorShow"] as Storyboard).Begin(); //On affiche le pointeur
                }
                else
                {
                    this.lastPoint = null;
                }
            }
            else //Une personne est engagée
            {
                //Vérifie si on doit désengager--

                if (!IsInScreen(e.CurrentPoint.Properties.UnclampedPosition,0.2f) &&    // si le point est en dehors de l'écran (avec une tolérance de 0.2, on peut sortir un peu de l'écran et dessiner) et que
                    e.CurrentPoint.Properties.IsEngaged)                                // celui-ci est engagé
                {
                    KinectCoreWindow.SetKinectOnePersonManualEngagement(null);
                    this.lastPoint = null;                    
                    (this.Resources["CursorHide"] as Storyboard).Begin(); //On cache le pointeur
                }
            }

            #endregion

            KinectPointerPoint kinectPoint = e.CurrentPoint;

            if(kinectPoint.Properties.IsEngaged) //On ne peut dessiner que si le point est engagé
            {
                
                //Vérification Dessin - Choix de couleur
                switch (isPanelOpen)
                {
                    case true: //On doit choisir la couleur
                        Canvas.SetLeft(colorChoicePanel, kinectPoint.Position.X * colorChoicePanel.ActualWidth - colorChoicePanel.ActualWidth);
                        this.UsedBrush = this.colorBrushes[this.colorBrushes.Count-1 - (int)Math.Floor(kinectPoint.Position.X * this.colorBrushes.Count)];
                        this.lastPoint = null;
                        break;
                    case false: //On doit dessiner

                        //Diamètre du cercle changeant avec la pression
                        double diameter = 30;
                        diameter = diameter * Math.Pow(kinectPoint.Properties.HandReachExtent + 0.5, 3);

                        //On modifie la position du curseur
                        double posX = Math.Max(e.CurrentPoint.Properties.UnclampedPosition.X, 0);
                        posX = Math.Min(posX, 1); // Position relative 0->1 compris
                        posX *= this.drawCanvas.ActualWidth;
                        posX -= diameter / 2;

                        double posY = Math.Max(e.CurrentPoint.Properties.UnclampedPosition.Y, 0);
                        posY = Math.Min(posY, 1); // Position relative 0->1 compris
                        posY *= this.drawCanvas.ActualHeight;
                        posY -= diameter / 2;


                        //On modifie le curseur
                        this.cursorInfo.PosX = posX;
                        this.cursorInfo.PosY = posY;

                        this.cursorInfo.Diameter = diameter;


                        #region Dessin


                        //Les deux possition à dessiner

                        if (kinectPoint.Properties.HandReachExtent > 0.5) // Plus que 50 cm de l'épaule
                        {

                            if (lastPoint != null) //On ne dessine une ligne que si on connait le dernier point
                            {
                                //Création d'une nouvelle ligne
                                Line line = new Line()
                                {
                                    //Premier point à partir du dernier point
                                    X1 = this.lastPoint.Value.X + diameter / 2,
                                    Y1 = this.lastPoint.Value.Y + diameter / 2,
                                    //Deuxième point à partir du point courant
                                    X2 = posX + diameter / 2,
                                    Y2 = posY + diameter / 2,

                                    //Design de la ligne
                                    StrokeThickness = diameter, //Largeur de la ligne
                                    Stroke = this.UsedBrush, //La couleur de la ligne
                                    StrokeStartLineCap = PenLineCap.Round, //Ligne dont les deux côtés sont rond
                                    StrokeEndLineCap = PenLineCap.Round

                                };

                                //Ajout de la ligne
                                drawCanvas.Children.Add(line);
                            }

                            //Update the last position
                            this.lastPoint = new Point(posX, posY);
                        }
                        else
                        {
                            //Si on est moins que 0.5cm, on met le dernier point à null pour ne plus dessiner
                            this.lastPoint = null;
                        }


                        #endregion //----Dessin

                        break;
                }
                
                
                

            }

          

        }

        /// <summary>
        /// Retourne si le point passé en paramètre est dans l'écran
        /// </summary>
        /// <param name="point">Un point de la position de kinect (%) 0.0->1.0 </param>
        /// <returns>Vrai si le point passé en paramètre est dans l'écran, faux sinon</returns>
        private bool IsInScreen(PointF point)
        {
            return IsInScreen(point, 0);
        }

        /// <summary>
        /// Retourne si le point passé en paramètre est dans l'écran
        /// </summary>
        /// <param name="point">Un point de la position de kinect (%) -x.x->+x.x </param>
        /// <param name="tolerance">Nombre indiquant la tolérance. Ex.: Le point est dans l'écran si x>0.0-tolerance </param>
        /// <returns>Vrai si le point passé en paramètre est dans l'écran, faux sinon</returns>
        private bool IsInScreen(PointF point, float tolerance)
        {
            return point.X > 0 - tolerance && point.X < 1 + tolerance && point.Y > 0 - tolerance && point.Y < 1 + tolerance;
        }

        /// <summary>
        /// Evenement déclenché lorsqu'une propriété à été modifiée
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
