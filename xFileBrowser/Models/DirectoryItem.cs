using System;
using System.ComponentModel;
using System.Drawing;

namespace xFileBrowser.Models {
	public class DirectoryItem : INotifyPropertyChanged {

		private bool itemChecked = false;

		public string FullPath { get; set; }
		public string Name { get; set; }
		public string ItemInfo { get; set; }
		public bool IsFolder { get; set; }
		public string Icon { get; set; }
		public Color IconColor { get; set; }
		public string FormattedSize { get; set; }
		public DateTime DateChange { get; set; }
		public string ReadOnly { get; set; }
		public string Hidden { get; set; }
		public string Archive { get; set; }
		public bool ItemChecked {
			set {
				if (itemChecked != value) {
					itemChecked = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ItemChecked"));
				}
			}
			get {
				return itemChecked;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
