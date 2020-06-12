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
			GetDir(currentDirectory.GetFileSystemInfoFullName());
			DirectoryList.ItemsSource = DirList;
		}

		#region Page events
		private void OnListViewItemSelected(object sender, SelectedItemChangedEventArgs e) {
			if (isSearch) {
				isSearch = false;
				SetSearchUi(isSearch);
			}
			if (e.SelectedItem != null)
				GetDir((e.SelectedItem as DirectoryItem).FullPath);
		}
		private async void EntrySearch_TextChanged(object sender, TextChangedEventArgs e) {
			List<FileSystemInfo> baseStors = null;
			List<FileSystemInfo> foundedItems = null;
			CurrFolderPathInfo = "Founded - 0";
			var searchText = "";
			try {
				DirList.Clear();
				if (!string.IsNullOrEmpty(e.NewTextValue) || !string.IsNullOrWhiteSpace(e.NewTextValue)) {
					searchText = e.NewTextValue.Trim().ToLower();
					baseStors = new List<FileSystemInfo>();
					foundedItems = new List<FileSystemInfo>();
					// get storage folders, this allow to skip 'self directory'
					baseStors.AddRange(new DirectoryInfo("/storage").GetFileSystemInfos());
					await Task.Run(() => {
						foreach (var item in baseStors) {
							if (item.Name == "self")
								continue;
							// search all entries by full path and then skipping elements, witch names that don't match
							foreach (var elem in Utilites.SearchAccessibleDirectoryItemsByFullName(item.GetFileSystemInfoFullName(), searchText).Distinct()) {
								if (elem.Split('/').Last().ToLower().Contains(searchText))
									foundedItems.Add(elem.Split('.').Count() > 1 ? new FileInfo(elem) as FileSystemInfo : new DirectoryInfo(elem));
							}
						}
						Utilites.FillDirsCollectionByItems(foundedItems, DirList, true);

						CurrFolderPathInfo = $"Founded - {DirList.Count()}";
					});
				}
			} catch (Exception ex) { }
		}
		private void ButtonUp_Clicked(object sender, EventArgs e) {
			try {
				if (isSearch)
					return;
				if (currentDirectory.Name == "storage")
					return;
				// skip 'emulated' directory
				GetDir(currentDirectory.Parent.Name == "emulated" ? currentDirectory.Parent.Parent.FullName : currentDirectory.Parent.FullName);
			} catch (Exception ex) {
				return;
			}
		}
		protected override bool OnBackButtonPressed() {
			try {
				if (isSearch) {
					isSearch = false;
					SetSearchUi(isSearch);
					GetDir(currentDirectory.GetFileSystemInfoFullName());
					return true;
				}
				if (currentDirectory.Name == "storage")
					return false;
				// skip 'emulated' directory
				GetDir(currentDirectory.Parent.Name == "emulated" ? currentDirectory.Parent.Parent.FullName : currentDirectory.Parent.FullName);
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
				// little hack for android
				await Task.Run(() => {
					Task.Delay(200).ContinueWith((args) => EntrySearchField.Focus());
				});
			} else {
				GetDir(currentDirectory.GetFileSystemInfoFullName());
			}
		}
		private void ButtonAddFolder_Clicked(object sender, EventArgs e) {

		}
#endregion


		/// <summary>
		/// Get directory items and set them to obs collection
		/// </summary>
		/// <param name="root">Directory full path</param>
		private async void GetDir(string root) {
			try {
				if (File.Exists(root)) {
					DirectoryList.SelectedItem = null;
					return;
				}
				currentDirectory = new DirectoryInfo(root);
				DirList = DirList ?? new ObservableCollection<DirectoryItem>();
				DirList.Clear();
				CurrFolderNameInfo = currentDirectory.Name == "0" ? Path.Combine(currentDirectory.Parent.Name, currentDirectory.Name) : currentDirectory.Name;
				CurrFolderPathInfo = currentDirectory.GetFileSystemInfoFullName();

				await Task.Run(() => {
					Utilites.SetDirectoriesToList(currentDirectory, DirList);
				});
			} catch (Exception ex) {
				return;
			}
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
				CurrFolderPathInfo = currentDirectory.GetFileSystemInfoFullName();
				ButtonSearch.Text = Constns.iconSearch;
			}
		}
		private void InitUiElems() {
			BindingContext = this;
			ButtonSearch.Text = Constns.iconSearch;
			ButtonUp.Text = Constns.iconArrowUp;
			ButtonAddFolder.Text = Constns.iconAddFolder;
		}
	}
}