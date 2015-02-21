using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
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

			NewVersion = Version.NowVersion;
			CheckUpdate();
		}

		private void TimerUpdate_Tick(object sender, EventArgs e) {
			CheckUpdate();
		}

		bool IsChecking = false;
		private void CheckUpdate(bool isForce = false) {
			if (IsChecking ||  NewVersion != Version.NowVersion) { return; }
			IsChecking = true;
			buttonUpdateCheck.StartAnimateImage(1);

			WebClient web = new WebClient();
			web.DownloadStringCompleted += web_DownloadStringCompleted;
			web.DownloadStringAsync(new Uri("https://dl.dropboxusercontent.com/u/95054900/Shimika/Simplist.txt"), isForce);
		}

		string NewVersion = "", NewUrl = "";
		private void web_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e) {
			buttonUpdateCheck.StopAnimateImage();
			IsChecking = false;
			if (e.Error != null) {
				NewVersion = NewVersion = Version.NowVersion;
				NewUrl = "";
				return;
			}

			bool isForce = (bool)e.UserState;

			try {
				string[] res = e.Result.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
				NewVersion = res[0];
				NewUrl = res[1];

				textVersion.Text = string.Format("{0} (Newest {1})", Version.NowVersion, NewVersion);
				SetImageByMode(buttonUpdate, Tab, true, TabMode.Setting);
				SetImageByMode(buttonUpdateCheck, Tab, true, TabMode.Setting);

				if (Version.NowVersion != NewVersion) {
					tabSetting.StartAnimateImage();
				} else if (isForce) {
					Notice("최신입니다.");
				}
			} catch {
			}
		}

		public static string UpdateFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Shimika\";

		BackgroundWorker BwUpdate;
		bool isUpdating = false;
		private void UpdateDownload() {
			if (isUpdating) { return; }
			if (NewUrl == "") { return; }

			if (!Directory.Exists(UpdateFolder)) {
				Directory.CreateDirectory(UpdateFolder);
			}

			StartUpdateIndicator();
			StopBackgroundWorker(BwUpdate);

			BwUpdate = new BackgroundWorker() {
				WorkerSupportsCancellation = true,
			};
			BwUpdate.DoWork += BwUpdate_DoWork;
			BwUpdate.RunWorkerCompleted += BwUpdate_RunWorkerCompleted;

			List<string> list = new List<string>();
			// Path, project, 

			list.Add(string.Format("{0}updater.exe", UpdateFolder));
			list.Add(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
			list.Add(Path.GetFileNameWithoutExtension(System.AppDomain.CurrentDomain.FriendlyName));
			list.Add(NewUrl);

			BwUpdate.RunWorkerAsync(list);
		}

		private void BwUpdate_DoWork(object sender, DoWorkEventArgs e) {
			List<string> list = (List<string>)e.Argument;

			string updater = list[0];
			string path = list[1];
			string project = list[2];
			string url = list[3];

			WebClient web = new WebClient();
			web.DownloadFile("https://dl.dropboxusercontent.com/u/95054900/Shimika/Updater.exe", updater);
			web.DownloadFile(url, string.Format("{0}\\{1}_update.exe", path, project));

			e.Cancel = CheckWorkerCancel(sender);
			if (!e.Cancel) {
				e.Result = list;
			}
		}

		private void BwUpdate_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			if (e.Cancelled) {
				StopUpdateIndicator();
				isUpdating = false;
				return;
			}
			
			List<string> list = (List<string>)e.Result;
			string updater = list[0];
			string path = list[1];
			string project = list[2];
			int id = Process.GetCurrentProcess().Id;

			try {
				if (!File.Exists(updater)) {
					throw new Exception("Can't get updater");
				}

				if (!File.Exists(string.Format("{0}\\{1}_update.exe", path, project))) {
					throw new Exception("Update error");
				}

				Process pro = new Process();
				pro.StartInfo = new ProcessStartInfo();
				pro.StartInfo.FileName = updater;
				pro.StartInfo.Arguments = string.Format("\"{0}\" \"{1}\" {2}", project, path, id);
				pro.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				pro.Start();

				this.Close();
			} catch (Exception ex) {
				Notice(ex.Message);
			}

			StopUpdateIndicator();
			isUpdating = false;
		}

		DispatcherTimer timerUpdateIndicator;
		int turnUpdate;

		// Torrent

		private void StartUpdateIndicator() {
			if (timerUpdateIndicator == null) {
				timerUpdateIndicator = new DispatcherTimer();
				timerUpdateIndicator.Interval = TimeSpan.FromMilliseconds(250);
				timerUpdateIndicator.Tick += timerUpdateIndicator_Tick;
			}
			turnUpdate = 0;
			timerUpdateIndicator.Start();
		}

		private void StopUpdateIndicator() {
			if (timerUpdateIndicator != null) {
				timerUpdateIndicator.Stop();
			}
			buttonUpdate.Source = "Resources/download.png";
		}

		private void timerUpdateIndicator_Tick(object sender, EventArgs e) {
			buttonUpdate.Source = string.Format("Resources/download{0}.png", turnUpdate);
			turnUpdate = (turnUpdate + 1) % 4;
		}
	}
}
