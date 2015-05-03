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
using ShimiKore;

namespace Simplist3 {
	public partial class MainWindow : Window {

		Updater updater = null;

		private void InitUpdater() {
			updater = new Updater("Simplist3", Version.NowVersion);
			updater.UpdateAvailable += UpdateAvailable;
			updater.UpdateComplete += UpdateComplete;

			updater.UpdateCheck();
		}

		private void UpdateCheck() {
			textVersion.Text = "";
			buttonUpdateCheck.StartAnimateImage(1);
			updater.UpdateCheck();
		}

		private void UpdateAvailable(object sender, UpdateArgs e) {
			textVersion.Text = string.Format("{0} (Newest {1})", Version.NowVersion, e.NewVersion);
			SetImageByMode(buttonUpdate, Tab, true, TabMode.Setting);
			SetImageByMode(buttonUpdateCheck, Tab, true, TabMode.Setting);

			if (e.IsOld) {
				tabSetting.StartAnimateImage();
			}
			else {
				//Notice("최신입니다.");
				buttonUpdateCheck.StopAnimateImage();
			}
		}

		private void UpdateDownload() {
			StartUpdateIndicator();
			updater.UpdateApplication();
		}

		private void UpdateComplete(object sender, UpdateCompleteArgs e) {
			if (e.Complete) {
				this.Close();
			}
			else {
				StopUpdateIndicator();
			}
		}
	}
}
