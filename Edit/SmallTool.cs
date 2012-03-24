using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows;
namespace Edit
{
    class SmallTool
    {

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetCursorPos(out POINT pt);
        public static Point GetMousePosition()
        {
            Point p = new Point();
            POINT p1 = new POINT();
            GetCursorPos(out p1);
            p.X = p1.X;
            p.Y = p1.Y;
            return p;
        }
    }
}
