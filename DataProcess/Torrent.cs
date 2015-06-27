using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Simplist3 {
	public partial class MainWindow : Window {
		string TorrentCaption = "";
		private void ShowDownloadWindow(string title, int episode, string keyword) {
			SetTitlebar(TabMode.Download);
			Mode = ShowMode.Download;

			textNewCaption.Text = "";
			buttonBack.ViewMode = ImageButton.Mode.Hidden;

			TorrentCaption = title;
			textCaption.Text = title;
			textEpisode.Text = episode.ToString();

			stackTorrent.Children.Clear();

			AnimateDownloadWindow(1, 0);
			StopTorrentIndicator();
			StopSubtitleIndicator();
			RefreshDownloadControl("Torrent");

			RefreshTorrent(keyword);
			InitSubtitle(title);
		}

		BackgroundWorker BwTorrent;
		private void RefreshTorrent(string keyword) {
			StopBackgroundWorker(BwTorrent);

			BwTorrent = new BackgroundWorker() {
				WorkerSupportsCancellation = true,
			};
			BwTorrent.DoWork += BwTorrent_DoWork;
			BwTorrent.RunWorkerCompleted += BwTorrent_RunWorkerCompleted;

			StartTorrentIndicator();
			BwTorrent.RunWorkerAsync(keyword);
		}

		private void BwTorrent_DoWork(object sender, DoWorkEventArgs e) {
			e.Result = Parser.GetTorrentList(e.Argument.ToString());
			e.Cancel = CheckWorkerCancel(sender);
		}

		private void BwTorrent_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			if (e.Cancelled) { return; }

			List<Listdata> list = e.Result as List<Listdata>;

			stackTorrent.Children.Clear();

			foreach (Listdata data in list) {
				ListItem item = new ListItem(data);
				item.Response += TorrentItem_Response;
				stackTorrent.Children.Add(item);
			}

			scrollTorrent.ScrollToTop();
			StopTorrentIndicator();
		}

		private void TorrentItem_Response(object sender, CustomButtonEventArgs e) {
			switch (e.ActionType) {
				case "Torrent":
					BackgroundWorker bwDownload = new BackgroundWorker();
					bwDownload.DoWork += bwDownload_DoWork;
					bwDownload.RunWorkerAsync(new Pair(e.Detail, e.Main));
					RefreshDownloadControl("Subtitle");
					break;
			}
		}

		private void bwDownload_DoWork(object sender, DoWorkEventArgs e) {
			Pair pair = e.Argument as Pair;
			string path = Network.DownloadFile(pair.First, string.Format("{0}.torrent", pair.Second));
			if (path != null) { Function.ExecuteFile(path); }
		}

		private void DownloadTab_Response(object sender, CustomButtonEventArgs e) {
			RefreshDownloadControl(e.Main);
		}
		private void RefreshDownloadControl(string type) {
			double op = type == "Torrent" ? 1 : 0;
			bool hit = type == "Torrent" ? true : false;

			ChangeTitle(type);

			scrollTorrent.IsHitTestVisible = hit;
			gridSubtitle.IsHitTestVisible = !hit;

			gridTorrentCaption.Visibility = hit ? Visibility.Visible : Visibility.Collapsed;
			gridSubtitleCaption.Visibility = !hit ? Visibility.Visible : Visibility.Collapsed;

			tabTorrent.ViewMode = hit ? TabButton.Mode.Focused : TabButton.Mode.Clickable;
			tabSubtitle.ViewMode = hit ? TabButton.Mode.Clickable : TabButton.Mode.Focused;

			Storyboard sb = new Storyboard();
			sb.Children.Add(Animation.GetDoubleAnimation(op, scrollTorrent));
			sb.Children.Add(Animation.GetDoubleAnimation(1 - op, gridSubtitle));

			sb.Begin(this);
		}
		private void AnimateDownloadWindow(double opacity, double topMargin) {
			Storyboard sb = new Storyboard();

			gridDown.IsHitTestVisible = opacity == 1 ? true : false;

			DoubleAnimation opDown = Animation.GetDoubleAnimation(opacity, gridDown, 250, 250 * opacity);
			DoubleAnimation opCont = Animation.GetDoubleAnimation(1 - opacity, gridContent, 250, 250 * (1 - opacity));
			ThicknessAnimation taDownTab = Animation.GetThicknessAnimation(350, 0, topMargin, gridDownTab);
			ThicknessAnimation taCont = Animation.GetThicknessAnimation(500, 0, -150 * opacity, gridContent, 0, 0, 250 * (1 - opacity));

			sb.Children.Add(opDown);
			sb.Children.Add(opCont);
			sb.Children.Add(taDownTab);
			//sb.Children.Add(taCont);

			sb.Begin(this);
		}

		private void StopBackgroundWorker(BackgroundWorker bw) {
			if (bw == null) { return; }

			try {
				bw.CancelAsync();
			} catch (Exception ex) {
			}
		}
		private bool CheckWorkerCancel(object sender) {
			BackgroundWorker bw = sender as BackgroundWorker;
			return bw.CancellationPending;
		}

		private void TorrentEpisode_Response(object sender, CustomButtonEventArgs e) {
			int type = 0;
			try {
				type = Convert.ToInt32(e.Main);
			} catch { return; }

			if (!Data.DictSeason.ContainsKey(TorrentCaption)) { return; }

			string source = Data.DictSeason[TorrentCaption].ArchiveTitle;
			int episode = Math.Max(0, Data.DictArchive[source].Episode + type);

			textEpisode.Text = episode.ToString();
			RefreshArchiveEpisode(source, episode);
		}
	}
}
