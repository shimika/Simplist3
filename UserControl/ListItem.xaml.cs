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
		enum ItemType { Season, Archive, Listitem };
		ItemType ObjectType = ItemType.Season;

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
			this.Reference = data.ArchiveTitle;
		}

		public ListItem(ArchiveData data, bool animate)
			: this(animate) {

			ObjectType = ItemType.Archive;

			this.textTime.Visibility = Visibility.Collapsed;

			this.Title = data.Title;
			this.textTitle.Margin = new Thickness(15, 0, 0, 0);
			Grid.SetColumn(this.textTitle, 0);
			Grid.SetColumnSpan(this.textTitle, 2);

			Grid.SetColumn(this.rect, 0);
			this.rect.Margin = new Thickness(15, 0, 0, 0);

			this.Episode = data.Episode;
			this.Reference = data.Title;
		}

		public ListItem(Listdata data)
			: this(false) {

			this.gridEpisodeBox.Visibility = Visibility.Collapsed;

			this.Title = data.Title;
			this.textTitle.FontSize = 14;
			this.Type = data.Type;
			ObjectType = ItemType.Listitem;

			if (this.Type == "Torrent" || this.Type == "Zip") {
				this.ToolTip = this.title;
			}

			switch (data.Type) {
				case "Anitable":
					this.Time = data.Time;
					this.Reference = data.Time;

					Grid.SetColumnSpan(this.textTitle, 2);

					break;
				default:
					this.Reference = data.Url;

					this.textTime.Visibility = Visibility.Collapsed;
					this.textTitle.Margin = new Thickness(15, 0, 0, 0);
					this.rect.Margin = new Thickness(15, 0, 0, 0);

					Grid.SetColumn(this.rect, 0);
					Grid.SetColumn(this.textTitle, 0);
					Grid.SetColumnSpan(this.textTitle, 3);

					if (data.Raw) {
						SetTitleColor(this.title);

						if (!Setting.ShowRaws) {
							this.textTitle.FontWeight = FontWeights.SemiBold;
						}
					}
					else if (this.Type == "Torrent") {
						this.textTitle.Foreground = Brushes.Gray;
						this.textTitle.FontWeight = FontWeights.Normal;
					}

					break;
			}

			string sitetype = "DefaultTag";
			switch (data.SiteType) {
				case 0: break;
				case 1:
					sitetype = "NaverTag";
					break;
				case 2:
					sitetype = "TistoryTag";
					break;
				case 3:
					sitetype = "EgloosTag";
					break;
				case 4:
					sitetype = "Fc2Tag";
					break;
				default:
					return;
			}

			try {
				sitetag.Fill = FindResource(sitetype) as SolidColorBrush;
				sitetag.Visibility = Visibility.Visible;
			} catch { }
		}

		private string[] RawsList = { "[Ohys-Raws]", "[Leopard-Raws]", "[Zero-Raws]", "[HorribleSubs]" };
		private void SetTitleColor(string title) {
			string r = "";
			string t = title;

			foreach (string raws in RawsList) {
				if (title.IndexOf(raws) >= 0) {
					t = title.Replace(raws, "");
					r = raws.Replace("-Raws", "");
				}
			}

			if (r == "") {
				//this.textTitle.Foreground = FindResource("PrimaryBrush") as SolidColorBrush;
				this.textTitle.Foreground = Brushes.Black;
			}
			else {
				this.textTitle.Text = "";
				this.textTitle.Inlines.Add(new Run(r) {
					Foreground = FindResource("PrimaryBrush") as SolidColorBrush,
					FontWeight = FontWeights.SemiBold,
				});
				this.textTitle.Inlines.Add(t);
			}
		}

		Point MouseDownPoint;
		int mousedown = -1;
		public event EventHandler<CustomButtonEventArgs> Response;

		private void GridMouseDown(object sender, MouseButtonEventArgs e) {
			MouseDownPoint = e.GetPosition(this);

			if (e.LeftButton == MouseButtonState.Pressed) {
				int p = -1;
				if (gridEpisodeBox.Visibility == System.Windows.Visibility.Visible) {
					p = (int)e.GetPosition(gridEpisodeBox).X;
				}

				if (p >= 0) { return; }

				mousedown = 0;
			}

			if (e.MiddleButton == MouseButtonState.Pressed) {
				mousedown = 1;
			}

			if (e.RightButton == MouseButtonState.Pressed) {
				mousedown = 2;
			}

			(sender as UIElement).CaptureMouse();
			AnimateButtonOn();
		}

		private void Grid_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
			(sender as UIElement).ReleaseMouseCapture();
			AnimateButtonOff();

			Point p = e.GetPosition(this);
			if (Math.Max(Math.Abs(MouseDownPoint.X - p.X), Math.Abs(MouseDownPoint.Y - p.Y)) <= 3 && Response != null) {
				switch (mousedown) {
					case 0:
						if (textTime.Visibility == Visibility.Visible && MouseDownPoint.X <= 70) {
							Response(this, new CustomButtonEventArgs("Modify", this.Title, ObjectType.ToString()));
						}
						else {
							if (ObjectType == ItemType.Listitem) {
								Response(this, new CustomButtonEventArgs(this.Type, this.Title, this.Reference));
							}
							else {
								Response(this, new CustomButtonEventArgs("Click", this.Title, ObjectType.ToString()));
							}
						}
						break;
					case 1:
						if (ObjectType == ItemType.Season || ObjectType == ItemType.Archive) {
							Response(this, new CustomButtonEventArgs("CopyClipboard", this.Title, ObjectType.ToString()));
						}
						break;
					case 2:
						if (Status.Root && ObjectType == ItemType.Season) {
							Response(this, new CustomButtonEventArgs("OpenFolder", this.Title, this.Reference));
						}
						else if (ObjectType == ItemType.Season || ObjectType == ItemType.Archive) {
							Response(this, new CustomButtonEventArgs("Modify", this.Title, ObjectType.ToString()));
						}
						break;
				}
			}

			mousedown = -1;
		}

		private void EpisodeButton_Response(object sender, CustomButtonEventArgs e) {
			try {
				int delta = Convert.ToInt32(e.Main);
				if (Response != null) {
					Response(this, new CustomButtonEventArgs("Episode", this.Reference, delta.ToString()));
				}
			} catch { return; }
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

		int episode = 0;
		public int Episode {
			get {
				return episode;
			}
			set {
				episode = value;

				if (episode < 0) {
					textTitle.Opacity = 0.3;
					gridEpisodeBox.Visibility = Visibility.Collapsed;

					if (this.ObjectType == ItemType.Season) {
						Grid.SetColumnSpan(this.textTitle, 2);
					}
				} else {
					textTitle.Opacity = 1;
					gridEpisodeBox.Visibility = Visibility.Visible;

					if (this.ObjectType == ItemType.Season) {
						Grid.SetColumnSpan(this.textTitle, 1);
					}
				}

				textEpisode.Text = episode.ToString();
			}
		}

		public string Reference { get; set; }
		public string Type { get; set; }

		private void AnimateItem() {
			Storyboard sb = new Storyboard();

			DoubleAnimation opacity = new DoubleAnimation(
				1, TimeSpan.FromMilliseconds(350)) {
					BeginTime = TimeSpan.FromMilliseconds(550),
				};
			Storyboard.SetTarget(opacity, this);
			Storyboard.SetTargetProperty(opacity, new PropertyPath(FrameworkElement.OpacityProperty));

			sb.Children.Add(opacity);
			//sb.Children.Add(margin);

			sb.Begin(this);
		}

		private void AnimateButtonOn() {
			if (MouseDownPoint == null) { return; }

			//MessageBox.Show("?");

			Storyboard sb = new Storyboard();

			circle.Margin = new Thickness(MouseDownPoint.X, MouseDownPoint.Y, 0, 0);
			circle.RenderTransformOrigin = new Point(0.5, 0.5);
			circle.RenderTransform = new ScaleTransform(0.2, 0.2);

			circle.Opacity = 0.5;
			DoubleAnimation opon = Animation.GetDoubleAnimation(1, circle, 100);
			DoubleAnimation rton = Animation.GetDoubleAnimation(1, fill, 150);

			DoubleAnimation scalex = new DoubleAnimation(8, TimeSpan.FromMilliseconds(3000));
			DoubleAnimation scaley = new DoubleAnimation(8, TimeSpan.FromMilliseconds(3000));
			Storyboard.SetTarget(scalex, circle);
			Storyboard.SetTarget(scaley, circle);
			//scalex.EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut };
			//scaley.EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut };
			Storyboard.SetTargetProperty(scalex, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
			Storyboard.SetTargetProperty(scaley, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));

			sb.Children.Add(opon);
			sb.Children.Add(rton);
			sb.Children.Add(scalex);
			sb.Children.Add(scaley);

			sb.Begin(this);
		}

		private void AnimateButtonOff() {
			if (MouseDownPoint == null) { return; }

			Storyboard sb = new Storyboard();

			//circle.Margin = new Thickness(MouseDownPoint.X, MouseDownPoint.Y, 0, 0);
			//circle.RenderTransformOrigin = new Point(0.5, 0.5);
			//circle.RenderTransform = new ScaleTransform(0.1, 0.1);

			this.rect.Opacity = 1;

			DoubleAnimation opoff = Animation.GetDoubleAnimation(0, this.circle, 400);
			DoubleAnimation rtoff = Animation.GetDoubleAnimation(0, this.fill, 200);

			DoubleAnimation scalex = new DoubleAnimation(8, TimeSpan.FromMilliseconds(1500));
			DoubleAnimation scaley = new DoubleAnimation(8, TimeSpan.FromMilliseconds(1500));
			Storyboard.SetTarget(scalex, circle);
			Storyboard.SetTarget(scaley, circle);
			scalex.EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut };
			scaley.EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut };
			Storyboard.SetTargetProperty(scalex, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
			Storyboard.SetTargetProperty(scaley, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));

			sb.Children.Add(opoff);
			sb.Children.Add(rtoff);
			sb.Children.Add(scalex);
			sb.Children.Add(scaley);

			sb.Begin(this);
		}
		/*
		private void AnimateButton(bool init) {
			if (MouseDownPoint == null) {
				return;
			}

			Storyboard sb = new Storyboard();

			circle.Margin = new Thickness(MouseDownPoint.X, MouseDownPoint.Y, 0, 0);
			circle.RenderTransformOrigin = new Point(0.5, 0.5);

			if (init) {
				circle.RenderTransform = new ScaleTransform(0.2, 0.2);
			}

			DoubleAnimation opon = Animation.GetDoubleAnimation(1, this.circle, 150);
			DoubleAnimation rton = Animation.GetDoubleAnimation(1, this.fill, 250);

			DoubleAnimation opoff = Animation.GetDoubleAnimation(0, this.circle, 300, 100);
			DoubleAnimation rtoff = Animation.GetDoubleAnimation(0, this.fill, 300, 200);

			DoubleAnimation scalex = new DoubleAnimation(1, TimeSpan.FromMilliseconds(2450));
			DoubleAnimation scaley = new DoubleAnimation(1, TimeSpan.FromMilliseconds(2450));
			Storyboard.SetTarget(scalex, circle);
			Storyboard.SetTarget(scaley, circle);
			//scalex.EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut };
			//scaley.EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut };
			Storyboard.SetTargetProperty(scalex, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
			Storyboard.SetTargetProperty(scaley, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));

			sb.Children.Add(opon); sb.Children.Add(opoff);
			sb.Children.Add(rton); sb.Children.Add(rtoff);
			sb.Children.Add(scalex); sb.Children.Add(scaley);

			sb.Begin(this);
		}

		private void AnimatePressButton() {
			if (MouseDownPoint == null) { return; }

			Storyboard sb = new Storyboard();

			circle.Margin = new Thickness(MouseDownPoint.X, MouseDownPoint.Y, 0, 0);
			circle.RenderTransformOrigin = new Point(0.5, 0.5);
			circle.RenderTransform = new ScaleTransform(0.2, 0.2);

			DoubleAnimation opon = Animation.GetDoubleAnimation(0.4, this.circle, 150);

			DoubleAnimation scalex = new DoubleAnimation(0.5, TimeSpan.FromMilliseconds(450));
			DoubleAnimation scaley = new DoubleAnimation(0.5, TimeSpan.FromMilliseconds(450));
			Storyboard.SetTarget(scalex, circle);
			Storyboard.SetTarget(scaley, circle);
			scalex.EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut };
			scaley.EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut };
			Storyboard.SetTargetProperty(scalex, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
			Storyboard.SetTargetProperty(scaley, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));

			sb.Children.Add(opon);
			sb.Children.Add(scalex);
			sb.Children.Add(scaley);

			sb.Begin(this);
		}
		 */ 
	}
}
