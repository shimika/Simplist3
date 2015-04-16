using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ShimiKore {
	class Updater {
		public string Project { get; set; }
		public string NowVersion { get; set; }

		public string NewVersion { get; set; }
		public string Url { get; set; }

		public event EventHandler<UpdateArgs> UpdateAvailable;
		public event EventHandler<UpdateCompleteArgs> UpdateComplete;

		private DispatcherTimer timer;
		private string UpdateFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Shimika");

		public Updater(string p, string v) {
			Project = p;
			NowVersion = v;

			timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromMinutes(60);
			timer.Tick += UpdateTimer_Tick;
			timer.Start();
		}

		private void UpdateTimer_Tick(object sender, EventArgs e) {
			UpdateCheck();
		}

		public void UpdateCheck() {
			NameValueCollection c = new NameValueCollection();
			c.Add("app", Project);

			WebClient web = new WebClient();
			web.UploadValuesCompleted += web_UploadValuesCompleted;
			web.UploadValuesAsync(new UriBuilder("http://d.uu.gl/check.php").Uri, "POST", c);
		}

		private void web_UploadValuesCompleted(object sender, UploadValuesCompletedEventArgs e) {
			if (e.Error != null) { return; }

			try {
				string v = System.Text.Encoding.UTF8.GetString(e.Result, 0, e.Result.Length).Trim();

				if (v != "") {
					NewVersion = v;

					if (UpdateAvailable != null) {
						UpdateAvailable(this, new UpdateArgs(v, String.Compare(NowVersion, v) != 0));
					}
				}
			}
			catch { return; }
		}

		bool isUpdating = false;
		public void UpdateApplication() {
			if (isUpdating) { return; }
			isUpdating = true;

			if (!Directory.Exists(UpdateFolder)) {
				Directory.CreateDirectory(UpdateFolder);
			}

			BackgroundWorker BwUpdate = new BackgroundWorker() {
				WorkerSupportsCancellation = true,
			};
			BwUpdate.DoWork += BwUpdate_DoWork;
			BwUpdate.RunWorkerCompleted += BwUpdate_RunWorkerCompleted;

			List<string> list = new List<string>();
			// Path, project, 

			list.Add(string.Format("{0}updater.exe", UpdateFolder));
			list.Add(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location).TrimEnd(Path.DirectorySeparatorChar));
			list.Add(Path.GetFileNameWithoutExtension(System.AppDomain.CurrentDomain.FriendlyName));
			list.Add(String.Format("http://d.uu.gl/shimika/{0}.exe", Project));

			BwUpdate.RunWorkerAsync(list);
		}

		private void BwUpdate_DoWork(object sender, DoWorkEventArgs e) {
			List<string> list = (List<string>)e.Argument;

			string updater = list[0];
			string path = list[1];
			string file = list[2];
			string url = list[3];

			WebClient web = new WebClient();
			web.DownloadFile("http://d.uu.gl/shimika/Updater.exe", updater);
			web.DownloadFile(url, string.Format("{0}\\{1}_update.exe", path, file));

			e.Result = list;
		}

		private void BwUpdate_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			List<string> list = (List<string>)e.Result;
			string updater = list[0];
			string path = list[1];
			string file = list[2];
			int id = Process.GetCurrentProcess().Id;

			try {
				if (!File.Exists(updater)) {
					throw new Exception("Can't get updater");
				}

				if (!File.Exists(string.Format("{0}\\{1}_update.exe", path, file))) {
					throw new Exception("Update error");
				}

				Process pro = new Process();
				pro.StartInfo = new ProcessStartInfo();
				pro.StartInfo.FileName = updater;
				pro.StartInfo.Arguments = string.Format(@"{0} ""{1}"" {2}", file, path, id);
				pro.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				pro.Start();

				if (UpdateComplete != null) {
					UpdateComplete(this, new UpdateCompleteArgs(true));
				}
			}
			catch (Exception ex) {
				if (UpdateComplete != null) {
					UpdateComplete(this, new UpdateCompleteArgs(false));
				}
			}
		}
	}

	public class UpdateArgs : EventArgs {
		public string NewVersion { get; internal set; }
		public bool IsOld { get; internal set; }

		public UpdateArgs(string v, bool o) {
			this.NewVersion = v;
			this.IsOld = o;
		}
	}

	public class UpdateCompleteArgs : EventArgs {
		public bool Complete { get; internal set; }

		public UpdateCompleteArgs(bool v) {
			this.Complete = v;
		}
	}
}
