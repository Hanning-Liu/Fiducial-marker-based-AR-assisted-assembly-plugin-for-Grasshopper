using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using MarkerBasedAR.Properties;

namespace MarkerBasedAR.ComponentsNClasses
{
    public class Module_Index : GH_Component
    {
        int previousOutputValue = 0;
        int previousTempValue = 0;
        bool previousDecrementState = false;
        bool previousIncrementState = false;
        /// <summary>
        /// Initializes a new instance of the Module_Index class.
        /// </summary>
        public Module_Index()
          : base("Module Index", "Index",
              "The index of the module to display.",
              BasicInfo.Category, "Interaction")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Start", "S", "Start value of the domain", GH_ParamAccess.item);
            pManager.AddIntegerParameter("End", "E", "End value of the domain", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Decrement", "Dec", "Decrement the value", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Increment", "Inc", "Increment the value", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("SetValue", "Set", "Set the current value directly", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Output", "Out", "Current output value", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Declare variables to store inputs
            int start = 0, end = 0, tempValue = 0;
            bool increment = false, decrement = false;
            int currentValue = previousOutputValue;

            // Retrieve inputs
            if (!DA.GetData(0, ref start)) return;
            if (!DA.GetData(1, ref end)) return;
            if (!DA.GetData(2, ref decrement)) return;
            if (!DA.GetData(3, ref increment)) return;
            if (!DA.GetData(4, ref tempValue)) return;

            // Update the current value based on inputs
            if (decrement) currentValue--;
            if (increment) currentValue++;

            // Test if the manually set value was changed or not
            if (tempValue != previousOutputValue && previousDecrementState != true && previousIncrementState != true && tempValue != previousTempValue) 
                currentValue = tempValue;

            // Constrain the current value within the specified domain
            currentValue = Math.Max(start, Math.Min(currentValue, end));

            // Set the output
            DA.SetData(0, currentValue);
            previousOutputValue = currentValue;
            previousTempValue = tempValue;
            previousDecrementState = decrement;
            previousIncrementState = increment;
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.Module_Index;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("88AE2DC7-6E85-4301-A49D-114EE69AD5D2"); }
        }
    }
}