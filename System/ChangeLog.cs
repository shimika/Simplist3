using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Simplist3 {
	public partial class MainWindow : Window {
		private void ShowChangeLog() {
			string[] split = Setting.ChangeLog.Split(
				new string[] { 
					Environment.NewLine }, 
					StringSplitOptions.None);

			string log = "";

			foreach (string str in split) {
				if (str == Version.OldVersion) {
					break;
				}
				log = String.Format("{0}{1}\n", log, str);
			}

			textChangeLog2.Text = log;
			gridChangelog.Visibility = Visibility.Visible;
		}
	}
}
