/// ETML
/// Summary : MainPage used to draw with Kinect V2
/// Date : 20.05.2015
/// Author : Rémi Jacquemard (jacquemare)


using System;
using System.ComponentModel;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using WindowsPreview.Kinect;
using WindowsPreview.Kinect.Input;


namespace KinectDraw
{
    /// <summary>
    /// L'unique page de l'application
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //########################################################### INITIALISATION DES VARIABLES ###########################################################

        //---------------------- CURSEUR -------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Classe permettant de faire le lien entre la position du curseur et l'interface graphique. 
        /// </summary>
        private class CursorPosition: INotifyPropertyChanged
        {
            //Positions
            private double _posX = 960;
            /// <summary>
            /// La position selon l'axe des X du curseur, en pixel
            /// </summary>
            public double PosX
            { 
                get { return _posX; } 
                set 
                {
                    if(_posX != value)
                    {
                        _posX = value;
                        if(this.PropertyChanged != null)
                            this.PropertyChanged(this, new PropertyChangedEventArgs("PosX"));
                    }                    
                } 
            }

            private double _posY=540;
            /// <summary>
            /// La position selon l'axe des Y du curseur, en pixel
            /// </summary>
            public double PosY
            {
                get { return _posY; }
                set
                {
                    if(_posY != value)
                    {
                        _posY = value;
                        if(this.PropertyChanged != null)
                            this.PropertyChanged(this, new PropertyChangedEventArgs("PosY"));
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }         
        /// <summary>
        /// Instance faisant le lien entre la position du curseur et l'interface graphique  
        /// </summary>      
        private CursorPosition cursorPosition = new CursorPosition();

        /// <summary>
        /// Le diamètre du curseur, en pixel
        /// </summary>
        private const double CURSOR_DIAMETER = 30;


        //---------------------- KINECT -------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Représente un capteur Kinect, permettant de le gérer (commencer la lecture, etc...)
        /// </summary>
        private KinectSensor kinect;

        /// <summary>
        /// Interface de kinect qui permet de gérer l'interaction utilisateur-machine
        /// </summary>
        private KinectCoreWindow kinectCore;

        //---------------------- DESSIN -------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Point pouvant être nul indiquant la dernière position (où est le début de la ligne ? )
        /// </summary>
        private Point? lastPosition = null;
        /// <summary>
        /// Le pinceau qui est utilisé pour dessiner les lignes
        /// </summary>
        private Brush drawBrush = new SolidColorBrush(Colors.OrangeRed);
 

        //########################################################### CONSTRUCTEUR ET INITIALISATION ###########################################################

        /// <summary>
        /// Constructeur de la page
        /// </summary>
        public MainPage()
        {
            //Initialisation des composants
            this.InitializeComponent();                             
            this.cursorEllipse.DataContext = this.cursorPosition;
            //------------

            //Quand la page est chargée, on initialise Kinect
            this.Loaded += MainPage_Loaded;
        }

        /// <summary>
        /// Evénement déclenché lorsque la page a été chargée
        /// </summary>
        /// <param name="sender">L'objet déclenchant</param>
        /// <param name="e">Les arguments de l'événement</param>
        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            //Récupération des objets Kinect nécessaires au programme
            this.kinect = KinectSensor.GetDefault();
            this.kinectCore = KinectCoreWindow.GetForCurrentThread();
            
            //On traite les évévenements permettant de faire bouger le curseur
            this.kinectCore.PointerMoved += kinectCore_PointerMoved;

            //On ouvre Kinect (Kinect s'allume)
            this.kinect.Open();
        }



        //########################################################### EVENEMENTS ###########################################################

        /// <summary>
        /// Evénement déclenché lorsqu'un pointeur de Kinect bougé
        /// </summary>
        /// <remarks>Fonctionne pour 1 seul personne uniquement. Kinect ne doit en détecter qu'une seule et unique</remarks>
        /// <param name="sender">L'objet déclenchant</param>
        /// <param name="args">Les arguments de l'événement contenant, notamment, la position actuel du curseur</param>
        void kinectCore_PointerMoved(KinectCoreWindow sender, KinectPointerEventArgs args)
        {
            //Un point selong l'axe X-Y (en %)
            KinectPointerPoint point = args.CurrentPoint;


            //Cette condition permet de contourner certain problème qui apparaissent.
            //En effet, cet événement est appelé 2 fois par personne, une fois pour la main droite et une fois pour la main gauche
            //On a donc 2 points de curseur par personne
            //N'utiliser que la main droite permet d'éviter que les deux mains bougent le curseur
            if (point.Properties.HandType == HandType.RIGHT) //On n'utilise que la main droite
            {
                //Calcule du diametre de la ligne
                double diameter = CURSOR_DIAMETER;
                diameter *= Math.Pow(point.Properties.HandReachExtent + 0.5, 3);

                //Traduit les positions données par Kinect (en %) en position en pixel
                double currentPosX = point.Position.X * this.cursorMoveCanvas.ActualWidth;
                double currentPosY = point.Position.Y * this.cursorMoveCanvas.ActualHeight;

                //Calcule la position du curseur
                this.cursorPosition.PosX = currentPosX - CURSOR_DIAMETER / 2;
                this.cursorPosition.PosY = currentPosY - CURSOR_DIAMETER / 2;

                if(point.Properties.HandReachExtent > 0.5) //Lorsqu'on commence à appuyer, on dessine
                {
                    

                    if (this.lastPosition != null) //Si le dernier point appuyé existe, on peut dessiner. Sinon, on attends le prochain "tour"
                    {
                        //Ajout la ligne entre le dernier point et le point courant
                        Line lineToAdd = new Line() 
                        { 
                            //La position initiale de la ligne, tirée du dernier point
                            X1 = lastPosition.Value.X, 
                            Y1 = lastPosition.Value.Y, 
                            //La position finale de la ligne, tirée du point courant
                            X2 = currentPosX, 
                            Y2 = currentPosY, 

                            //Design de la ligne
                            StrokeThickness = diameter, //Le diamètre de la ligne a été calculé
                            Stroke = this.drawBrush, //La couleur stockée en local est utilisée
                            //La ligne commence et finis par un cercle.
                            StrokeStartLineCap = PenLineCap.Round, 
                            StrokeEndLineCap = PenLineCap.Round 
                        };
                        

                        //On ajoute la ligne dans le canvas de dessin
                        this.drawCanvas.Children.Add(lineToAdd); 
                    }

                    //Met à jour la dernière position du point. La ligne à dessiner se fera à partir de ce point
                    this.lastPosition = new Point(currentPosX, currentPosY);                    

                }
                else //Si on n'appuie pas/plus, on ne dessine pas
                {
                    this.lastPosition = null;
                }
                
            }//FIN - main droite
            
        }//FIN - kinectCore_PointerMoved




    } //FIN DE LA CLASSE MAINPAGE
}//FIN DU NAMESPACE
