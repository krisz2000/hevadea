﻿using Microsoft.Xna.Framework;

namespace Hevadea.Framework.UI.Containers
{
    public class DockContainer : Container
    {
        public override void Layout()
        {
            var h = new Rectangle(UnitHost.Location, UnitHost.Size);

            foreach (var c in Childrens)
            {
                var b = c.UnitBound;
                switch (c.Dock)
                {
                    case Dock.Top:
                        b = new Rectangle(h.X, h.Y, h.Width, b.Height);
                        h = new Rectangle(h.X, h.Y + b.Height, h.Width, h.Height - b.Height);
                        break;
                    case Dock.Right:
                        b = new Rectangle(h.X + h.Width - b.Width, h.Y, b.Width, h.Height);
                        h = new Rectangle(h.X, h.Y , h.Width - b.Width, h.Height);
                        break;
                    case Dock.Bottom:
                        b = new Rectangle(h.X, h.Y + h.Height - b.Height, h.Width, b.Height);
                        h = new Rectangle(h.X, h.Y, h.Width, h.Height - b.Height);
                        break;
                    case Dock.Left:
                        b = new Rectangle(h.X, h.Y, b.Width, h.Height);
                        h = new Rectangle(h.X + b.Width, h.Y, h.Width - b.Width, h.Height);
                        break;
                    case Dock.Fill:
                        b = h;
                        break;
                    case Dock.None:
                        break;
                }
                
                c.UnitBound = b;
            }
        }
    }
}