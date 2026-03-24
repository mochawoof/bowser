using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace bowser
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
            string hkey = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion";
            label2.Text = $"bowser v{Assembly.GetExecutingAssembly().GetName().Version}" +
                $"\nWebView2 Version: {CoreWebView2Environment.GetAvailableBrowserVersionString()}" + 
                $"\n{Registry.GetValue(hkey, "ProductName", "")} {Registry.GetValue(hkey, "DisplayVersion", "")} ({Registry.GetValue(hkey, "CurrentBuildNumber", "")})" + 
                "\nhttps://github.com/mochawoof/bowser";
        }
    }
}
