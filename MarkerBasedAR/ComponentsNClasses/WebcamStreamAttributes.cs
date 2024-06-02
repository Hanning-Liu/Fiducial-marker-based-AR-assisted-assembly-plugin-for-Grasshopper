using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using Grasshopper.Kernel.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkerBasedAR.ComponentsNClasses
{

    public class WebcamStreamAttributes : GH_ComponentAttributes
    {
        internal WebcamStreamAttributes(WebcamStream component)
        : base(component)
        {
        }


        public override GH_ObjectResponse RespondToMouseDoubleClick(
            GH_Canvas sender,
            GH_CanvasMouseEvent e)
        {
            (DocObject as WebcamStream).ShowWebcamForm();
            return base.RespondToMouseDoubleClick(sender, e);
        }
    }

}
