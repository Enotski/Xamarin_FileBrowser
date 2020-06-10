using System.Drawing;

namespace xFileBrowser.Models {
	public class DirectoryItem {
		public string FullPath { get; set; }
		public string Name { get; set; }
		public string ItemInfo { get; set; }
		public bool IsFolder { get; set; }
		public string Icon { get; set; }
		public Color IconColor { get; set; }
	}
}
