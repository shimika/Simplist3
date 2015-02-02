using System;
using System.Collections.Generic;
using System.Linq;
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
	/// Container.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class Container : UserControl {
		public Container() {
			InitializeComponent();
		}

		public enum ListType { Season, Archive };

		[System.ComponentModel.DefaultValue(ListType.Season)]
		public ListType ContainerType { get; set; }

		#region Title
		public string Title {
			get { return (string)GetValue(TitleProperty); }
			set {
				SetValue(TitleProperty, value);
			}
		}
		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
			"Title",
			typeof(string),
			typeof(Container),
			new FrameworkPropertyMetadata(
				null,
				FrameworkPropertyMetadataOptions.AffectsRender,
				TitlePropertyChanged));

		private static void TitlePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			Container container = obj as Container;

			if (e.NewValue == null) {
				container.gridTitle.Visibility = Visibility.Collapsed;
			} else {
				container.gridTitle.Visibility = Visibility.Visible;
				container.textTitle.Text = e.NewValue.ToString();
			}
		}

		public void SetWeekDay(bool focus) {
			if (focus) {
				this.textTitle.Opacity = 1;
				this.textTitle.Foreground = Brushes.Crimson;
				this.textTitle.Text = string.Format("{0} ★", this.Title);
			} else {
				this.textTitle.Opacity = 0.5;
				this.textTitle.Foreground = FindResource("PrimaryBrush") as SolidColorBrush;
				this.textTitle.Text = this.Title;
			}
		}

		#endregion

		Dictionary<string, SeasonData> TableSeason = new Dictionary<string, SeasonData>();

		Dictionary<string, ListItem> ItemDicionary = new Dictionary<string, ListItem>();

		public void Add(bool animate, params SeasonData[] dataCollect) {
			if (ContainerType == ListType.Archive) { return; }

			foreach (SeasonData data in dataCollect) {
				TableSeason.Add(data.Title, data);

				ListItem item = new ListItem(data, animate);
				item.Response += item_Response;
				ItemDicionary.Add(data.Title, item);
			}

			RefreshContainer();
		}

		public int Add(bool animate, params ArchiveData[] dataCollect) {
			if (ContainerType == ListType.Season) { return -1; }

			string lastTitle = null;

			foreach (ArchiveData data in dataCollect) {
				ListItem item = new ListItem(data, animate);
				item.Response += item_Response;
				ItemDicionary.Add(data.Title, item);

				lastTitle = data.Title;
			}

			RefreshContainer();

			if (lastTitle != null && animate) {
				List<string> list = null;

				if (Status.NowSelect == Status.SelectMode.All) {
					list = Data.DictArchive.Values.Select(x => x.Title)
						.OrderBy(x => x).ToList();
				} else {
					list = Data.DictArchive.Values.Where(x => x.Episode >= 0).Select(x => x.Title)
						.OrderBy(x => x).ToList();
				}
				return list.IndexOf(lastTitle);
			}

			return -1;
		}

		public event EventHandler<CustomButtonEventArgs> Response;
		private void item_Response(object sender, CustomButtonEventArgs e) {
			if (Response != null) {
				Response(this, e);
			}
		}

		public void Delete(string title) {
			stack.Children.Remove(ItemDicionary[title]);
			ItemDicionary.Remove(title);

			if (ContainerType == ListType.Season) {
				SeasonData data = Data.DictSeason[title];
				Data.DictArchive[data.ArchiveTitle].SeasonTitle = null;

				TableSeason.Remove(title);
				Data.DictSeason.Remove(title);
			} else {
				Data.DictArchive.Remove(title);
			}

			RefreshContainer();
		}

		public void RefreshListEpisode(string title, int episode) {
			if (ContainerType == ListType.Season) {
				string arcTitle = Data.DictSeason[title].ArchiveTitle;
				Data.DictArchive[arcTitle].Episode = episode;
				ItemDicionary[title].Episode = episode;
			} else {
				Data.DictArchive[title].Episode = episode;
				ItemDicionary[title].Episode = episode;
			}
		}

		public void RefreshContainer() {
			if (ItemDicionary.Count == 0) {
				this.Visibility = Visibility.Collapsed;
				return;
			}

			this.Visibility = Visibility.Visible;
			this.stack.Children.Clear();

			if (ContainerType == ListType.Season) {
				List<SeasonData> list = TableSeason.Values.ToList();
				list.Sort();

				foreach (SeasonData data in list) {
					stack.Children.Add(ItemDicionary[data.Title]);
				}
			} else {
				List<ArchiveData> list = Data.DictArchive.Values.ToList();
				list.Sort();

				foreach (ArchiveData data in list) {
					if (Status.NowSelect == Status.SelectMode.Unfinished && data.Episode < 0) {
						continue;
					}

					stack.Children.Add(ItemDicionary[data.Title]);
				}
			}
		}

		public int GetContainerHeight() {
			if (ItemDicionary.Count == 0) {
				return 0;
			}
			return (ItemDicionary.Count + 1) * 40;
		}
	}
}
