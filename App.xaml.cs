using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Shell;

namespace Simplist3 {
	/// <summary>
	/// App.xaml에 대한 상호 작용 논리
	/// </summary>


	public partial class App : Application, ISingleInstanceApp {
		[STAThread]
		public static void Main() {
			if (SingleInstance<App>.InitializeAsFirstInstance("Simplist3")) {
				App application = new App();

				application.Init();
				application.Run();

				// Allow single instance code to perform cleanup operations
				SingleInstance<App>.Cleanup();
			}
		}

		public void Init() {
			this.InitializeComponent();
		}

		public bool SignalExternalCommandLineArgs(IList<string> args) {
			try {
				return ((MainWindow)MainWindow).ProcessCommandLineArgs(args);
			} catch (Exception e) {
				return false;
			}
		}
	}
}
