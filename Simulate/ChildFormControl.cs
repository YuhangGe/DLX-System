using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Simulate
{
    class ChildFormControl
    {
        private delegate void NullParameterFun();
        bool close = false;
        public MainForm mw;
        ChildWindow rw, cw;
        ConditionForm cf;
        SetvalueForm sf;
        SaveMemoryForm saf;
        DeviceForm df;
        bool rwa = true, cwa = true;
        public string clargs=null;
        private static ChildFormControl instance = null;
        protected ChildFormControl()
        {
        }

        public static ChildFormControl getInstance()
        {
            if (instance == null)
                instance = new ChildFormControl();
            return instance;
        }
  
        public MemoryPanel getMemoryPanel()
        {
            return this.mw.memoryPanel;
        }

        public void SaveValueForm()
        {
            if (saf != null)
            {
                try
                {
                    saf.Close();
                }
                catch (Exception e)
                {
                }
                saf = null;
            }
            saf = new SaveMemoryForm();
            saf.Top = this.mw.Top + (this.mw.Height - saf.Height) / 2;
            saf.Left = this.mw.Left + (this.mw.Width - saf.Width) / 2;
            saf.Show();
        }
        public void SetValueForm(String s)
        {
            this.SetValueForm();
            sf.setLocation(s);
        }
        public void SetValueForm()
        {
            if (sf != null)
            {
                try
                {
                    sf.Close();
                }
                catch (Exception e)
                {
                }
                sf = null;
            }
            sf = new SetvalueForm();
            sf.Top = this.mw.Top + (this.mw.Height - sf.Height) / 2;
            sf.Left = this.mw.Left + (this.mw.Width - sf.Width) / 2;
            sf.Show();
        }
        public void ConditionBreakpointsForm()
        {
            if (cf != null)
            {
                try
                {
                    cf.Close();
                }
                catch (Exception e)
                {
                }
                cf = null;
            }
            cf = new ConditionForm();
            cf.Top = this.mw.Top + (this.mw.Height - cf.Height) / 2;
            cf.Left = this.mw.Left + (this.mw.Width - cf.Width) / 2;
            cf.Show();
        }

        public void DevicesForm()
        {
            if (df == null)
            {
                df = new DeviceForm();
                df.Top = this.mw.Top + (this.mw.Height - df.Height) / 2;
                df.Left = this.mw.Left + (this.mw.Width - df.Width) / 2;
                df.Closing += new System.ComponentModel.CancelEventHandler(childWindows_Closing);
                df.Show();
            }
            else
            {
                df.Top = this.mw.Top + (this.mw.Height - df.Height) / 2;
                df.Left = this.mw.Left + (this.mw.Width - df.Width) / 2;
                df.Visibility = Visibility.Visible;
                df.Activate();
            }
        }

        public void setWindows(MainForm mw, ChildWindow w2, ChildWindow w3)
        {
            this.mw = mw;
            rw = w2;
            cw = w3;
            rw.Closing += new System.ComponentModel.CancelEventHandler(childWindows_Closing);
            cw.Closing+=new System.ComponentModel.CancelEventHandler(childWindows_Closing);
            this.mw.Closing += new System.ComponentModel.CancelEventHandler(mw_Closing);
            this.mw.memoryPanel.getViewer("Register").MouseDown += new System.Windows.Input.MouseButtonEventHandler(RegisterViewer_MouseDown);
            this.mw.memoryPanel.getViewer("Console").MouseDown += new System.Windows.Input.MouseButtonEventHandler(ConsoleViewer_MouseDown);
        }

        public void ProcessExit()
        {
            this.mw.Close();
        }

        public void ShowCondition()
        {
            ConditionForm cf = new ConditionForm();
            cf.Top = mw.Top + (mw.Height - cf.Height) / 2;
            cf.Left = mw.Left + (mw.Width - cf.Width) / 2;
            cf.ShowDialog();
        }

        void ConsoleViewer_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.cw.Visibility == Visibility.Hidden)
            {
                this.cw.Visibility = Visibility.Visible;
                return;
            }
            if (this.cw.Visibility == Visibility.Visible)
                this.cw.Visibility = Visibility.Hidden;
        }
        void RegisterViewer_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.rw.Visibility == Visibility.Hidden)
            {
                this.rw.Visibility = Visibility.Visible;
                return;
            }
            if (this.rw.Visibility == Visibility.Visible)
                this.rw.Visibility = Visibility.Hidden;
        }
        void mw_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.close = true;
            rw.Close();
            cw.Close();
            if(df!=null)
                df.Close();
            try
            {
                cf.Close();
            }
            catch (Exception ex) { }
            cf = null;
        }
        void childWindows_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!close)
            {
                e.Cancel = true;
                if(sender is ChildWindow)
                    ((ChildWindow)sender).Visibility = Visibility.Hidden;
                if(sender is DeviceForm)
                    ((DeviceForm)sender).Visibility = Visibility.Hidden;
                return;
            }
        }

        public void AboutForm()
        {
            AboutForm af = new AboutForm();
            af.Owner = this.mw;
            af.ShowDialog();
        }
        
        public void move(double detaX, double detaY)
        {
            if (rw != null && cw != null)
            {
                if (rwa)
                {
                    rw.Top += detaY;
                    rw.Left += detaX;
                }
                if (cwa)
                {
                    cw.Top += detaY;
                    cw.Left += detaX;
                }
            }
        }
        public void active(Window w)
        {
            if (w == this.mw)
            {
                if (rw != null && cw != null)
                {
                    if (rw.Visibility == Visibility.Visible)
                    {
                        rw.Topmost = true;
                        rw.Topmost = false;
                    }
                    if (cw.Visibility == Visibility.Visible)
                    {
                        cw.Topmost = true;
                        cw.Topmost = false;
                    }
                    mw.Topmost = true;
                    mw.Topmost = false;
                }
            }
            if (w == this.rw)
            {
                if (cw != null)
                {
                    if (cw.Visibility == Visibility.Visible)
                    {
                        cw.Topmost = true;
                        cw.Topmost = false;
                    }

                    mw.Topmost = true;
                    mw.Topmost = false;

                    rw.Topmost = true;
                    rw.Topmost = false;
                }
            }
            if (w == this.cw)
            {
                if (rw != null)
                {
                    if (rw.Visibility == Visibility.Visible)
                    {
                        rw.Topmost = true;
                        rw.Topmost = false;
                    }

                    mw.Topmost = true;
                    mw.Topmost = false;

                    cw.Topmost = true;
                    cw.Topmost = false;
                }
            }
        }
        public void absorb(ChildWindow w)
        {
            try
            {
                double top = w.Top;
                double left = w.Left;
                double bottom = top + w.Height;
                double right = left + w.Width;
                double top1 = mw.Top;
                double left1 = mw.Left;
                double bottom1 = top1 + mw.Height;
                double right1 = left1 + mw.Width;
                if (Math.Abs(left - right1) < 15)
                {
                    w.Left = right1;
                    if (w == rw)
                        rwa = true;
                    else
                        cwa = true;
                    return;
                }
                if (Math.Abs(top - bottom1) < 15)
                {
                    w.Top = bottom1;
                    if (w == rw)
                        rwa = true;
                    else
                        cwa = true;
                    return;
                }
                if (Math.Abs(left1 - right) < 15)
                {
                    w.Left = left1 - w.Width;
                    if (w == rw)
                        rwa = true;
                    else
                        cwa = true;
                    return;
                }
                if (Math.Abs(top1 - bottom) < 15)
                {
                    w.Top = top1 - w.Height;
                    if (w == rw)
                        rwa = true;
                    else
                        cwa = true;
                    return;
                }
                if (w == rw)
                    rwa = false;
                else
                    cwa = false;
            }
            catch (Exception ex)
            {
            }
        }
        public void update()
        {
            this.mw.memoryPanel.update();
            ((RegisterPanel)(((ScrollPanel)this.rw.MainPanel.Children[0]).getContent())).update();
        }
        public void JumpPc()
        {
            this.mw.memoryPanel.jumptoPC();
        }
        public void threadUpdate()
        {
            this.mw.Dispatcher.Invoke(new NullParameterFun(this.JumpPc));
            this.mw.Dispatcher.Invoke(new NullParameterFun(this.update));            
            //this.mw.Dispatcher.Invoke(new NullParameterFun(((RegisterPanel)(((ScrollPanel)this.rw.MainPanel.Children[0]).getContent())).update));
        }

        public static bool RectangleOverlapping(int top1, int left1, int width1, int height1, int top2, int left2, int width2, int height2)
        {
            Point[] Vertex1 = new Point[4];
            Point[] Vertex2 = new Point[4];
            Point[,] border1 = new Point[4, 2];
            Point[,] border2 = new Point[4, 2];
            Vertex1[0] = new Point(top1, left1);
            Vertex1[1] = new Point(top1, left1 + width1);
            Vertex1[2] = new Point(top1 + height1, left1 + width1);
            Vertex1[3] = new Point(top1 + height1, left1);
            Vertex2[0] = new Point(top2, left2);
            Vertex2[1] = new Point(top2, left2 + width2);
            Vertex2[2] = new Point(top2 + height2, left2 + width2);
            Vertex2[3] = new Point(top2 + height2, left2);
            border1[0, 0] = Vertex1[0];
            border1[0, 1] = Vertex1[1];
            border1[1, 0] = Vertex1[1];
            border1[1, 1] = Vertex1[2];
            border1[2, 0] = Vertex1[2];
            border1[2, 1] = Vertex1[3];
            border1[3, 0] = Vertex1[3];
            border1[3, 1] = Vertex1[0];
            border2[0, 0] = Vertex2[0];
            border2[0, 1] = Vertex2[1];
            border2[1, 0] = Vertex2[1];
            border2[1, 1] = Vertex2[2];
            border2[2, 0] = Vertex2[2];
            border2[2, 1] = Vertex2[3];
            border2[3, 0] = Vertex2[3];
            border2[3, 1] = Vertex2[0];
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (LineCross(border1[i, 0], border1[i, 1], border2[j, 0], border2[j, 1]))
                        return true;
            if (top1 >= top2 && top1 <= top2 + height2 && left1 >= left2 && left1 <= left2 + width2)
                return true;
            if (top2 >= top1 && top2 <= top1 + height1 && left2 >= left1 && left2 <= left1 + width1)
                return true;
            return false;
        }
        public static bool LineCross(Point L1p1, Point L1p2, Point L2p1, Point L2p2)
        {
            bool dir1, dir2;
            dir1 = (L1p1.X == L1p2.X);
            dir2 = (L2p1.X == L2p2.X);
            if (dir1 == dir2)
            {
                if (dir1)
                {
                    if (L1p1.X != L2p1.X)
                        return false;
                    if (Math.Min(L1p1.Y, L1p2.Y) < Math.Max(L2p1.Y, L2p2.Y))
                        if (Math.Max(L1p1.Y, L1p2.Y) >= Math.Min(L2p1.Y, L2p2.Y))
                            return true;
                        else
                            return false;
                    else if (Math.Min(L1p1.Y, L1p2.Y) == Math.Max(L2p1.Y, L2p2.Y))
                        return false;
                    else
                        if (Math.Min(L1p1.Y, L1p2.Y) <= Math.Max(L2p1.Y, L2p2.Y))
                            return true;
                        else
                            return false;
                }
                else
                {
                    if (L1p1.Y != L2p1.Y)
                        return false;
                    if (Math.Min(L1p1.X, L1p2.X) < Math.Max(L2p1.X, L2p2.X))
                        if (Math.Max(L1p1.X, L1p2.X) >= Math.Min(L2p1.X, L2p2.X))
                            return true;
                        else
                            return false;
                    else if (Math.Min(L1p1.X, L1p2.X) == Math.Max(L2p1.X, L2p2.X))
                        return false;
                    else
                        if (Math.Min(L1p1.X, L1p2.X) <= Math.Max(L2p1.X, L2p2.X))
                            return true;
                        else
                            return false;
                }
            }
            else
            {
                if (dir1)
                {
                    if (Math.Min(L1p1.Y, L1p2.Y) < L2p1.Y
                        && Math.Max(L1p1.Y, L1p2.Y) > L2p1.Y
                        && Math.Min(L2p1.X, L2p2.X) < L1p1.X
                        && Math.Max(L2p1.X, L2p2.X) > L1p1.X)
                        return true;
                    else
                        return false;
                }
                else
                {
                    if (Math.Min(L1p1.X, L1p2.X) < L2p1.X
                        && Math.Max(L1p1.X, L1p2.X) > L2p1.X
                        && Math.Min(L2p1.Y, L2p2.Y) < L1p1.Y
                        && Math.Max(L2p1.Y, L2p2.Y) > L1p1.Y)
                        return true;
                    else
                        return false;
                }
            }
        }
    }
}
