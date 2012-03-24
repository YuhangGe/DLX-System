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
using System.Diagnostics;

namespace Simulate
{
	public partial class App: System.Windows.Application
	{
       
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
             
                if (e!=null && e.Args !=null && e.Args.Length != 0)
                {
                    ChildFormControl.getInstance().clargs = e.Args[0];
               
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
