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

			ver = Version.NowVersion;
			CheckUpdate();
		}

		private void TimerUpdate_Tick(object sender, EventArgs e) {
			CheckUpdate();
		}

		bool IsChecking = false;
		private void CheckUpdate(bool isForce = false) {
			if (IsChecking ||  ver != Version.NowVersion) { return; }
			IsChecking = true;

			WebClient web = new WebClient();
			web.DownloadStringCompleted += web_DownloadStringCompleted;
			web.DownloadStringAsync(new Uri("https://dl.dropboxusercontent.com/u/95054900/Simplist.txt"), isForce);
		}

		string ver = "", url = "";
		private void web_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e) {
			IsChecking = false;
			if (e.Error != null) {
				ver = ver = Version.NowVersion;
				url = "";
				return;
			}

			bool isForce = (bool)e.UserState;

			try {
				string[] res = e.Result.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
				ver = res[0];
				url = res[1];

				textVersion.Text = string.Format("{0} (Newest {1})", Version.NowVersion, ver);
				SetImageByMode(buttonUpdate, Tab, true, TabMode.Setting);
				SetImageByMode(buttonUpdateCheck, Tab, true, TabMode.Setting);

				if (Version.NowVersion != ver) {
					tabSetting.StartAnimateImage();
				} else if (isForce) {
					Notice("최신입니다.");
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
