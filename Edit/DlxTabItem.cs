using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace Edit
{
    public class DlxTabItem
    {
        public delegate void eventdelegate(object sender, object[] args);
        public event eventdelegate CloseClick, SelectClick;
        public TabItemHeader Header;
        public TabItemContent Content;
        public int ControlIndex = -1;
        public String header
        {
            set
            {
                this.Header.SetContent(value);
            }
            get
            {
                return this.Header.header;
            }
        }
        public UIElement content
        {
            get
            {
                return this.Content.getContent();
            }
            set
            {
                this.Content.content(value);
            }
        }
        public int index;
        private bool _IsSelect;
        public bool IsSelect
        {
            set
            {
                this._IsSelect = value;
                this.Header.SetSelect(value);
            }
            get
            {
                return this._IsSelect;
            }
        }
        public DlxTabItem()
        {
            this.Header = new TabItemHeader();
            this.Content = new TabItemContent();
            this.Header.CloseClick += new TabItemHeader.eventdelegate(Header_CloseClick);
            this.Header.SelectClick += new TabItemHeader.eventdelegate(Header_SelectClick);
        }

        void Header_SelectClick(object sender, object[] args)
        {
            if (this.SelectClick != null)
                this.SelectClick.Invoke(this, new object[] { this.header, this.ControlIndex });
        }

        void Header_CloseClick(object sender, object[] args)
        {
            if (this.CloseClick != null)
            {
                this.CloseClick.Invoke(this, new object[] { this.header, new TabCloseEvent(), this.ControlIndex });
            }
        }
        public void OnClose()
        {
            if (this.CloseClick != null)
            {
                this.CloseClick.Invoke(this, new object[] { this.header, new TabCloseEvent(), this.ControlIndex });
            }
        }
    }
}
