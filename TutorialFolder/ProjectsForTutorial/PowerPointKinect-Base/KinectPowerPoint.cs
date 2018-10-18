/// ETML
/// Summary : PowerPoint Add-in which enable kinect slide change
/// Date : 28.05.2015
/// Author : Rémi Jacquemard (jacquemare)

using Microsoft.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace PowerPointKinect
{
    public partial class KinectPowerPoint
    {
        //-----Reconnaissance des corps-----------------------------------------
        /// <summary>
        /// Le reader qui permet de récupérer les joueurs présents et détectés par Kinect
        /// </summary>
        private BodyFrameReader bodyReader { get; set; }
        /// <summary>
        /// La liste de tout les body détectés
        /// </summary>
        private IList<Body> bodies;
        /// <summary>
        /// La liste de tout les détecteurs de mouvements. Il y en a 6, car kinect peut détecter jusqu'à 6 personnes
        /// </summary>
        private IList<GestureDetector> bodiesDetectors;

        /// <summary>
        /// Chemin de l'addin
        /// </summary>
        private string originalPath;

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
        }


        //##################################################################################################### EVENEMENTS KINECT ####################

        
        

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
            //Le capteur Kinect actuellement raccordée au PC
            KinectSensor kinectSensor = KinectSensor.GetDefault();

            //Commence la lecture des mouvements-------------------------------  

            //******************************************************************* A COMPLETER *******************************************************//
            //********************************************************** récupérer les frames de body ***********************************************//
            
            //Tableau contenant les corps détectés
            this.bodies = new Body[kinectSensor.BodyFrameSource.BodyCount];
            //Tableau contenant les détecteurs de mouvements associés
            this.bodiesDetectors = new GestureDetector[this.bodies.Count];

            //Crée les détecteurs de mouvement correspondants

            //******************************************************************* A COMPLETER *******************************************************//
            //******************************************** création d'un GestureDetector par personne détectable ***********************************//
            //******************************************** gérer les événements du GestureDetector précédemment créé ***********************************//

            //----------------

            //On commence à recevoir des frames
            kinectSensor.Open();
           
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
