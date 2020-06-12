using Xamarin.Forms;
using xFileBrowser.Views;

namespace xFileBrowser {
	public partial class App : Application {
		public App() {
			InitializeComponent();

			MainPage = new FileBrowserPage();
		}

		protected override void OnStart() {
		}

		protected override void OnSleep() {
			
		}

		protected override void OnResume() {
		}
	}
}
