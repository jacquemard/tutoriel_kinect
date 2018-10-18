using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Tools.Ribbon;
using Microsoft.Kinect;

namespace PowerPointKinect
{
    public partial class KinectRibbon
    {
        private KinectSensor kinectSensor;

        private void KinectRibbon_Load(object sender, RibbonUIEventArgs e)
        {
            this.kinectSensor = KinectSensor.GetDefault();
        }

        private void btnSwitch_Click(object sender, RibbonControlEventArgs e)
        {
            RibbonButton switchButton = (RibbonButton)sender;

            if (!kinectSensor.IsOpen)
            {
                kinectSensor.Open();
                switchButton.Label = "Stop";
                
            }
            else
            {
                kinectSensor.Close();
                switchButton.Label = "Start";
            }
        }



    }
}
