using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Simplist3 {
	public partial class MainWindow : Window {
		DispatcherTimer timerTorrentIndicator;
		DispatcherTimer timerSubtitleIndicator;
		int turnTorrent, turnSubtitle;

		// Torrent

		private void StartTorrentIndicator() {
			if (timerTorrentIndicator == null) {
				timerTorrentIndicator = new DispatcherTimer();
				timerTorrentIndicator.Interval = TimeSpan.FromMilliseconds(250);
				timerTorrentIndicator.Tick += timerTorrentIndicator_Tick;
			}
			turnTorrent = 0;
			timerTorrentIndicator.Start();
		}

		private void StopTorrentIndicator() {
			if (timerTorrentIndicator != null) {
				timerTorrentIndicator.Stop();
			}
			tabTorrent.Source = "Resources/download.png";
		}

		private void timerTorrentIndicator_Tick(object sender, EventArgs e) {
			tabTorrent.Source = string.Format("Resources/download{0}.png", turnTorrent);
			turnTorrent = (turnTorrent + 1) % 4;
		}

		// Subtitle

		private void StartSubtitleIndicator() {
			if (timerSubtitleIndicator == null) {
				timerSubtitleIndicator = new DispatcherTimer();
				timerSubtitleIndicator.Interval = TimeSpan.FromMilliseconds(250);
				timerSubtitleIndicator.Tick += timerSubtitleIndicator_Tick;
			}

			turnSubtitle = 0;
			timerSubtitleIndicator.Start();
		}

		private void StopSubtitleIndicator() {
			if (timerSubtitleIndicator != null) {
				timerSubtitleIndicator.Stop();
			}
			tabSubtitle.Source = "Resources/subtitle.png";
		}

		private void timerSubtitleIndicator_Tick(object sender, EventArgs e) {
			tabSubtitle.Source = string.Format("Resources/subtitle{0}.png", turnSubtitle);
			turnSubtitle = (turnSubtitle + 1) % 4;
		}
	}
}
