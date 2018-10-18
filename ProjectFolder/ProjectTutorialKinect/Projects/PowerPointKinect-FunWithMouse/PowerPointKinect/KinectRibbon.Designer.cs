namespace PowerPointKinect
{
    partial class KinectRibbon : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public KinectRibbon()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.tab1 = this.Factory.CreateRibbonTab();
            this.grpKinect = this.Factory.CreateRibbonGroup();
            this.btnSwitch = this.Factory.CreateRibbonButton();
            this.chkTransition = this.Factory.CreateRibbonCheckBox();
            this.tab1.SuspendLayout();
            this.grpKinect.SuspendLayout();
            // 
            // tab1
            // 
            this.tab1.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.tab1.Groups.Add(this.grpKinect);
            this.tab1.Label = "TabAddIns";
            this.tab1.Name = "tab1";
            // 
            // grpKinect
            // 
            this.grpKinect.Items.Add(this.btnSwitch);
            this.grpKinect.Items.Add(this.chkTransition);
            this.grpKinect.Label = "Kinect";
            this.grpKinect.Name = "grpKinect";
            // 
            // btnSwitch
            // 
            this.btnSwitch.Image = global::PowerPointKinect.Properties.Resources.play;
            this.btnSwitch.Label = "Start";
            this.btnSwitch.Name = "btnSwitch";
            this.btnSwitch.ShowImage = true;
            this.btnSwitch.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnSwitch_Click);
            // 
            // chkTransition
            // 
            this.chkTransition.Label = "Override transitions";
            this.chkTransition.Name = "chkTransition";
            // 
            // KinectRibbon
            // 
            this.Name = "KinectRibbon";
            this.RibbonType = "Microsoft.PowerPoint.Presentation";
            this.Tabs.Add(this.tab1);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.KinectRibbon_Load);
            this.tab1.ResumeLayout(false);
            this.tab1.PerformLayout();
            this.grpKinect.ResumeLayout(false);
            this.grpKinect.PerformLayout();

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tab1;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup grpKinect;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnSwitch;
        internal Microsoft.Office.Tools.Ribbon.RibbonCheckBox chkTransition;
    }

    partial class ThisRibbonCollection
    {
        internal KinectRibbon KinectRibbon
        {
            get { return this.GetRibbon<KinectRibbon>(); }
        }
    }
}
