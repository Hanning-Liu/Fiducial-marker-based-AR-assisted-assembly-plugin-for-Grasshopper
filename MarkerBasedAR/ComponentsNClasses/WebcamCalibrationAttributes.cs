using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;


namespace MarkerBasedAR.ComponentsNClasses
{
    public class WebcamCalibrationAttributes : GH_ComponentAttributes
    {
        internal WebcamCalibrationAttributes(WebcamCalibration component)
        : base(component)
        {
        }


        public override GH_ObjectResponse RespondToMouseDoubleClick(
            GH_Canvas sender,
            GH_CanvasMouseEvent e)
        {
            (DocObject as WebcamCalibration).ShowWebcamForm();
            return base.RespondToMouseDoubleClick(sender, e);
        }
    }
}
