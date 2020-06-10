using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xFileBrowser.Models;
using xFileBrowser.Resources;

namespace xFileBrowser.Views {
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class FileBrowserPage : ContentPage {
		public ObservableCollection<DirectoryItem> DirList { get; set; }
		public const string iconFolder = "\U000F0256";
		public const string iconFile = "\U000F0224";

		public FileBrowserPage() {
			InitializeComponent();
			GetDir("/storage");
			DirectoryList.ItemsSource = DirList;
		}
		private async void GetDir(string path) {
			try {
				if (File.Exists(path)) {
					DirectoryList.SelectedItem = null;
					return;
				}
				DirList = DirList ?? new ObservableCollection<DirectoryItem>();
				DirList.Clear();
				var dirInfo = new DirectoryInfo(path);
				var dirs = dirInfo.GetDirectories();
				var files = dirInfo.GetFiles();
				CurrFolderName.Text = dirInfo.Name == "0" ? Path.Combine(dirInfo.Parent.Name, dirInfo.Name) : dirInfo.Name;
				CurrFolderPath.Text = dirInfo.FullName.EndsWith("emulated") ? dirInfo.FullName + @"/0" : dirInfo.FullName;

				await Task.Run(() => {
					foreach (var dir in dirs) {
						if (dir.Name == "self")
							continue;
						var entriesCount = dir.Name == "emulated" ? Directory.GetFileSystemEntries(dir.FullName + @"/0").Count() : dir.EnumerateFileSystemInfos().Count();
						DirList.Add(new DirectoryItem {
							FullPath = dir.FullName.EndsWith("emulated") ? dir.FullName + @"/0" : dir.FullName,
							Name = dir.Name.EndsWith("emulated") ? dir.Name + @"/0" : dir.Name,
							Icon = iconFolder,
							ItemInfo = $"Objects - {entriesCount} | {dir.LastWriteTime}",
							IconColor = Color.FromHex("ebebeb")
						});
					}
					foreach (var file in files) {
						var found = Constns.fileApperanceDict.TryGetValue(file.Extension.ToLower(), out Constns.FileAppearance appearance);
						DirList.Add(new DirectoryItem {
							FullPath = file.FullName,
							Name = file.Name,
							Icon = found ? appearance.Icon : iconFile,
							ItemInfo = $"{Utilits.SizeSuffix(file.Length, 2)} | {file.LastWriteTime}",
							IconColor = found ? appearance.Color : Color.FromHex("#ebebeb"),
						});
					}
				});
			} catch (Exception ex) {
				return;
			}
		}

		private void OnListViewItemSelected(object sender, SelectedItemChangedEventArgs e) {
			if (e.SelectedItem != null)
				GetDir((e.SelectedItem as DirectoryItem).FullPath);
		}

		private void ButtonUp_Clicked(object sender, EventArgs e) {
			try {
				var dirs = CurrFolderPath.Text.Split(new[] { '/' }).ToList();
				if (dirs.Last() == "storage")
					return;
				dirs.RemoveAt(dirs.Count - 1);
				if (dirs.Count == 0)
					return;
				if (dirs.Last() == "emulated")
					dirs.RemoveAt(dirs.Count - 1);
				GetDir(Path.Combine(dirs.ToArray()));
			} catch (Exception ex) {
				return;
			}
		}
		protected override bool OnBackButtonPressed() {
			try {
				var dirs = CurrFolderPath.Text.Split(new[] { '/' }).ToList();
				if (dirs.Last() == "storage")
					return false;
				dirs.RemoveAt(dirs.Count - 1);
				if (dirs.Count == 0)
					return false;
				if (dirs.Last() == "emulated")
					dirs.RemoveAt(dirs.Count - 1);
				GetDir(Path.Combine(dirs.ToArray()));
				return true;
			} catch (Exception ex) {
				return true;
			}
		}
	}
}