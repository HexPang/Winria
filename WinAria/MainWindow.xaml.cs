using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WinAria.Util;
using Winria.Util;

namespace WinAria
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string name = parameter as string;
            string val = value as string;
            if(name == "Start")
            {
                if(val == "active" || val == "complete")
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }else if(name == "Stop")
            {
                if(val != "active")
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }else if(name == "Delete")
            {
                if(val != "active")
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
            if (value == null) return null;
            if(targetType == typeof(Visibility))
            {
                return (bool)value ? Visibility.Visible : Visibility.Collapsed;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) { return null; }
            if (targetType == typeof(Visibility))
            {
                return (bool)value ? Visibility.Visible : Visibility.Collapsed;
            }
            return null;
        }
    }
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        
        public ObservableCollection<MissionItem> MissionList;
        public ObservableCollection<MissionItem> WaittingMissionList;
        public ObservableCollection<MissionItem> PausedMissionList;

        private System.Timers.Timer timer = null;
        public MainWindow()
        {
            InitializeComponent();
            Settings.SetLanguage("", Resources); 
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!AriaUtil.IsRunning())
            {
                AriaUtil.Start();
            }
            else
            {
                reloadList();
            }
            timer = new System.Timers.Timer();
            timer.Enabled = true;
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            reloadList();
        }
        private void reloadListSource()
        {
            if (missionListView == null) return;
            this.Invoke(() => {
                if (StateList.SelectedIndex == 0)
                {
                    missionListView.ItemsSource = MissionList;
                }else if(StateList.SelectedIndex == 1)
                {
                    missionListView.ItemsSource = PausedMissionList;
                }else
                {
                    missionListView.ItemsSource = WaittingMissionList;
                }
                ListBoxItem item = StateList.Items[0] as ListBoxItem;
                if(MissionList != null)
                    item.Content = Settings.GetLanguage(Resources)["Active"] + " (" + MissionList.Count + ")";
                item = StateList.Items[1] as ListBoxItem;
                if(PausedMissionList != null) item.Content = Settings.GetLanguage(Resources)["Finished"] + " (" + PausedMissionList.Count + ")";
                item = StateList.Items[2] as ListBoxItem;
                if(WaittingMissionList != null) item.Content = Settings.GetLanguage(Resources)["Waiting"] + " (" + WaittingMissionList.Count + ")";
            });

        }
        private void reloadList()
        {
            AriaUtil.JsonRequestAsync("aria2.tellActive", null, (ent) => {
                if (ent.Result != null)
                {
                    MissionList = ent.Result.ToObject<ObservableCollection<MissionItem>>();
                    reloadListSource();
                }
            });
            AriaUtil.JsonRequestAsync("aria2.tellStopped", JToken.FromObject(new List<int> { 0, 9999 }), (ent) => {
                if (ent.Result != null)
                {
                    PausedMissionList = ent.Result.ToObject<ObservableCollection<MissionItem>>();
                }
            });
            AriaUtil.JsonRequestAsync("aria2.tellWaiting", JToken.FromObject(new List<int> { 0, 9999 }), (ent) => {
                if (ent.Result != null)
                {
                    WaittingMissionList = ent.Result.ToObject<ObservableCollection<MissionItem>>();
                }
            });
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var input = await this.ShowInputAsync("WinAria", Settings.GetLanguage(Resources)["EnterURLOrMagnet"].ToString());
            if (input != null)
            {
                List<List<string>> files = new List<List<string>> { new List<string> { input } };
                AriaUtil.JsonRequestAsync("aria2.addUri", JToken.FromObject(files), (ent) => {
                    if (ent.Result != null)
                    {
                        reloadList();
                    }
                });
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string gid = btn.Tag as string;
            AriaUtil.JsonRequestAsync("aria2.pause", JToken.FromObject(new List<string> { gid }), (ent) => {
                reloadList();
            });
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string gid = btn.Tag as string;
            AriaUtil.JsonRequestAsync("aria2.remove", JToken.FromObject(new List<string> { gid }), (ent) => {
                reloadList();
            });
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string gid = btn.Tag as string;
            AriaUtil.JsonRequestAsync("aria2.unpause", JToken.FromObject(new List<string> { gid }), (ent) => {
                reloadList();
            });
        }

        private void StateList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            reloadListSource();
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AriaUtil.JsonRequestAsync("aria2.shutdown", null, (ent) => {

            });
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            SettingWindow settingWindow = new SettingWindow();
            settingWindow.ShowDialog();

        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "*.torrent|*.torrent";
            if(ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                byte[] fileBuffer = File.ReadAllBytes(ofd.FileName);
                string base64Str = Convert.ToBase64String(fileBuffer);
                AriaUtil.JsonRequestAsync("aria2.addTorrent", JToken.FromObject(new List<string> { base64Str }), (ent) => {
                    //Debug.WriteLine(ent);
                });
            }
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            Spider spider = new Spider();
            spider.Show();

        }
    }
}
