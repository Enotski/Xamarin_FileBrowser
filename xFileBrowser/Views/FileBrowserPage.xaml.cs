using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xFileBrowser.Models;
using xFileBrowser.Resources;

namespace xFileBrowser.Views {
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class FileBrowserPage : ContentPage, INotifyPropertyChanged {
		private bool isSearchShown = false;
		private bool isAddDirModalWinShown = false;
		private bool isErrorMessShown = false;
		private bool isMenuShown = false;
		private string currFolPathInfo = "";
		private string currFolNameInfo = "";
		private string errorMessage = "";

		public static ObservableCollection<DirectoryItem> DirList { get; set; }
		private DirectoryInfo currentDirectory = new DirectoryInfo("/storage");

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
		public string ErrorMessageText {
			set {
				if (errorMessage != value) {
					errorMessage = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ErrorMessageText"));
				}
			}
			get {
				return errorMessage;
			}
		}
		public bool AddDirModalWinShown {
			set {
				if (isAddDirModalWinShown != value) {
					isAddDirModalWinShown = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AddDirModalWinShown"));
				}
			}
			get {
				return isAddDirModalWinShown;
			}
		}
		public bool ErrorMessageShown {
			set {
				if (isErrorMessShown != value) {
					isErrorMessShown = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ErrorMessageShown"));
				}
			}
			get {
				return isErrorMessShown;
			}
		}
		public bool MenuShown {
			set {
				if (isMenuShown != value) {
					isMenuShown = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MenuShown"));
				}
			}
			get {
				return isMenuShown;
			}
		}

		public FileBrowserPage() {
			InitializeComponent();
			InitUiElems();
			DirList = new ObservableCollection<DirectoryItem>();
			GetDir(currentDirectory.GetFileSystemInfoFullName());
			DirectoryList.ItemsSource = DirList;
		}

		#region Page events
		private void OnListViewItemSelected(object sender, SelectedItemChangedEventArgs e) {
			if (isSearchShown) {
				isSearchShown = false;
				SetSearchUi(isSearchShown);
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
				if (isSearchShown)
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
				if (isSearchShown) {
					isSearchShown = false;
					SetSearchUi(isSearchShown);
					GetDir(currentDirectory.GetFileSystemInfoFullName());
					return true;
				}
				if (AddDirModalWinShown) {
					SetAddDirectoryUi(false);
					return true;
				}
				if (ErrorMessageShown) {
					ErrorMessageShown = false;
					ErrorMessageText = "";
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
			isSearchShown = !isSearchShown;
			SetSearchUi(isSearchShown);
			if (isSearchShown) {
				DirList.Clear();
				// little hack for android
				await Task.Run(() => {
					Task.Delay(200).ContinueWith((args) => EntrySearchField.Focus());
				});
			} else {
				GetDir(currentDirectory.GetFileSystemInfoFullName());
			}
		}
		private async void ButtonAddFolder_Clicked(object sender, EventArgs e) {
			if(currentDirectory.Name == "storage") {
				ErrorMessageText = "Can't add new folder in current directory";
				ErrorMessageShown = true;
				await Task.Run(() => {
					Task.Delay(3000).ContinueWith((args) => {
						ErrorMessageShown = false;
						ErrorMessageText = "";
					});
				});
				return;
			}
			AddDirModalWinShown = !AddDirModalWinShown;
			if (AddDirModalWinShown) {
				SetAddDirectoryUi(true);
			}
		}
		private void ModalBackGround_Tapped(object sender, EventArgs e) {
			if (AddDirModalWinShown) {
				SetAddDirectoryUi(false);
			}
		}
		private void BtnAcceptNewDirAdding_Clicked(object sender, EventArgs e) {
			var newDirName = "";
			if (string.IsNullOrEmpty(EntryNewDirectoryField.Text) || string.IsNullOrWhiteSpace(EntryNewDirectoryField.Text))
				return;
			try {
				newDirName = Path.Combine(currentDirectory.FullName, EntryNewDirectoryField.Text.Trim());
				if (Directory.Exists(newDirName)) {
					return;
				}
				// Try to create the directory.
				DirectoryInfo di = Directory.CreateDirectory(newDirName);
				// skip 'emulated' directory
				GetDir(currentDirectory.GetFileSystemInfoFullName());

				EntryNewDirectoryField.Unfocus();
				EntryNewDirectoryField.Text = "";
				ModalBackGround.IsVisible = false;
				AddDirModalWinShown = false;

			} catch (Exception ex) { }
		}
		private void BtnBackFromNewDirAdding_Clicked(object sender, EventArgs e) {
			SetAddDirectoryUi(false);
		}
		private void ErrorMessageFrame_Tapped(object sender, EventArgs e) {
			ErrorMessageShown = false;
			ErrorMessageText = "";
		}
		private void ButtonMenu_Clicked(object sender, EventArgs e) {
			MenuShown = !MenuShown;
			//MenuPanel.IsVisible = MenuShown;
			ButtonMenu.Text = MenuShown ? Constns.iconCloseRaw : Constns.iconMenu;
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
			FrameSearchField.IsVisible = isSearch;
			EntrySearchField.Text = "";

			ButtonMenu.IsVisible = !isSearch;
			LabelCurrFolderName.IsVisible = !isSearch;
			ButtonAddFolder.IsVisible = !isSearch;
			ButtonUp.IsVisible = !isSearch;

			ButtonSearch.Text = isSearch ? Constns.iconCloseBox : Constns.iconSearch;
			CurrFolderPathInfo = isSearch ? "Founded - 0" : currentDirectory.GetFileSystemInfoFullName();
		}
		private async void SetAddDirectoryUi(bool show) {
			if (show) {
				ModalBackGround.IsVisible = true;
				await Task.Run(() => {
					Task.Delay(200).ContinueWith((args) => {
						EntryNewDirectoryField.Focus();
					});
				});
			} else {
				EntryNewDirectoryField.Unfocus();
				EntryNewDirectoryField.Text = "";
				ModalBackGround.IsVisible = false;
				await Task.Run(() => {
					Task.Delay(100).ContinueWith((args) => {
						AddDirModalWinShown = false;
					});
				});
			}

		}
		private void InitUiElems() {
			BindingContext = this;
			ButtonSearch.Text = Constns.iconSearch;
			ButtonUp.Text = Constns.iconArrowUp;
			ButtonAddFolder.Text = Constns.iconAddFolder;
			ButtonMenu.Text = Constns.iconMenu;
			BtnTransfer.Text = Constns.iconTransfer;
			BtnRemove.Text = Constns.iconRemove;
			BtnCopy.Text = Constns.iconCopy;
			BtnRename.Text = Constns.iconRename;
			BtnInfo.Text = Constns.iconInfo;
		}
	}
}