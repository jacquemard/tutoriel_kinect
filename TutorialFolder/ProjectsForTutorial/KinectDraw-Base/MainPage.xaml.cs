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
            //###############################################################################
            // TODO: INITIALISER KINECT 
            //###############################################################################

            this.drawCanvas.Children.Add(new Line());
        }



        //########################################################### EVENEMENTS ###########################################################

       


    } //FIN DE LA CLASSE MAINPAGE
}//FIN DU NAMESPACE
