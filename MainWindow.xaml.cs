using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Simplist3 {
	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainWindow : Window {
		public static MainWindow m;

		public MainWindow() {
			InitializeComponent();


			//this.Left = SystemParameters.VirtualScreenLeft;
			//this.Top = SystemParameters.VirtualScreenTop / 2 - 300;

			m = this;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			double dpiX = 0, dpiY = 0;
			PresentationSource presentationsource = PresentationSource.FromVisual(this);

			if (presentationsource != null) {
				dpiX = presentationsource.CompositionTarget.TransformToDevice.M11;
				dpiY = presentationsource.CompositionTarget.TransformToDevice.M22;
			}

			if (dpiX > 0) {
				int r = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Right;
				int t = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Top;

				this.Left = (r - (this.Width + 100) * dpiX * dpiX) / dpiX;
				this.Top = t * dpiY + 50 * dpiY;
			}
			else {
				this.Left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Right - 450;
				this.Top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height / 2 - 300;
			}

			LoadSetting();
			CheckLite();
			ApplySettingToControl();

			List<ArchiveData> listA = Data.DictArchive.Values.ToList();
			containArchive.Add(false, listA.ToArray());

			for (int i = 0; i < 7; i++) {
				List<SeasonData> listS = Data.DictSeason.Values.Where(x => x.Week == i).ToList();
				(FindName(string.Format("containWeek{0}", i)) as Container).Add(false, listS.ToArray());
			}

			int height = RefreshWeekHead();
			scrollSeason.ScrollToVerticalOffset(height);

			RefreshAnitable(WeekDay);
			InitNotification();
			InitUpdater();

			this.Activated += Window_Activated;

			if (String.Compare(Version.OldVersion, Version.NowVersion) != 0) {
				Setting.SaveSetting();
				ShowChangeLog();
			}
		}
	}
}
