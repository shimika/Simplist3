using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Simplist3 {
	public partial class MainWindow : Window {
		DispatcherTimer TimerUpdate;
		private void InitUpdateTimer() {
			TimerUpdate = new DispatcherTimer();
			TimerUpdate.Interval = TimeSpan.FromHours(1);
			TimerUpdate.Tick += TimerUpdate_Tick;
			TimerUpdate.Start();

			ver = Setting.Version;
			CheckUpdate();
		}

		private void TimerUpdate_Tick(object sender, EventArgs e) {
			CheckUpdate();
		}

		bool IsChecking = false;
		private void CheckUpdate() {
			if (IsChecking ||  ver != Setting.Version) { return; }
			IsChecking = true;

			WebClient web = new WebClient();
			web.DownloadStringCompleted += web_DownloadStringCompleted;
			web.DownloadStringAsync(new Uri("https://dl.dropboxusercontent.com/u/95054900/Simplist.txt"));
		}

		string ver = "", url = "";
		private void web_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e) {
			IsChecking = false;
			if (e.Error != null) {
				ver = ver = Setting.Version;
				url = "";
				return;
			}

			try {
				string[] res = e.Result.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
				ver = res[0];
				url = res[1];

				textVersion.Text = string.Format("{0} (Newest {1})", Setting.Version, ver);
				SetImageMode(buttonUpdate, Tab, true, TabMode.Setting);

				if (Setting.Version != ver) {
					tabSetting.StartAnimateImage();
				}
			} catch {
			}
		}

		private void UpdateDownload() {
			try {
				Function.ExecuteFile(url);
			} catch { }
		}
	}
}
