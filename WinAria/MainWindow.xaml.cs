using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WinAria.Util;

namespace WinAria
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public class BitTorrentItem
        {
            //public List<string> announceList { get; set; }
            public Dictionary<string, string> info { get; set; }
            public string mode { get; set; }
        }
        public class MissionUri
        {
            public string status { get; set; }
            public string uri { get; set; }
        }
        public class MissionFile
        {
            public string completedLength { get; set; }
            public string index { get; set; }
            public string length { get; set; }
            public string path { get; set; }
            public string select { get; set; }
            public List<MissionUri> uris { get; set; }
        }
        public class MissionItem {
            public BitTorrentItem bittorrent { get; set; }
            public string completedLength { get; set; }
            public string connections { get; set; }
            public string dir { get; set; }
            public string downloadSpeed { get; set; }
            public string status { get; set; }
            public string totalLengthSize { get { return AriaUtil.GetFileSize(long.Parse(totalLength)); } }
            public string totalLength { get; set; }
            public string uploadLength { get; set; }
            public string uploadSpeed { get; set; }
            public string downloadSpeedSize { get {
                    return AriaUtil.GetFileSize(int.Parse(downloadSpeed)) + "/s";
                } }
            public string gid { get; set; }
            public string progress { get {
                    if(long.Parse(totalLength) == 0)
                    {
                        return "0%";
                    }
                    double pst = (double.Parse(completedLength) / double.Parse(totalLength));
                    return Math.Round((pst * 100),2) + "%";
                }
            }
            public List<MissionFile> files { get; set; }
            public string DisplayName { get
                {
                    string displayName = files[0].path;
                    if(bittorrent != null && bittorrent.info != null && bittorrent.info.ContainsKey("name"))
                    {
                        return bittorrent.info["name"];
                    }
                    else
                    {
                        if(displayName.IndexOf("/") > 0)
                        {
                            displayName = displayName.Substring(displayName.LastIndexOf("/") + 1);
                        }
                    }
                    return displayName;
                }
            }
        }
        public ObservableCollection<MissionItem> MissionList;
        public ObservableCollection<MissionItem> WaittingMissionList;
        public ObservableCollection<MissionItem> PausedMissionList;

        private Timer timer = null;
        public MainWindow()
        {
            InitializeComponent();
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
            timer = new Timer();
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
                    item.Content = "Active(" + MissionList.Count + ")";
                item = StateList.Items[1] as ListBoxItem;
                if(PausedMissionList != null) item.Content = "Paused(" + PausedMissionList.Count + ")";
                item = StateList.Items[2] as ListBoxItem;
                if(WaittingMissionList != null) item.Content = "Waiting(" + WaittingMissionList.Count + ")";
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ShowMessageAsync();
        }

        private async Task ShowMessageAsync()
        {

            var input = await this.ShowInputAsync("WinAria", "Please enter URL or Magnet address");
            if(input != null)
            {
                List<List<string>> files = new List<List<string>> { new List<string> { input } };
                AriaUtil.JsonRequestAsync("aria2.addUri", JToken.FromObject(files),(ent)=> {
                    if(ent.Result != null)
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
    }
}
