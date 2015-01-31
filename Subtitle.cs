﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Simplist3 {
	public partial class MainWindow : Window {
		private void InitSubtitle(string title) {
			int week = Data.DictSeason[title].Week;

			gridSubtitle.Children.Clear();

			NowSubtitle = title;
			NowCaption = "";
			NowContainer = null;

			if (StackHistory == null) {
				StackHistory = new Stack<Pair>();
			}
			StackHistory.Clear();

			RefreshSubtitle("Weekdata", week.ToString());
		}

		BackgroundWorker BwSubtitle;
		Stack<Pair> StackHistory;
		string NowCaption, NowSubtitle;
		AniScrollViewer NowContainer = null;

		private void RefreshSubtitle(string type, object tag) {
			if (NowContainer != null) {
				NowContainer.IsHitTestVisible = false;
			}

			StopBackgroundWorker(BwSubtitle);

			BwSubtitle = new BackgroundWorker() {
				WorkerSupportsCancellation = true,
			};
			BwSubtitle.DoWork += BwSubtitle_DoWork;
			BwSubtitle.RunWorkerCompleted += BwSubtitle_RunWorkerCompleted;

			StartSubtitleIndicator();
			BwSubtitle.RunWorkerAsync(new Pair(type, tag));
		}

		private void BwSubtitle_DoWork(object sender, DoWorkEventArgs e) {
			Pair args = e.Argument as Pair;
			List<Listdata> list;
			string title;

			switch (args.First) {
				case "Weekdata":
					string value = args.Second as string;

					list = Parser.Week(value, "Weekdata");
					title = Function.GetWeekday(value);
					Pair pair = CheckMatching(list);

					if (pair != null) {
						title = pair.First;
						list = Parser.GetMakerList(pair.Second as string, false);
					}

					e.Result = new Pair(title, list);
					break;
				case "Anime":
					Pair pairAnime = args.Second as Pair;

					title = pairAnime.First;
					list = Parser.GetMakerList(pairAnime.Second as string, false);

					e.Result = new Pair(title, list);

					break;
				case "Maker":
					Pair pairMaker = args.Second as Pair;
					string url = pairMaker.Second as string;
					list = Parser.GetFileList(url);

					e.Result = new Pair(pairMaker.First, list);

					break;
				case "File":
					Pair pairFile = args.Second as Pair;
					string fileurl = pairFile.Second as string;

					string path = Network.DownloadFile(fileurl, pairFile.First);
					if (path == null) {
						Notice("파일 다운로드 에러");
						e.Result = null;
						return;
					}

					string ext = Path.GetExtension(path).ToLower();

					if (ext == ".smi" || ext == ".ass") {
						Function.SaveFile(path, pairFile.First, NowSubtitle);
						e.Result = null;
					} else if (ext == ".zip" || ext == ".jpg") {
						list = Parser.ParseZip(path);
						e.Result = new Pair(pairFile.First, list);
					} else {
						Function.ExecuteFile(path);
						e.Result = null;
					}

					break;
			}

			e.Cancel = CheckWorkerCancel(sender);
		}

		private Pair CheckMatching(List<Listdata> list) {
			int max = -1, min = 9999;
			string url1 = null, title1 = null;
			string url2 = null, title2 = null;

			foreach (Listdata data in list) {
				int prefix = Function.StringPrefixMatch(NowSubtitle, data.Title);
				if (prefix == NowSubtitle.Length || prefix == data.Title.Length) {
					return new Pair(data.Title, data.Url);
				}

				int match = Function.StringMatching(NowSubtitle, data.Title);

				if (max < prefix) {
					max = prefix;
					title1 = data.Title;
					url1 = data.Url;
				}

				if (min < match) {
					min = match;
					title2 = data.Title;
					url2 = data.Url;
				}
			}

			if (max >= NowSubtitle.Length / 2) {
				return new Pair(title1, url1);
			}
			if (min < NowSubtitle.Length / 3) {
				return new Pair(title2, url2);
			}

			return null;
		}

		private void BwSubtitle_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			if (e.Cancelled) { return; }

			if (e.Result != null) {
				Pair pair = e.Result as Pair;
				string title = pair.First;
				List<Listdata> list = pair.Second as List<Listdata>;

				if (NowContainer != null) {
					buttonBack.ViewMode = ImageButton.Mode.Visible;
					StackHistory.Push(new Pair(NowCaption, NowContainer));
					AnimateContainer(NowContainer, 0, -150);
				} else {
					buttonBack.ViewMode = ImageButton.Mode.Hidden;
				}

				ChangeCaption(title);
				AniScrollViewer scroll = new AniScrollViewer() {
					Opacity = 0,
					Margin = new Thickness(150, 0, 0, 0)
				};
				StackPanel stack = new StackPanel();

				foreach (Listdata data in list) {
					ListItem item = new ListItem(data);
					item.Response += SubtitleItem_Response;
					stack.Children.Add(item);
				}

				NowCaption = title;
				NowContainer = scroll;
				AnimateContainer(scroll, 1, 0);

				scroll.Content = stack;
				gridSubtitle.Children.Add(scroll);
				scroll.ScrollToTop();
			} else {
				NowContainer.IsHitTestVisible = true;
			}

			StopSubtitleIndicator();
		}

		private void SubtitleItem_Response(object sender, CustomButtonEventArgs e) {
			switch (e.ActionType) {
				case "Maker":
					// Main : Title
					// Detail : Blog URL
					RefreshSubtitle(e.ActionType, new Pair(e.Main, e.Detail));
					break;
				case "Blog":
					if (e.Detail == "") {
						Notice("URL 분석 실패");
					} else {
						Function.ExecuteFile(new UriBuilder(e.Detail).Uri.ToString());
					}
					break;
				case "File":
					RefreshSubtitle(e.ActionType, new Pair(e.Main, e.Detail));
					break;
				case "Zip":
					Function.Unzip(e.Detail, e.Main, NowSubtitle);
					break;
			}
		}

		private void BackButton_Response(object sender, CustomButtonEventArgs e) {
			if (e.ActionType != "Click") { return; }
			if (NowContainer == null) { return; }

			Pair pair = StackHistory.Pop();
			ChangeCaption(pair.First, -1);
			AniScrollViewer scroll = pair.Second as AniScrollViewer;

			AnimateContainer(NowContainer, 0, 150);
			AnimateContainer(scroll, 1, 0);

			NowCaption = pair.First;
			NowContainer = scroll;

			if (StackHistory.Count > 0) {
				buttonBack.ViewMode = ImageButton.Mode.Visible;
			} else {
				buttonBack.ViewMode = ImageButton.Mode.Hidden;
			}
		}

		private void ChangeCaption(string str, int m = 1) {
			textOldCaption.Text = textNewCaption.Text;
			textNewCaption.Text = str;

			Storyboard sb = new Storyboard();

			DoubleAnimation daOld = Animation.GetDoubleAnimation(0, textOldCaption, 350);
			daOld.From = 1;
			DoubleAnimation daNewStart = Animation.GetDoubleAnimation(0, textNewCaption, 0);
			DoubleAnimation daNew = Animation.GetDoubleAnimation(1, textNewCaption, 350);
			daNew.From = 0;

			ThicknessAnimation taOld = Animation.GetThicknessAnimation(350, -150 * m, 0, textOldCaption);
			taOld.From = new Thickness(0);
			ThicknessAnimation taNew = Animation.GetThicknessAnimation(350, 0, 0, textNewCaption);
			taNew.From = new Thickness(150 * m, 0, 0, 0);

			sb.Children.Add(daOld);
			sb.Children.Add(daNewStart);
			sb.Children.Add(daNew);
			sb.Children.Add(taOld);
			sb.Children.Add(taNew);

			sb.Begin(this);
		}
		private void AnimateContainer(AniScrollViewer element, double opacity, double left) {
			element.IsHitTestVisible = opacity == 0 ? false : true;

			Storyboard sb = new Storyboard();

			sb.Children.Add(Animation.GetDoubleAnimation(opacity, element, 350));
			sb.Children.Add(Animation.GetThicknessAnimation(350, left, 0, element));

			sb.Begin();
		}
	}
}
