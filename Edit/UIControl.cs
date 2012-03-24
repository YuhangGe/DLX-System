using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Edit
{
    class UIControl
    {
        private static UIControl instance;
        public UIControl() { }
        public static UIControl getInstance()
        {
            if (instance == null)
                instance = new UIControl();
            return instance;
        }
        //
        //
        //
        private TreePanel ProjectTreeViewer;
        private DLXTabControl FileControl;

    }

   
}
