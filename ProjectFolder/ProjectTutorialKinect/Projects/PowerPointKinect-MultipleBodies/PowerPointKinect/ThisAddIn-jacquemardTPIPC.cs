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

namespace PowerPointKinect
{
    public partial class ThisAddIn
    {

        //Reconnaissance des gestures
        public VisualGestureBuilderFrameSource gestureSource { get; private set; }
        public VisualGestureBuilderFrameReader gestureReader { get; private set; }

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            

        }

        private void initGesture()
        {
            KinectSensor kinectSensor = KinectSensor.GetDefault();
            this.gestureSource = new VisualGestureBuilderFrameSource(kinectSensor, 0);
            this.gestureReader = gestureSource.OpenReader();
            this.gestureReader.FrameArrived += gestureReader_FrameArrived;
            
        }

        void gestureReader_FrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
        {
            Debug.WriteLine("YO");
            using(VisualGestureBuilderFrame frame = e.FrameReference.AcquireFrame())
            {
                if(frame != null)
                {
                    foreach(var result in frame.DiscreteGestureResults)
                    {
                        
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
