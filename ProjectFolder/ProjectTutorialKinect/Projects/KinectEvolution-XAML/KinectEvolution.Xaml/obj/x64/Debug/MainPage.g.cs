﻿

#pragma checksum "C:\Users\jacquemare\OneDrive\ETML - FPA 2013-2015\TPI\Projects\KinectEvolution-XAML\KinectEvolution.Xaml\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "096D8A53D4368F5F9C9CAC972DCBAA09"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace KinectEvolution
{
    partial class MainPage : global::Windows.UI.Xaml.Controls.Page, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 13 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.HelpButton_Click;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 16 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.DefaultView_Click;
                 #line default
                 #line hidden
                break;
            case 3:
                #line 17 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.ToggleCameraFullScreen_Click;
                 #line default
                 #line hidden
                break;
            case 4:
                #line 18 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.ToggleTechFullscreen_Click;
                 #line default
                 #line hidden
                break;
            case 5:
                #line 209 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Tapped += this.CameraPreview_Tapped;
                 #line default
                 #line hidden
                break;
            case 6:
                #line 211 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Tapped += this.TechPreview_Tapped;
                 #line default
                 #line hidden
                break;
            case 7:
                #line 219 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.Selector)(target)).SelectionChanged += this.OnSelectionChanged;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}


