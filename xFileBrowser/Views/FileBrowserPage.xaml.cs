using Android.App;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xFileBrowser.Models;
using xFileBrowser.Resources;

namespace xFileBrowser.Views {
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class FileBrowserPage : ContentPage, INotifyPropertyChanged {
		private bool isSearch = false;
		public static ObservableCollection<DirectoryItem> DirList { get; set; }
		private string currFolPathInfo = "";
		private string currFolNameInfo = "";
		public new event PropertyChangedEventHandler PropertyChanged;
		public string CurrFolderPathInfo {
			set {
				if (currFolPathInfo != value) {
					currFolPathInfo = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrFolderPathInfo"));
				}
			}
			get {
				return currFolPathInfo;
			}
		}
		public string CurrFolderNameInfo {
			set {
				if (currFolNameInfo != value) {
					currFolNameInfo = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrFolderNameInfo"));
				}
			}
			get {
				return currFolNameInfo;
			}
		}
		private DirectoryInfo currentDirectory = new DirectoryInfo("/storage");

		public FileBrowserPage() {
			InitializeComponent();
			InitUiElems();
			DirList = new ObservableCollection<DirectoryItem>();
			GetDir(currentDirectory.GetDirrectoryInfoFullName());
			DirectoryList.ItemsSource = DirList;
		}
		private async void GetDir(string path) {
			try {
				if (File.Exists(path)) {
					DirectoryList.SelectedItem = null;
					return;
				}
				currentDirectory = new DirectoryInfo(path);
				DirList = DirList ?? new ObservableCollection<DirectoryItem>();
				DirList.Clear();
				CurrFolderNameInfo = currentDirectory.Name == "0" ? Path.Combine(currentDirectory.Parent.Name, currentDirectory.Name) : currentDirectory.Name;
				CurrFolderPathInfo = currentDirectory.GetDirrectoryInfoFullName();

				await Task.Run(() => {
					Utilits.SetDirectoriesToList(currentDirectory, DirList);
				});
			} catch (Exception ex) {
				return;
			}
		}

		private void OnListViewItemSelected(object sender, SelectedItemChangedEventArgs e) {
			if (isSearch) {
				isSearch = false;
				SetSearchUi(isSearch);
			}
			if (e.SelectedItem != null)
				GetDir((e.SelectedItem as DirectoryItem).FullPath);
		}

		private void ButtonUp_Clicked(object sender, EventArgs e) {
			try {
				var dirs = CurrFolderPathInfo.Split(new[] { '/' }).ToList();
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
				if (isSearch) {
					isSearch = false;
					SetSearchUi(isSearch);
					GetDir(currentDirectory.GetDirrectoryInfoFullName());
					return true;
				}
				var dirs = CurrFolderPathInfo.Split(new[] { '/' }).ToList();
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

		private async void ButtonSearchOpen_Clicked(object sender, EventArgs e) {
			isSearch = !isSearch;
			SetSearchUi(isSearch);
			if (isSearch) {
				DirList.Clear();
				// little hack fo android
				await Task.Run(() => {
					Task.Delay(200).ContinueWith((args) => EntrySearchField.Focus());
				});
			} else {
				GetDir(currentDirectory.GetDirrectoryInfoFullName());
			}
		}
		private void ButtonAddFolder_Clicked(object sender, EventArgs e) {

		}
		private void SetSearchUi(bool isSearch) {
			if (isSearch) {
				LabelCurrFolderName.IsVisible = false;
				FrameSearchField.IsVisible = true;
				ButtonAddFolder.IsVisible = false;
				ButtonUp.IsVisible = false;
				EntrySearchField.Text = "";
				CurrFolderPathInfo = "Founded - 0";
				ButtonSearch.Text = Constns.iconClose;
			} else {
				LabelCurrFolderName.IsVisible = true;
				FrameSearchField.IsVisible = false;
				MiddleLayout.IsVisible = true;
				ButtonAddFolder.IsVisible = true;
				ButtonUp.IsVisible = true;
				CurrFolderPathInfo = currentDirectory.GetDirrectoryInfoFullName();
				ButtonSearch.Text = Constns.iconSearch;
			}
		}
		private void InitUiElems() {
			BindingContext = this;
			ButtonSearch.Text = Constns.iconSearch;
			ButtonUp.Text = Constns.iconArrowUp;
			ButtonAddFolder.Text = Constns.iconAddFolder;
		}
		private async void EntrySearch_TextChanged(object sender, TextChangedEventArgs e) {
			List<FileSystemInfo> baseStors = null;
			List<FileSystemInfo> result = null;
			List<DirectoryItem> itemList = null;
			CurrFolderPathInfo = "Founded - 0";
			try {
				DirList.Clear();
				if (!string.IsNullOrEmpty(e.NewTextValue) || !string.IsNullOrWhiteSpace(e.NewTextValue)) {
					baseStors = new List<FileSystemInfo>();
					result = new List<FileSystemInfo>();
					baseStors.AddRange(new DirectoryInfo("/storage").GetFileSystemInfos());
					await Task.Run(() => {
						foreach (var item in baseStors) {
							if (item.Name == "self")
								continue;
							foreach (var elem in Utilits.SearchAccessibleFiles(item.Name == "emulated" ? item.FullName + "/0" : item.FullName, e.NewTextValue.Trim().ToLower())) {
								result.Add(elem.Split('.').Count() > 1 ? new FileInfo(elem) as FileSystemInfo : new DirectoryInfo(elem));
							}
						}
						itemList = new List<DirectoryItem>();
						foreach (var item in result) {
							if (item.Name != "self" && item.Name.ToLower().Contains(e.NewTextValue.Trim().ToLower()) && !DirList.Any(x => x.FullPath == item.FullName)) {
								try {
									if (item.Extension == "") {
										var entriesCount = item.Name == "emulated" ? Directory.GetFileSystemEntries(item.FullName + @"/0").Count() : (item as DirectoryInfo).EnumerateFileSystemInfos().Count();
										itemList.Add(new DirectoryItem {
											FullPath = item.FullName.EndsWith("emulated") ? item.FullName + @"/0" : item.FullName,
											Name = item.Name.EndsWith("emulated") ? item.Name + @"/0" : item.Name,
											Icon = Constns.iconFolder,
											ItemInfo = $"{item.FullName} | Objects - {entriesCount}",
											IconColor = Color.FromHex("ebebeb"),
											IsFolder = true
										});
									} else {
										var found = Constns.fileApperanceDict.TryGetValue(item.Extension.ToLower(), out Constns.FileAppearance appearance);
										itemList.Add(new DirectoryItem {
											FullPath = item.FullName,
											Name = item.Name,
											Icon = found ? appearance.Icon : Constns.iconFile,
											ItemInfo = $"{item.FullName} | {Utilits.SizeSuffix((item as FileInfo).Length, 2)}",
											IconColor = found ? appearance.Color : Color.FromHex("#ebebeb"),
										});
									}
								} catch (Exception ex) { }
							}
						}
						CurrFolderPathInfo = $"Founded - {itemList.Count()}";
						foreach (var item in itemList.OrderByDescending(x => x.IsFolder).ThenBy(x => x.Name))
							DirList.Add(item);
					});
				}
			} catch (Exception ex) { }
		}
	}
}