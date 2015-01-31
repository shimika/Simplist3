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
	/// NotiItem.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class NotiItem : UserControl {
		public NotiItem(Listdata data) {
			InitializeComponent();

			this.Data = data;

			this.textTitle.Text = data.Type;
			this.textEpisode.Text = data.Tag;
			this.textMaker.Text = data.Title;
			this.textTime.Text = data.Time;
		}

		Point MouseDownPoint;
		Listdata Data { get; set; }

		private void GridMouseDown(object sender, MouseButtonEventArgs e) {
			MouseDownPoint = e.GetPosition(this);
		}

		public event EventHandler<CustomButtonEventArgs> Response;
		private void Button_Click(object sender, RoutedEventArgs e) {
			AnimateButton();
			if (Response != null) {
				Response(this, new CustomButtonEventArgs("Click", this.Data.Type, this.Data.Url));
			}
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
	}
}
