using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace xFileBrowser.Resources {
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DarkTheme : ResourceDictionary {
		public static Dictionary<string, Color> themeColors = new Dictionary<string, Xamarin.Forms.Color> {
			{"PageBackgroundColor", Color.FromHex("#2b2b2b") },
			{"PrimaryColor", Color.FromHex("#555555") },
			{"SecondaryColor", Color.FromHex("#404040") },
			{"PrimaryTextColor", Color.FromHex("#ebebeb") },
			{"SecondaryTextColor", Color.FromHex("#b8b8b8") },
			{"DisabledTextColor", Color.FromHex("#999") },
			{"PrimaryGoldColor", Color.FromHex("#e8a600") },
			{"DangerColor", Color.FromHex("#c40f02") },
			{"TransparentColor", Color.Transparent },
		};
		public DarkTheme() {
			InitializeComponent();
			foreach (var item in themeColors)
				this.Add(item.Key, item.Value);
		}
	}
}