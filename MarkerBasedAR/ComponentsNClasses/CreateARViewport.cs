using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Display;
using Rhino;
using Rhino.Geometry;
using MarkerBasedAR.Properties;

namespace MarkerBasedAR.ComponentsNClasses
{
    public class CreateARViewport : GH_Component
    {

        public CreateARViewport()
          : base("CreateARViewport", "Nickname",
              "Description",
              BasicInfo.Category, "Viewport")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Button", "B", "Create or not.", GH_ParamAccess.item);
            pManager.AddTextParameter("Viewport_Name", "V_Name", "The name of the viewport.", GH_ParamAccess.item);
            pManager.AddTextParameter("ARDisplayMode_Name", "D_Name", "The name of the Displaymode.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Width", "W", "The width of the viewport in pixels.(Default 1024)", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Height", "H", "The height of the viewport in pixels.(Default 768)", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Adjustment in Width", "A_W", "The adjustment in width in pixels.(Default 18)", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Adjustment in height", "A_H", "The adjustment in height in pixels.(Default 47)", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("AR_RhinoView", "RhinoView", "The RhinoView.", GH_ParamAccess.item);
            pManager.AddTextParameter("Viewport_names", "V_names", "The names of all the viewports.", GH_ParamAccess.list);
            pManager.AddTextParameter("DisplayMode_Names", "D_names", "The names of all the displaymodes.", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool b = false;
            string v_name = null;
            string d_name = null;
            int width = 1024, height = 768;
            int ad_width = 18, ad_height = 47;
            if (!DA.GetData(0, ref b) || !DA.GetData(1, ref v_name) || !DA.GetData(2, ref d_name) || !DA.GetData(3, ref width) || !DA.GetData(4, ref height) || !DA.GetData(5, ref ad_width) || !DA.GetData(6, ref ad_height))
                return;
            //Get the current RhinoDoc
            RhinoDoc doc01 = RhinoDoc.ActiveDoc;

            //Set up the variables
            DisplayModeDescription display_mode = null;
            RhinoView rv = null;
            RhinoView pick = null;

            //Add the Viewport for AR display
            if (b)
            {
                System.Drawing.Rectangle view01_Pos = new System.Drawing.Rectangle(0, 0, width + ad_width, height + ad_height);
                rv = doc01.Views.Add(v_name, Rhino.Display.DefinedViewportProjection.Perspective, view01_Pos, true);
            }

            //Get all the displaymodes and pick out the AR_Overlay displaymode
            List<string> DisplayMode_Names = new List<string>();
            DisplayModeDescription[] displaymodes = Rhino.Display.DisplayModeDescription.GetDisplayModes();
            for (int i = 0; i < displaymodes.Length; i++)
            {
                DisplayMode_Names.Add(displaymodes[i].EnglishName);
                if (displaymodes[i].EnglishName == d_name)
                {
                    display_mode = displaymodes[i];
                }
            }

            //Apply the AR_Overlay displaymode to the AR viewport
            if (rv != null)
            {
                rv.ActiveViewport.DisplayMode = display_mode;
            }

            //Get all the RhinoViewport names(Different in Rhino8 and Rhino7 API)
            RhinoView[] rhinoViews = doc01.Views.GetViewList(true, false);
            foreach (RhinoView rhinoView in rhinoViews)
            {
                if (rhinoView.MainViewport.Name == v_name)
                {
                    pick = rhinoView;
                }
            }

            //Get all the RhinoViewport names
            List<string> Viewport_names = new List<string>();
            foreach (RhinoView rhinoView in rhinoViews)
            {
                Viewport_names.Add(rhinoView.MainViewport.Name);
            }
            DA.SetData(0, pick);
            DA.SetData(1, Viewport_names);
            DA.SetData(2, DisplayMode_Names);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.CreateARViewport;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("47357DF8-AAF2-4EE7-8709-193C1241141A"); }
        }
    }
}