using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Office = Microsoft.Office.Core;
using System.Diagnostics;
using Microsoft.Kinect;
using Microsoft.Office.Tools.Ribbon;
using System.Threading;
using Microsoft.Office.Tools;

namespace PowerPointAddInTest
{
    public partial class ThisAddIn
    {


        public KinectSensor sensor = KinectSensor.GetDefault();

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            PowerPoint.Application app = Globals.ThisAddIn.Application;
            
            app.SlideShowBegin += app_SlideShowBegin;

            //this.CustomTaskPanes.Add(controle, "Test");

            //CustomTaskPane customPane = this.CustomTaskPanes.Add(controle, "kinect");
            //customPane.Visible = true;

        }


        void app_SlideShowBegin(PowerPoint.SlideShowWindow Wn)
        {
            
            Thread.Sleep(5000);
            Wn.View.GotoSlide(2) ;

            Debug.WriteLine("On commence");
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
