using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Tools.Ribbon;
using System.Diagnostics;

namespace PowerPointAddInTest
{
    public partial class Ribbon1
    {


        private void Ribbon1_Load(object sender, RibbonUIEventArgs e)
        {
            Debug.WriteLine("Ribbon Loaded");
        }

        private void button1_Click(object sender, RibbonControlEventArgs e)
        {
            Globals.ThisAddIn.sensor.Open();
            
        }
    }
}
