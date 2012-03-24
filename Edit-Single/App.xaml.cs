using System;
using System.IO;
using System.Net;
using System.Security;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace Edit_Single
{
	public partial class App: System.Windows.Application
	{

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            UIControl.path = Environment.CurrentDirectory;
        }
    }
}
