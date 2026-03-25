using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace bowser
{
    public partial class Settings : Form
    {
        private Form1 parent;
        public Settings(Form1 parent)
        {
            InitializeComponent();

            this.parent = parent;

            proxyURLTextBox.Text = Properties.Settings.Default.proxyURL;
            proxyUsernameTextBox.Text = Properties.Settings.Default.proxyUsername;
            proxyPasswordTextBox.Text = Properties.Settings.Default.proxyPassword;
            searchEngineComboBox.SelectedIndex = Properties.Settings.Default.searchEngine;

            if (ShowDialog() == DialogResult.OK) {
                Properties.Settings.Default.proxyURL = proxyURLTextBox.Text;
                Properties.Settings.Default.proxyUsername = proxyUsernameTextBox.Text;
                Properties.Settings.Default.proxyPassword = proxyPasswordTextBox.Text;
                Properties.Settings.Default.searchEngine = searchEngineComboBox.SelectedIndex;
                Properties.Settings.Default.Save();
            }
        }

        private void resetSettingsButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to reset all your settings?", "Reset Settings", MessageBoxButtons.OKCancel) == DialogResult.OK) {
                StringCollection bookmarks = Properties.Settings.Default.bookmarks;
                Properties.Settings.Default.Reset();
                Properties.Settings.Default.bookmarks = bookmarks;
                Properties.Settings.Default.Save();
            }
        }

        private void clearBookmarksButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to remove all your bookmarks?", "Clear Bookmarks", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                Properties.Settings.Default.bookmarks.Clear();
                Properties.Settings.Default.Save();

                parent.clearBookmarks();
            }
        }
    }
}
