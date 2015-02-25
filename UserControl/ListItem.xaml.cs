﻿using System;
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
			this.Reference = data.Title;
		}

		public ListItem(Listdata data)
			: this(false) {

			this.buttonModify.Visibility = Visibility.Collapsed;
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
						this.textTitle.Foreground 
							= FindResource("PrimaryBrush") as SolidColorBrush;
					}

					break;
			}

			Grid.SetColumn(this.buttonMain, 0);
			Grid.SetColumnSpan(this.buttonMain, 3);

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

		Point MouseDownPoint;
		private void GridMouseDown(object sender, MouseButtonEventArgs e) {
			MouseDownPoint = e.GetPosition(this);

			if (e.MiddleButton == MouseButtonState.Pressed) {
				AnimateButton();
				if (ObjectType == ItemType.Season || ObjectType == ItemType.Archive) {
					Response(this, new CustomButtonEventArgs("CopyClipboard", this.Title, ObjectType.ToString()));
				}
			}

			if (e.RightButton == MouseButtonState.Pressed) {
				AnimateButton();
				if (Response != null) {
					if (Status.Root && ObjectType == ItemType.Season) {
						Response(this, new CustomButtonEventArgs("OpenFolder", this.Title, this.Reference));
					} else if (ObjectType == ItemType.Season || ObjectType == ItemType.Archive) {
						Response(this, new CustomButtonEventArgs("Modify", this.Title, ObjectType.ToString()));
					}
				}
			}
		}

		public event EventHandler<CustomButtonEventArgs> Response;
		private void MainButton_Click(object sender, RoutedEventArgs e) {
			AnimateButton();

			if (Response != null) {
				if (ObjectType == ItemType.Listitem) {
					Response(this, new CustomButtonEventArgs(this.Type, this.Title, this.Reference));
				} else {
					Response(this, new CustomButtonEventArgs("Click", this.Title, ObjectType.ToString()));
				}
			}
		}
		private void TimeButton_Click(object sender, RoutedEventArgs e) {
			if (ObjectType == ItemType.Listitem) { return; }

			AnimateButton();
			if (Response != null) {
				Response(this, new CustomButtonEventArgs("Modify", this.Title, ObjectType.ToString()));
			}
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
						Grid.SetColumnSpan(this.buttonMain, 2);
						Grid.SetColumnSpan(this.textTitle, 2);
					}
				} else {
					textTitle.Opacity = 1;
					gridEpisodeBox.Visibility = Visibility.Visible;

					if (this.ObjectType == ItemType.Season) {
						Grid.SetColumnSpan(this.buttonMain, 1);
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

			circle.Margin = new Thickness(MouseDownPoint.X, MouseDownPoint.Y, 0, 0);
			circle.RenderTransformOrigin = new Point(0.5, 0.5);
			circle.RenderTransform = new ScaleTransform(0.2, 0.2);

			DoubleAnimation opon = Animation.GetDoubleAnimation(1, this.circle, 150);
			DoubleAnimation rton = Animation.GetDoubleAnimation(1, this.fill, 150);

			DoubleAnimation opoff = Animation.GetDoubleAnimation(0, this.circle, 300, 100);
			DoubleAnimation rtoff = Animation.GetDoubleAnimation(0, this.fill, 300, 100);

			DoubleAnimation scalex = new DoubleAnimation(0.2, 0.7, TimeSpan.FromMilliseconds(450));
			DoubleAnimation scaley = new DoubleAnimation(0.2, 0.7, TimeSpan.FromMilliseconds(450));
			Storyboard.SetTarget(scalex, circle);
			Storyboard.SetTarget(scaley, circle);
			scalex.EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut };
			scaley.EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut };
			Storyboard.SetTargetProperty(scalex, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
			Storyboard.SetTargetProperty(scaley, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));

			sb.Children.Add(opon); sb.Children.Add(opoff);
			sb.Children.Add(rton); sb.Children.Add(rtoff);
			sb.Children.Add(scalex); sb.Children.Add(scaley);

			sb.Begin(this);
		}

	}
}