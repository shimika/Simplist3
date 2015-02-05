using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace Simplist3 {
	class Animation {
		public static ThicknessAnimation GetThicknessAnimation(
			double duration, double left, double top, 
			FrameworkElement fe = null, 
			double right = 0, double bottom = 0, double delay = 0, double exponent = 5) {

			ThicknessAnimation ta = new ThicknessAnimation(
					new Thickness(left, top, right, bottom),
					TimeSpan.FromMilliseconds(duration)) {
						EasingFunction = new ExponentialEase() {
							Exponent = exponent, EasingMode = EasingMode.EaseOut,
						},
						BeginTime = TimeSpan.FromMilliseconds(delay)
					};

			if (fe != null) {
				Storyboard.SetTarget(ta, fe);
				Storyboard.SetTargetProperty(ta, new PropertyPath(FrameworkElement.MarginProperty));
			}

			return ta;
		}
		public static DoubleAnimation GetDoubleAnimation(double opacity, FrameworkElement fe, double duration = 250, double delay = 0) {
			DoubleAnimation da = new DoubleAnimation(opacity, TimeSpan.FromMilliseconds(duration)) {
				BeginTime = TimeSpan.FromMilliseconds(delay),
			};
			Storyboard.SetTarget(da, fe);
			Storyboard.SetTargetProperty(da, new PropertyPath(FrameworkElement.OpacityProperty));

			return da;
		}
	}
}
