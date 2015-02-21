using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Simplist3 {
	public partial class MainWindow : Window {
		public System.Windows.Forms.NotifyIcon tray = new System.Windows.Forms.NotifyIcon();
		private void InitTray() {
			tray.Visible = true;

			tray.Icon = System.Drawing.Icon.FromHandle(Simplist3.Properties.Resources.tray.Handle);
			tray.Text = "Simplist3";

			System.Windows.Forms.ContextMenuStrip ctxt = new System.Windows.Forms.ContextMenuStrip();
			System.Windows.Forms.ToolStripMenuItem copen = new System.Windows.Forms.ToolStripMenuItem("열기");
			System.Windows.Forms.ToolStripMenuItem cshutdown = new System.Windows.Forms.ToolStripMenuItem("종료");

			tray.MouseDoubleClick += delegate(object sender, System.Windows.Forms.MouseEventArgs e) { ActivateMe(); };
			copen.Click += delegate(object sender, EventArgs e) { ActivateMe(); };
			cshutdown.Click += delegate(object sender, EventArgs e) { this.Close(); };

			ctxt.Items.Add(copen);
			ctxt.Items.Add(cshutdown);
			tray.ContextMenuStrip = ctxt;
		}
	}
}
