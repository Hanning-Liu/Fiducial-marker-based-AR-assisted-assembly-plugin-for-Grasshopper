using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkerBasedAR.ComponentsNClasses
{
    internal class BitmapPreviewAttributes : GH_ComponentAttributes
    {
        private Rectangle ButtonBounds { get; set; }
        internal BitmapPreviewAttributes(BitmapPreview component)
        : base(component)
        {
        }
        protected override void Layout()
        {
            base.Layout();
            BitmapPreview owner = Owner as BitmapPreview;
            int num1 = owner.preview.Width;
            if (num1 < 50)
                num1 = 50;
            int num2 = owner.preview.Height;
            if (num2 < 50)
                num2 = 50;
            Rectangle rectangle1 = GH_Convert.ToRectangle(Bounds);

            rectangle1.Width = num1;
            rectangle1.Height += num2;
            Rectangle rectangle2 = rectangle1;
            rectangle2.Y = rectangle2.Bottom - num2;
            rectangle2.Height = num2;
            rectangle2.Width = num1;
            Bounds = (RectangleF)rectangle1;
            ButtonBounds = rectangle2;
        }
        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);
            BitmapPreview owner = Owner as BitmapPreview;
            if (channel != GH_CanvasChannel.Objects)
                return;
            GH_Capsule capsule = GH_Capsule.CreateCapsule(ButtonBounds, GH_Palette.Normal, 0, 0);
            capsule.Render(graphics, Selected, Owner.Locked, true);
            capsule.AddOutputGrip(OutputGrip.Y);
            capsule.Dispose();
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            //RectangleF buttonBounds = (RectangleF)ButtonBounds;
            graphics.DrawImage(owner.preview, Bounds.X + 2f, m_innerBounds.Y - (ButtonBounds.Height - Bounds.Height), (owner.preview.Width - 4), (owner.preview.Height - 4));
            stringFormat.Dispose();
        }
    }
}
