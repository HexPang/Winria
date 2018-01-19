using MahApps.Metro.Controls;
using System.Collections.Generic;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using WinAria.Util;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace WinAria
{
    /// <summary>
    /// Spider.xaml 的交互逻辑
    /// </summary>
    public partial class Spider : MetroWindow
    {
        List<string> urlList;
        public Spider()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string url = urlTextBox.Text;
            if (url != "")
            {
                Button btn = sender as Button;
                btn.IsEnabled = false;
                HttpUtil.Instance.RequestGetAsync(url, (ent) => {
                    string fileExtStr = "apk,mpg,mpeg,avi,rm,rmvb,mov,wmv,asf,dat,asx,wvx,mpe,mpa,mp3,wma,rm,ram,wav,mid,midi,rmi,m3u,wplogg,ape,cda,au,aiff,aif,aifc,669,wax,snd,exe,msi,bat,txt,rtf,iaf,wab,mht,doc,chm,reg,dll,ini,log,ctt,fla,swf,zip,rar,cab,ace,z,arc,arj,lzh,tar,uue,gzip,iso,bin,cif,nrg,vcd,fcd,img,c2d,tao,dao,vhd,jpeg,jpg,gif,bmp,png,ico,icl,psd,tif,cr2,crw,cur,ani,bmp,gif,tiff,pcx,png,jpeg,targe,wmf";
                    string[] fileExt = fileExtStr.Split(',');
                    Regex reg = new Regex(@"(?is)<a[^>]*?href=(['""]?)(?<url>[^'""\s>]+)\1[^>]*>(?<text>(?:(?!</?a\b).)*)</a>");
                    MatchCollection mc = reg.Matches(ent.raw);
                    urlList = new List<string>();
                    foreach (Match m in mc)
                    {
                        string u = m.Groups["url"].Value;
                        string text = m.Groups["text"].Value;
                        if (u.StartsWith("magnet:?"))
                        {
                            if (!urlList.Exists((s) => { if (s == u) return true; else return false; }))
                            {
                                urlList.Add(System.Web.HttpUtility.UrlDecode(u));
                            }
                        }
                        else
                        {
                            foreach (string ext in fileExt)
                            {
                                if (u.IndexOf("." + ext) > 0)
                                {
                                    if (!urlList.Exists((s) => { if (s == u) return true; else return false; }))
                                    {
                                        urlList.Add(System.Web.HttpUtility.UrlDecode(u));
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    urlListBox.ItemsSource = urlList;
                    btn.IsEnabled = true;
                });
            }
            //List<string> urlList 
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if(urlListBox.SelectedItems.Count > 0)
            {
                List<string> uri = new List<string>();
                foreach(string item in urlListBox.SelectedItems)
                {
                    uri.Add(item);
                }
                AriaUtil.AddURI(uri, (ent) => {
                    Debug.WriteLine(ent);
                });
            }
        }
    }
}
