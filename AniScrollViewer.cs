using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Simplist3 {
	class AniScrollViewer : ScrollViewer {
		public static readonly DependencyProperty CurrentVerticalOffsetProperty =
			DependencyProperty.Register("CurrentVerticalOffset", typeof(double), typeof(AniScrollViewer),
			new PropertyMetadata(new PropertyChangedCallback(OnVerticalChanged)));

		public double CurrentVerticalOffset {
			get { return (double)GetValue(CurrentVerticalOffsetProperty); }
			set { SetValue(CurrentVerticalOffsetProperty, value); }
		}

		private static void OnVerticalChanged(DependencyObject property, DependencyPropertyChangedEventArgs e) {
			AniScrollViewer viewer = property as AniScrollViewer;
			Console.WriteLine((double)e.NewValue);
			viewer.ScrollToVerticalOffset((double)e.NewValue);

		}

		/*
		protected override void OnMouseWheel(System.Windows.Input.MouseWheelEventArgs e) {
			double value = this.VerticalOffset;
			double cvalue = this.CurrentVerticalOffset;

			double newValue = Math.Max(value - e.Delta * 0.6, 0);

			this.BeginAnimation(AniScrollViewer.CurrentVerticalOffsetProperty, new DoubleAnimation(Math.Max(cvalue - e.Delta, 0), TimeSpan.FromMilliseconds(200)) {
				EasingFunction = new PowerEase() {
					Power = 5,
					EasingMode = EasingMode.EaseOut,
				},
			});
		}
		 */
	}
}
