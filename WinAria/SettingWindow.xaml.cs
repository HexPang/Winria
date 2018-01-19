using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Winria.Util;

namespace WinAria
{
    /// <summary>
    /// SettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : MetroWindow
    {
        Dictionary<string, string> config;
        public SettingWindow()
        {
            InitializeComponent();
            config = Util.AriaUtil.LoadConfig();
            Settings.SetLanguage("", Resources);
        }

        private void changeDownloadPathBtn_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog browserDialog = new FolderBrowserDialog();
            if(browserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                downloadPath.Text = browserDialog.SelectedPath;
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            downloadPath.Text = config["dir"];
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            config["dir"] = downloadPath.Text;
            Util.AriaUtil.SaveConfig(config);
            DialogResult = true;
            this.Close();
        }
    }
}
