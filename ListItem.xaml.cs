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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Simplist3 {
	/// <summary>
	/// ListItem.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class ListItem : UserControl {
		enum ItemType { Season, Archive, Anitable, Download };
		ItemType IType = ItemType.Season;

		public ListItem() { }

		public ListItem(bool animate) {
			InitializeComponent();

			if (animate) {
				AnimateItem();
			} else {
				this.Opacity = 1;
			}
		}

		public ListItem(SeasonData data, bool animate)
			: this(animate) {

			this.Title = data.Title;
			this.Time = data.TimeString;

			this.Episode = Data.DictArchive[data.ArchiveTitle].Episode;
			this.ArchiveTitle = data.ArchiveTitle;
		}

		public ListItem(ArchiveData data, bool animate)
			: this(animate) {

			this.textTime.Visibility = Visibility.Collapsed;
			this.buttonModify.Visibility = Visibility.Collapsed;

			this.Title = data.Title;
			this.textTitle.Margin = new Thickness(15, 0, 0, 0);
			Grid.SetColumn(this.textTitle, 0);
			Grid.SetColumnSpan(this.textTitle, 2);

			Grid.SetColumn(this.rect, 0);
			this.rect.Margin = new Thickness(15, 0, 0, 0);

			Grid.SetColumn(this.buttonMain, 0);
			Grid.SetColumnSpan(this.buttonMain, 2);

			this.Episode = data.Episode;
			this.ArchiveTitle = data.Title;

			IType = ItemType.Archive;
		}

		public ListItem(ListData data, bool isTable)
			: this(false) {

			this.buttonModify.Visibility = Visibility.Collapsed;
			this.gridEpisodeBox.Visibility = Visibility.Collapsed;

			this.Title = data.Title;

			if (isTable) {
				IType = ItemType.Anitable;

				this.Time = data.Time;
				this.ID = data.ID;
				this.Tag = data.Memo;

				Grid.SetColumnSpan(this.textTitle, 2);
				Grid.SetColumn(this.buttonMain, 0);
				Grid.SetColumnSpan(this.buttonMain, 3);
			} else {
				IType = ItemType.Download;

				this.Tag = data.Memo;
				this.ID = data.Url;
				this.textTime.Visibility = Visibility.Collapsed;

				this.textTitle.Margin = new Thickness(15, 0, 0, 0);
				this.rect.Margin = new Thickness(15, 0, 0, 0);

				Grid.SetColumn(this.rect, 0);
				Grid.SetColumn(this.textTitle, 0);
				Grid.SetColumnSpan(this.textTitle, 3);
			}
			Grid.SetColumn(this.buttonMain, 0);
			Grid.SetColumnSpan(this.buttonMain, 3);
		}

		Point MouseDownPoint;
		private void GridMouseDown(object sender, MouseButtonEventArgs e) {
			MouseDownPoint = e.GetPosition(this);

			if (e.MiddleButton == MouseButtonState.Pressed) {
				AnimateButton();
				if (IType == ItemType.Season || IType == ItemType.Archive) {
					Response(this, new CustomButtonEventArgs("CopyClipboard", this.Title, IType.ToString()));
				}
			}

			if (e.RightButton == MouseButtonState.Pressed) {
				AnimateButton();
				if (Response != null) {
					if (Status.IsRoot && IType == ItemType.Season) {
						Response(this, new CustomButtonEventArgs("OpenFolder", this.Title, this.ArchiveTitle));
					} else {
						Response(this, new CustomButtonEventArgs("Modify", this.Title, IType.ToString()));
					}
				}
			}
		}

		public event EventHandler<CustomButtonEventArgs> Response;
		public event EventHandler<CustomButtonEventArgs> DownloadResponse;
		private void Button_Click(object sender, RoutedEventArgs e) {
			AnimateButton();
			if (IType == ItemType.Download) {
				if (DownloadResponse != null) {
					DownloadResponse(this, new CustomButtonEventArgs(this.Tag, this.title, this.ID));
				}
			}

			if (Response != null) {
				if (IType == ItemType.Anitable) {
					Response(this, new CustomButtonEventArgs("Click", this.Title, this.Time));
				} else {
					Response(this, new CustomButtonEventArgs("Click", this.Title, IType.ToString()));
				}
			}
		}
		private void Button_Click2(object sender, RoutedEventArgs e) {
			AnimateButton();
			if (Response != null) {
				Response(this, new CustomButtonEventArgs("Modify", this.Title, IType.ToString()));
			}
		}

		private void ImageButton_Response(object sender, CustomButtonEventArgs e) {
			try {
				int delta = Convert.ToInt32(e.Main);
				if (Response != null) {
					Response(this, new CustomButtonEventArgs("Episode", this.ArchiveTitle, delta.ToString()));
				}
			} catch { return; }
		}

		private void AnimateItem() {
			Storyboard sb = new Storyboard();

			DoubleAnimation opacity = new DoubleAnimation(
				1, TimeSpan.FromMilliseconds(350)) {
					BeginTime = TimeSpan.FromMilliseconds(550),
				};
			Storyboard.SetTarget(opacity, this);
			Storyboard.SetTargetProperty(opacity, new PropertyPath(FrameworkElement.OpacityProperty));

			ThicknessAnimation margin = new ThicknessAnimation(
				new Thickness(150, 0, 15, 0),
				new Thickness(0, 0, 15, 0),
				TimeSpan.FromMilliseconds(350)) {
					BeginTime = TimeSpan.FromMilliseconds(500),
					EasingFunction = new ExponentialEase() {
						Exponent = 5,
						EasingMode = EasingMode.EaseOut
					}
				};
			Storyboard.SetTarget(margin, this);
			Storyboard.SetTargetProperty(margin, new PropertyPath(FrameworkElement.MarginProperty));

			sb.Children.Add(opacity);
			sb.Children.Add(margin);

			sb.Begin(this);
		}
		private void AnimateButton() {
			if (MouseDownPoint == null) { return; }

			Storyboard sb = new Storyboard();

			circle.Margin = new Thickness(MouseDownPoint.X - 150, 0, 0, 0);
			circle.RenderTransformOrigin = new Point(0.5, 0.5);
			circle.RenderTransform = new ScaleTransform(0.5, 1);

			DoubleAnimation opacity = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(250));
			Storyboard.SetTarget(opacity, circle);
			Storyboard.SetTargetProperty(opacity, new PropertyPath(Ellipse.OpacityProperty));

			DoubleAnimation scalex = new DoubleAnimation(0.5, 1, TimeSpan.FromMilliseconds(250));
			Storyboard.SetTarget(scalex, circle);
			Storyboard.SetTargetProperty(scalex, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));

			sb.Children.Add(opacity);
			sb.Children.Add(scalex);

			sb.Begin(this);
		}

		string time = "";
		public string Time {
			get {
				return time;
			}
			set {
				time = value;
				textTime.Text = string.Format("{0:D2}:{1:D2}"
					, time.Substring(0, 2), time.Substring(2, 2));
			}
		}

		string title = "";
		public string Title {
			get {
				return title;
			}
			set {
				title = value;
				this.textTitle.Text = title;
			}
		}
		private string ArchiveTitle { get; set; }

		public string ID { get; set; }
		public string Tag { get; set; }

		int episode = 0;
		public int Episode {
			get {
				return episode;
			}
			set {
				episode = value;
				RefreshEpisode(episode);
			}
		}

		public void RefreshEpisode(int value) {
			if (value < 0) {
				textTitle.Opacity = 0.3;
				gridEpisodeBox.Visibility = Visibility.Collapsed;
			} else {
				textTitle.Opacity = 1;
				gridEpisodeBox.Visibility = Visibility.Visible;
			}

			textEpisode.Text = value.ToString();
		}
	}
}
