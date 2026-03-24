using Microsoft.VisualBasic;
using SharpCompress.Archives;
using SharpCompress.Archives.GZip;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace bowser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            fetchAutocomplete();
            setupAutocomplete();
            refreshBookmarks();
            webView21.CoreWebView2InitializationCompleted += WebView21_CoreWebView2InitializationCompleted;
            webView21.EnsureCoreWebView2Async();
        }

        private void WebView21_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            webView21.CoreWebView2.NavigationStarting += (send, ev) =>
            {
                textBox1.Text = ev.Uri;
                Cursor = Cursors.WaitCursor;
            };
            webView21.CoreWebView2.NavigationCompleted += (send, ev) =>
            {
                Cursor = Cursors.Default;
            };
            //webView21.CoreWebView2.FaviconChanged += CoreWebView2_FaviconChanged;
            webView21.CoreWebView2.DocumentTitleChanged += (send, ev) =>
            {
                this.Text = webView21.CoreWebView2.DocumentTitle + " - bowser";
            };
            webView21.CoreWebView2.BasicAuthenticationRequested += (send, ev) =>
            {
                ev.Response.UserName = "";
                ev.Response.Password = "";
            };
        }

        private int getBookmarkIndexByUuid(string uuid) {
            StringCollection bookmarks = Properties.Settings.Default.bookmarks;
            if (bookmarks != null)
            {
                for (int i = 0; i < bookmarks.Count; i++)
                {
                    string bookmark = bookmarks[i];
                    if (bookmark.Split(';')[0].Equals(uuid)) {
                        return i;
                    }
                }
            }
            return -1;
        }

        private void makeAndAddBookmarkButton(int index, string bookmarkFull) {
            //Properties.Settings.Default.bookmarks.Clear();
            //Properties.Settings.Default.Save();

            string uuid;
            string iconB64;
            string url;
            string bookmark;

            try
            {
                uuid = bookmarkFull.Split(';')[0];
                iconB64 = bookmarkFull.Split(';')[1];
                url = bookmarkFull.Split(';')[2];
                bookmark = bookmarkFull.Split(';')[3];
            }
            catch (Exception e) {
                Debug.WriteLine(e);
                return;
            }

            ToolStripSplitButton bookmarkButton = new ToolStripSplitButton(bookmark);
            bookmarkButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;

            try
            {
                bookmarkButton.Image = Image.FromStream(new MemoryStream(Convert.FromBase64String(iconB64)));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Invalid image for bookmark " + bookmark + " with url " + url + " with uuid " + uuid);
            }

            bool opening = false;
            bookmarkButton.Click += (sender, e) =>
            {
                if (!opening) {
                    textBox1.Text = url;
                    go();
                }
            };
            bookmarkButton.DropDownOpening += (sender, e) =>
            {
                opening = true;
            };
            bookmarkButton.DropDownClosed += (sender, e) =>
            {
                opening = false;
            };

            toolStrip1.Items.Add(bookmarkButton);

            ToolStripMenuItem editItem = new ToolStripMenuItem("Edit...");
            editItem.Click += (sender, e) =>
            {
                string editedTo = Interaction.InputBox("Edit URL: ", "Edit Bookmark", url, this.Location.X, this.Location.Y);
                editedTo = editedTo.Replace(";", "");
                if (!editedTo.Equals(""))
                {
                    url = editedTo;
                    Properties.Settings.Default.bookmarks[getBookmarkIndexByUuid(uuid)] = uuid + ";" + iconB64 + ";" + editedTo + ";" + bookmark;
                    Properties.Settings.Default.Save();
                }
            };
            bookmarkButton.DropDownItems.Add(editItem);

            ToolStripMenuItem renameItem = new ToolStripMenuItem("Rename...");
            renameItem.Click += (sender, e) =>
            {
                string renamedTo = Interaction.InputBox("Rename to: ", "Rename Bookmark", bookmark, this.Location.X, this.Location.Y);
                renamedTo = renamedTo.Replace(";", "");
                if (!renamedTo.Equals(""))
                {
                    bookmarkButton.Text = renamedTo;
                    bookmark = renamedTo;
                    Properties.Settings.Default.bookmarks[getBookmarkIndexByUuid(uuid)] = uuid + ";" + iconB64 + ";" + url + ";" + renamedTo;
                    Properties.Settings.Default.Save();
                }
            };
            bookmarkButton.DropDownItems.Add(renameItem);

            ToolStripMenuItem deleteItem = new ToolStripMenuItem("Delete");
            deleteItem.Click += (sender, e) =>
            {
                Properties.Settings.Default.bookmarks.RemoveAt(getBookmarkIndexByUuid(uuid));
                Properties.Settings.Default.Save();
                toolStrip1.Items.Remove(bookmarkButton);
            };
            bookmarkButton.DropDownItems.Add(deleteItem);
        }
        private void refreshBookmarks()
        {
            StringCollection bookmarks = Properties.Settings.Default.bookmarks;
            if (bookmarks != null)
            {
                for (int i = 0; i < bookmarks.Count; i++)
                {
                    string bookmark = bookmarks[i];
                    //Debug.WriteLine(bookmark);
                    makeAndAddBookmarkButton(i, bookmark);
                }
            }
        }

        public bool fetchAutocomplete() {
            Debug.WriteLine("Checking for autocomplete");

            bool fetchedNew = false;

            /*if (!File.Exists("autocomplete.csv")) {
                Debug.WriteLine("Downloading autocomplete gz");

                new WebClient().DownloadFile("https://raw.githubusercontent.com/zakird/crux-top-lists/main/data/global/current.csv.gz", "autocomplete.csv.gz");

                if (File.Exists("autocomplete.csv.gz")) {
                    Debug.WriteLine("Extracting autocomplete gz");

                    GZipArchive.OpenArchive("autocomplete.csv.gz").Entries.Where(entry => entry.Key.EndsWith(".csv")).ToList().ForEach(entry => entry.WriteToFile("autocomplete.csv", new ExtractionOptions()));

                    Debug.WriteLine("Done extracting autocomplete gz");
                    fetchedNew = true;
                }
            }*/

            if (!File.Exists("autocomplete.txt"))
            {
                Debug.WriteLine("Downloading autocomplete txt");

                new WebClient().DownloadFile("https://raw.githubusercontent.com/scrapy/protego/refs/heads/master/tests/top-10000-websites.txt", "autocomplete.txt");
                Debug.WriteLine("Done downloading autocomplete txt");
                fetchedNew = true;
            }

            return fetchedNew;
        }

        public void setupAutocomplete() {
            /*string[] autoCompleteLines = File.ReadAllLines("autocomplete.csv");

            int maxAutoCompletes = autoCompleteLines.Length;

            string[] autoCompleteFormattedLines = new string[maxAutoCompletes];
            for (int i = 0; i < maxAutoCompletes; i++) {
                autoCompleteFormattedLines[i] = autoCompleteLines[i].Split(',')[0];
            }
            textBox1.AutoCompleteCustomSource.Clear();
            textBox1.AutoCompleteCustomSource.AddRange(autoCompleteFormattedLines);*/

            string[] autoCompleteLines = File.ReadAllLines("autocomplete.txt");
            textBox1.AutoCompleteCustomSource.Clear();
            textBox1.AutoCompleteCustomSource.AddRange(autoCompleteLines);
        }

        private void go()
        {
            
            if (!DomainEndings.checkIfIn(textBox1.Text)) {
                textBox1.Text = "https://google.com/search?q=" + textBox1.Text;
            } else if (!textBox1.Text.StartsWith("http://") && !textBox1.Text.StartsWith("https://"))
            {
                textBox1.Text = "http://" + textBox1.Text;
            }

            try
                {
                    webView21.CoreWebView2.Navigate(textBox1.Text);
                }
                catch (Exception ex)
                {
                    webView21.NavigateToString("<style>body {background: #fff;}</style><body>Failed to navigate to page!</body>");
                }
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == (char) Keys.Enter) {
                go();
            }
        }

        private void goButton_Click(object sender, EventArgs e)
        {
            go();
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            webView21.CoreWebView2.GoBack();
        }

        private void reloadButton_Click(object sender, EventArgs e)
        {
            webView21.CoreWebView2.Reload();
        }

        private void forwardButton_Click(object sender, EventArgs e)
        {
            webView21.CoreWebView2.GoForward();
        }

        private async void addBookmarkButton_Click(object sender, EventArgs e)
        {
            Stream s = await webView21.CoreWebView2.GetFaviconAsync(Microsoft.Web.WebView2.Core.CoreWebView2FaviconImageFormat.Png);

            string iconB64 = "";
            if (s != null)
            {
                // resize icon
                using (MemoryStream ms = new MemoryStream())
                {
                    s.CopyTo(ms);
                    var bitmap = new Bitmap((Bitmap)Image.FromStream(ms), new Size(16, 16));
                    using (MemoryStream ms2 = new MemoryStream())
                    {
                        bitmap.Save(ms2, System.Drawing.Imaging.ImageFormat.Png);
                        byte[] iconBytes = ms2.ToArray();
                        iconB64 = Convert.ToBase64String(iconBytes);
                    }
                }
            }

            StringCollection bookmarks = Properties.Settings.Default.bookmarks;
            if (bookmarks == null)
            {
                bookmarks = new StringCollection();
            }

            bookmarks.Add(Guid.NewGuid().ToString() + ";" + iconB64 + ";" + textBox1.Text + ";" + webView21.CoreWebView2.DocumentTitle);
            makeAndAddBookmarkButton(bookmarks.Count, bookmarks[bookmarks.Count - 1]);
            Properties.Settings.Default.bookmarks = bookmarks;
            Properties.Settings.Default.Save();
        }

        private void aboutButton_Click(object sender, EventArgs e)
        {
            new About().ShowDialog();
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {
            new Settings().ShowDialog();
        }
    }
}
