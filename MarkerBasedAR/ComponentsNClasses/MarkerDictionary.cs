using System;
using System.Collections.Generic;
using OpenCvSharp.Aruco;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using MarkerBasedAR.Properties;
using System.Drawing;

namespace MarkerBasedAR.ComponentsNClasses
{
    public class MarkerDictionary : GH_ValueList
    {
        public MarkerDictionary()
        {
            Category = BasicInfo.Category;
            SubCategory = "Marker";
            NickName = "Dic";
            MutableNickName = false;
            Name = "MarkerDictionary";
            Description = "Provides a list of MarkerDictionary from OpenCvSharp.";

            ListMode = GH_ValueListMode.DropDown;

            ListItems.Clear();

            //ListItems.Add(new GH_ValueListItem("Dict4X4_50",          "\"" + PredefinedDictionaryName.Dict4X4_50.ToString()         + "\"" ));
            //ListItems.Add(new GH_ValueListItem("Dict4X4_100",         "\"" + PredefinedDictionaryName.Dict4X4_100.ToString()        + "\"" ));
            //ListItems.Add(new GH_ValueListItem("Dict4X4_250",         "\"" + PredefinedDictionaryName.Dict4X4_250.ToString()        + "\"" ));
            //ListItems.Add(new GH_ValueListItem("Dict4X4_1000",        "\"" + PredefinedDictionaryName.Dict4X4_1000.ToString()       + "\"" ));
            //ListItems.Add(new GH_ValueListItem("Dict5X5_50",          "\"" + PredefinedDictionaryName.Dict5X5_50.ToString()         + "\"" ));
            //ListItems.Add(new GH_ValueListItem("Dict5X5_100",         "\"" + PredefinedDictionaryName.Dict5X5_100.ToString()        + "\"" ));
            //ListItems.Add(new GH_ValueListItem("Dict5X5_250",         "\"" + PredefinedDictionaryName.Dict5X5_250.ToString()        + "\"" ));
            //ListItems.Add(new GH_ValueListItem("Dict5X5_1000",        "\"" + PredefinedDictionaryName.Dict5X5_1000.ToString()       + "\"" ));
            //ListItems.Add(new GH_ValueListItem("Dict6X6_50",          "\"" + PredefinedDictionaryName.Dict6X6_50.ToString()         + "\"" ));
            //ListItems.Add(new GH_ValueListItem("Dict6X6_100",         "\"" + PredefinedDictionaryName.Dict6X6_100.ToString()        + "\"" ));
            //ListItems.Add(new GH_ValueListItem("Dict6X6_250",         "\"" + PredefinedDictionaryName.Dict6X6_250.ToString()        + "\"" ));
            //ListItems.Add(new GH_ValueListItem("Dict6X6_1000",        "\"" + PredefinedDictionaryName.Dict6X6_1000.ToString()       + "\"" ));
            //ListItems.Add(new GH_ValueListItem("Dict7X7_50",          "\"" + PredefinedDictionaryName.Dict7X7_50.ToString()         + "\"" ));
            //ListItems.Add(new GH_ValueListItem("Dict7X7_100",         "\"" + PredefinedDictionaryName.Dict7X7_100.ToString()        + "\"" ));
            //ListItems.Add(new GH_ValueListItem("Dict7X7_250",         "\"" + PredefinedDictionaryName.Dict7X7_250.ToString()        + "\"" ));
            //ListItems.Add(new GH_ValueListItem("Dict7X7_1000",        "\"" + PredefinedDictionaryName.Dict7X7_1000.ToString()       + "\"" ));
            //ListItems.Add(new GH_ValueListItem("DictArucoOriginal",   "\"" + PredefinedDictionaryName.DictArucoOriginal.ToString()  + "\"" ));
            //ListItems.Add(new GH_ValueListItem("DictAprilTag_16h5",   "\"" + PredefinedDictionaryName.DictAprilTag_16h5.ToString()  + "\"" ));
            //ListItems.Add(new GH_ValueListItem("DictAprilTag_25h9",   "\"" + PredefinedDictionaryName.DictAprilTag_25h9.ToString()  + "\"" ));
            //ListItems.Add(new GH_ValueListItem("DictAprilTag_36h10",  "\"" + PredefinedDictionaryName.DictAprilTag_36h10.ToString() + "\"" ));
            //ListItems.Add(new GH_ValueListItem("DictAprilTag_36h11",  "\"" + PredefinedDictionaryName.DictAprilTag_36h11.ToString() + "\"" ));
            
            ListItems.Add(new GH_ValueListItem("Dict4X4_50",          "\"0\"" ));
            ListItems.Add(new GH_ValueListItem("Dict4X4_100",         "\"1\"" ));
            ListItems.Add(new GH_ValueListItem("Dict4X4_250",         "\"2\"" ));
            ListItems.Add(new GH_ValueListItem("Dict4X4_1000",        "\"3\"" ));
            ListItems.Add(new GH_ValueListItem("Dict5X5_50",          "\"4\"" ));
            ListItems.Add(new GH_ValueListItem("Dict5X5_100",         "\"5\"" ));
            ListItems.Add(new GH_ValueListItem("Dict5X5_250",         "\"6\"" ));
            ListItems.Add(new GH_ValueListItem("Dict5X5_1000",        "\"7\"" ));
            ListItems.Add(new GH_ValueListItem("Dict6X6_50",          "\"8\"" ));
            ListItems.Add(new GH_ValueListItem("Dict6X6_100",         "\"9\"" ));
            ListItems.Add(new GH_ValueListItem("Dict6X6_250",         "\"10\"" ));
            ListItems.Add(new GH_ValueListItem("Dict6X6_1000",        "\"11\"" ));
            ListItems.Add(new GH_ValueListItem("Dict7X7_50",          "\"12\"" ));
            ListItems.Add(new GH_ValueListItem("Dict7X7_100",         "\"13\"" ));
            ListItems.Add(new GH_ValueListItem("Dict7X7_250",         "\"14\"" ));
            ListItems.Add(new GH_ValueListItem("Dict7X7_1000",        "\"15\"" ));
            ListItems.Add(new GH_ValueListItem("DictArucoOriginal",   "\"16\"" ));
            ListItems.Add(new GH_ValueListItem("DictAprilTag_16h5(30)",   "\"17\"" ));
            ListItems.Add(new GH_ValueListItem("DictAprilTag_25h9(35)",   "\"18\"" ));
            ListItems.Add(new GH_ValueListItem("DictAprilTag_36h10(2320)",  "\"19\"" ));
            ListItems.Add(new GH_ValueListItem("DictAprilTag_36h11(587)",  "\"20\"" ));
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("80B1D086-ACB6-43CF-9013-0F93EB57A4F0"); }
        }
        protected override Bitmap Icon
        {
            get
            {
                return Resources.ArucoMarkerDictionary;
            }
        }
    }
}