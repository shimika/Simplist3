using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Simplist3 {
	public partial class MainWindow : Window {
		private void GlobalPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
			switch (e.Key) {
				case Key.Escape:
					if (Setting.Tray) {
						this.Opacity = 0;
						Notice("", false, 0);
						new AltTab().HideAltTab(this);
					} else {
						this.Close();
					}
					break;

				case Key.Enter:
					if (Mode == ShowMode.Add || Mode == ShowMode.Modify) {
						AddOrModifyItem();
					}
					break;
			}
		}
	}
}
