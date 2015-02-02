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
	/// TabButton.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class TabButton : UserControl {
		public TabButton() {
			InitializeComponent();

			this.RenderTransformOrigin = new Point(0.5, 0.5);
		}

		public string Type {
			get;
			set;
		}

		#region Source
		public string Source {
			get { return (string)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}
		public static readonly DependencyProperty SourceProperty
			= DependencyProperty.Register("Source", typeof(string), typeof(TabButton),
			new FrameworkPropertyMetadata(
				null,
				FrameworkPropertyMetadataOptions.AffectsRender,
				SourcePropertyChanged));

		private static void SourcePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			TabButton button = obj as TabButton;

			string uri = string.Format("pack://application:,,,/Simplist3;component/{0}", e.NewValue);
			button.image.Source = new BitmapImage(new Uri(uri));
		}
		#endregion

		#region Text
		public string Text {
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}
		public static readonly DependencyProperty TextProperty
			= DependencyProperty.Register("Text", typeof(string), typeof(TabButton),
			new FrameworkPropertyMetadata(
				null,
				FrameworkPropertyMetadataOptions.AffectsRender,
				TextPropertyChanged));

		private static void TextPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			TabButton button = obj as TabButton;
			button.text.Text = e.NewValue.ToString();
		}
		#endregion

		#region ViewMode
		public enum Mode { Focused, Clickable, Hidden };

		public Mode ViewMode {
			get { return (Mode)GetValue(EnableProperty); }
			set { SetValue(EnableProperty, value); }
		}
		public static readonly DependencyProperty EnableProperty
			= DependencyProperty.Register("ViewMode", typeof(Mode), typeof(TabButton),
			new FrameworkPropertyMetadata(
				Mode.Clickable,
				FrameworkPropertyMetadataOptions.AffectsRender,
				EnablePropertyChanged));

		private static void EnablePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			TabButton button = obj as TabButton;
			button.AnimateControl((Mode)e.OldValue, (Mode)e.NewValue);
		}
		#endregion

		#region Size
		public double Size {
			get { return (double)GetValue(SizeProperty); }
			set {
				SetValue(SizeProperty, value);
			}
		}
		public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
			"Size",
			typeof(double),
			typeof(TabButton),
			new FrameworkPropertyMetadata(
				Double.NaN,
				FrameworkPropertyMetadataOptions.AffectsRender,
				SizePropertyChanged));

		private static void SizePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			(obj as TabButton).Width = (double)e.NewValue;
			(obj as TabButton).circle.Width = (double)e.NewValue;
			(obj as TabButton).circle.Height = (double)e.NewValue;
		}
		#endregion

		public event EventHandler<CustomButtonEventArgs> Response;
		private void Button_Click(object sender, RoutedEventArgs e) {
			if (ViewMode == Mode.Focused || ViewMode == Mode.Hidden) { return; }

			if (this.Text == null) {
				AnimateCircle();
			}

			if (Response != null) {
				Response(this, new CustomButtonEventArgs("Click", Type, ""));
			}
		}

		private void AnimateCircle() {
			Storyboard sb = new Storyboard();

			circle.RenderTransformOrigin = new Point(0.5, 0.5);
			circle.RenderTransform = new ScaleTransform(0.2, 1);

			DoubleAnimation opacity = new DoubleAnimation(0.7, 0, TimeSpan.FromMilliseconds(200));
			Storyboard.SetTarget(opacity, circle);
			Storyboard.SetTargetProperty(opacity, new PropertyPath(Ellipse.OpacityProperty));

			DoubleAnimation scalex = new DoubleAnimation(0.5, 1, TimeSpan.FromMilliseconds(200));
			Storyboard.SetTarget(scalex, circle);
			Storyboard.SetTargetProperty(scalex, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));

			sb.Children.Add(opacity);
			sb.Children.Add(scalex);

			sb.Begin(this);
		}

		private void AnimateControl(Mode o, Mode n) {
			Storyboard sb = new Storyboard();

			double from = o == Mode.Hidden ? 0 : 1;
			double to = n == Mode.Hidden ? 0 : 1;

			double op = 1;
			this.IsHitTestVisible = false;

			switch (n) {
				case Mode.Clickable:
					op = 0.5;
					this.IsHitTestVisible = true;
					break;
				case Mode.Hidden:
					op = 0;
					break;
			}

			this.RenderTransform = new ScaleTransform(from, from);

			DoubleAnimation opacity = new DoubleAnimation(op, TimeSpan.FromMilliseconds(250));
			DoubleAnimation scalex = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(250)) {
				EasingFunction = new CubicEase() {
					EasingMode = EasingMode.EaseOut,
				}
			};
			DoubleAnimation scaley = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(250)) {
				EasingFunction = new CubicEase() {
					EasingMode = EasingMode.EaseOut,
				}
			};

			Storyboard.SetTarget(opacity, this);
			Storyboard.SetTarget(scalex, this);
			Storyboard.SetTarget(scaley, this);

			Storyboard.SetTargetProperty(opacity, new PropertyPath(UIElement.OpacityProperty));
			Storyboard.SetTargetProperty(scalex, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
			Storyboard.SetTargetProperty(scaley, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));

			sb.Children.Add(opacity);
			sb.Children.Add(scalex);
			sb.Children.Add(scaley);

			sb.Begin(this);
		}

		public void StartAnimateImage() {
			Storyboard sb = new Storyboard() {
				RepeatBehavior = RepeatBehavior.Forever,
			};

			image.RenderTransformOrigin = new Point(0.5, 0.5);
			image.RenderTransform = new RotateTransform(0);

			DoubleAnimation rotate = new DoubleAnimation(0, -360, TimeSpan.FromMilliseconds(3000));
			Storyboard.SetTarget(rotate, image);
			Storyboard.SetTargetProperty(rotate, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));

			sb.Children.Add(rotate);

			sb.Begin(this, true);
		}
	}
}
