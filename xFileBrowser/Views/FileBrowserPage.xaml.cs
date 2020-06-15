using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xFileBrowser.Models;
using xFileBrowser.Resources;
using HeyRed.Mime;

namespace xFileBrowser.Views {
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class FileBrowserPage : ContentPage, INotifyPropertyChanged {
		private bool isAllChecked = false;
		private bool isSearchShown = false;
		private bool isAddDirModalWinShown = false;
		private bool isErrorMessShown = false;
		private bool isMenuShown = false;
		private bool isInfoWindowShown = false;
		private string currFolPathInfo = "";
		private string currFolNameInfo = "";
		private string errorMessage = "";
		public static ObservableCollection<DirectoryItem> DirList { get; set; }
		public static List<DirectoryItem> ItemsForTransfer = new List<DirectoryItem>();
		private DirectoryInfo currentDirectory = new DirectoryInfo("/storage");

		public new event PropertyChangedEventHandler PropertyChanged;

		public IDocumentViewer docView = DependencyService.Get<IDocumentViewer>();
		public bool RenameWindowShown { get; set; }
		public bool IsTransferMode { get; set; }
		public bool IsCopyMode { get; set; }
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
		public bool InfoWindowShown {
			set {
				if (isInfoWindowShown != value) {
					isInfoWindowShown = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InfoWindowShown"));
				}
			}
			get {
				return isInfoWindowShown;
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
		private async void ButtonSearch_CheckAll_Accept_Clicked(object sender, EventArgs e) {
			if (IsTransferMode || IsCopyMode) {
				await Task.Run(() => {
					foreach (var item in ItemsForTransfer) {
						MoveCopyDirItem(item.FullPath, Path.Combine(currentDirectory.FullName, item.Name), IsCopyMode);
					}
				}).ContinueWith((arg) => {
					foreach (var item in DirList) {
						item.ItemChecked = isAllChecked;
					}
					GetDir(currentDirectory.GetFileSystemInfoFullName());
				});
				SetTransferUi(false);
				return;
			}
			if (MenuShown) {
				isAllChecked = !isAllChecked;
				await Task.Run(() => {
					foreach (var item in DirList) {
						item.ItemChecked = isAllChecked;
					}
				});
			} else {
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
		}
		private void ButtonAddFolder_Clicked(object sender, EventArgs e) {
			if (currentDirectory.Name == "storage") {
				ShowErrorMessage("Can't add new folder in current directory");
				return;
			}
			AddDirModalWinShown = !AddDirModalWinShown;
			if (AddDirModalWinShown) {
				SetAddDirectoryUi(true);
			}
		}
		private void ButtonMenu_Clicked(object sender, EventArgs e) {
			MenuShown = !MenuShown;
			if (IsTransferMode || IsCopyMode) {
				IsTransferMode = false;
				IsCopyMode = false;
				SetTransferUi(false);
			}
			SetBottomMenuUi();
		}
		protected override bool OnBackButtonPressed() {
			try {
				if (isSearchShown) {
					isSearchShown = false;
					SetSearchUi(isSearchShown);
					GetDir(currentDirectory.GetFileSystemInfoFullName());
					return true;
				} else if (AddDirModalWinShown) {
					if (RenameWindowShown)
						SetRenameDirectoryUi(false);
					else
						SetAddDirectoryUi(false);
					return true;
				} else if (MenuShown) {
					MenuShown = false;
					SetBottomMenuUi();
					return true;
				} else if (IsTransferMode || IsCopyMode) {
					IsTransferMode = false;
					IsCopyMode = false;
					SetTransferUi(false);
					return true;
				} else if (InfoWindowShown) {
					SetInfoDirWindowUi(false);
					return true;
				} else if (ErrorMessageShown) {
					ErrorMessageShown = false;
					ErrorMessageText = "";
					return true;
				} else if (currentDirectory.Name == "storage")
					return false;
				// skip 'emulated' directory
				GetDir(currentDirectory.Parent.Name == "emulated" ? currentDirectory.Parent.Parent.FullName : currentDirectory.Parent.FullName);
				return true;
			} catch (Exception ex) {
				return true;
			}
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
		private void ModalBackGround_Tapped(object sender, EventArgs e) {
			if (AddDirModalWinShown) {
				if (RenameWindowShown)
					SetRenameDirectoryUi(false);
				else
					SetAddDirectoryUi(false);
			} else if (InfoWindowShown) {
				SetInfoDirWindowUi(false);
			}
		}
		private async void BtnAcceptNewDirAdding_Clicked(object sender, EventArgs e) {
			var newDirName = "";
			if (string.IsNullOrEmpty(EntryNewDirectoryField.Text) || string.IsNullOrWhiteSpace(EntryNewDirectoryField.Text))
				return;
			try {
				newDirName = Path.Combine(currentDirectory.FullName, EntryNewDirectoryField.Text.Trim());
				if (Directory.Exists(newDirName)) {
					ShowErrorMessage($"Folder: [{newDirName}] already exist!");
					return;
				}
				if (RenameWindowShown) {
					await Task.Run(() => RenameFolder(DirList.FirstOrDefault(x => x.ItemChecked)?.FullPath, newDirName)).ContinueWith((arg) => {
						if (!arg.Result)
							ShowErrorMessage($"Something goes wrong");
					});
				} else {
					// Try to create the directory.
					DirectoryInfo di = Directory.CreateDirectory(newDirName);
				}
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
		private void ListViewItem_Tapped(object sender, EventArgs e) {
			try {
				var context = (sender as ViewCell)?.BindingContext as DirectoryItem;
				if (context == null)
					return;

				if (MenuShown) {
					context.ItemChecked = !context.ItemChecked;
					return;
				}
				if (isSearchShown) {
					isSearchShown = false;
					SetSearchUi(isSearchShown);
				}
				if (context.IsFolder)
					GetDir(context.FullPath);
				else
					docView?.ShowDocumentFile(context.FullPath, MimeTypesMap.GetMimeType(context.FullPath));
			} catch { }
		}
		private void ItemChecked_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			BtnTransfer.TextColor = DirList.Any(x => x.ItemChecked) ? Color.FromHex("ebebeb") : Color.FromHex("999");
			BtnCopy.TextColor = DirList.Any(x => x.ItemChecked) ? Color.FromHex("ebebeb") : Color.FromHex("999");
			BtnRemove.TextColor = DirList.Any(x => x.ItemChecked) ? Color.FromHex("ebebeb") : Color.FromHex("999");
			BtnRename.TextColor = (DirList.All(x => x.ItemChecked) || DirList.Where(x => x.ItemChecked).Count() > 1) ? Color.FromHex("999") : DirList.Any(x => x.ItemChecked) ? Color.FromHex("ebebeb") : Color.FromHex("999");
			BtnInfo.TextColor = (DirList.All(x => x.ItemChecked) || DirList.Where(x => x.ItemChecked).Count() > 1) ? Color.FromHex("999") : DirList.Any(x => x.ItemChecked) ? Color.FromHex("ebebeb") : Color.FromHex("999");
		}
		private void BtnTransfer_Clicked(object sender, EventArgs e) {
			if (DirList.Where(x => x.ItemChecked).Count() == 0) {
				return;
			}
			if (currentDirectory.Name == "storage") {
				ShowErrorMessage("Can't transfer this directory");
				return;
			}
			IsTransferMode = true;
			ItemsForTransfer.Clear();
			ItemsForTransfer.AddRange(DirList.Where(x => x.ItemChecked));
			SetTransferUi(true);
		}
		private void BtnCopy_Clicked(object sender, EventArgs e) {
			if (DirList.Where(x => x.ItemChecked).Count() == 0) {
				return;
			}
			if (currentDirectory.Name == "storage") {
				ShowErrorMessage("Can't copy this directory");
				return;
			}
			IsCopyMode = true;
			ItemsForTransfer.Clear();
			ItemsForTransfer.AddRange(DirList.Where(x => x.ItemChecked));
			SetTransferUi(true);
		}
		private async void BtnRemove_Clicked(object sender, EventArgs e) {
			if (DirList.Where(x => x.ItemChecked).Count() == 0) {
				return;
			}
			if (currentDirectory.Name == "storage") {
				ShowErrorMessage("Can't remove this directory");
				return;
			}
			try {
				var list = DirList.Where(x => x.ItemChecked);
				await Task.Run(() => {
					foreach (var item in list) {
						try {
							if (!Directory.Exists(item.FullPath)) {
								ShowErrorMessage($"Error! {item.FullPath} is not exist");
								continue;
							}
							Directory.Delete(item.FullPath, true);
						} catch (Exception ex) { }
					}
					GetDir(currentDirectory.GetFileSystemInfoFullName());
				});
			} catch (Exception ex) { }
		}
		private void BtnRename_Clicked(object sender, EventArgs e) {
			if (DirList.Where(x => x.ItemChecked).Count() != 1) {
				return;
			}
			if (currentDirectory.Name == "storage") {
				ShowErrorMessage("Can't rename this directory");
				return;
			}
			AddDirModalWinShown = !AddDirModalWinShown;
			if (AddDirModalWinShown) {
				SetRenameDirectoryUi(true, DirList.FirstOrDefault(x => x.ItemChecked)?.Name);
			}
		}
		private void BtnInfo_Clicked(object sender, EventArgs e) {
			if (DirList.Where(x => x.ItemChecked).Count() != 1) {
				return;
			}
			var item = DirList.FirstOrDefault(x => x.ItemChecked);
			if (item.IsFolder)
				item.FormattedSize = Utilites.SizeSuffix(Utilites.SizeOfDirectory(item.FullPath), 2);
			InfoDirModalWindow.BindingContext = item;
			SetInfoDirWindowUi(true);
		}
		private void InfoWindowOk_Clicked(object sender, EventArgs e) {
			SetInfoDirWindowUi(false);
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
					foreach (var item in DirList) {
						item.PropertyChanged += ItemChecked_PropertyChanged;
					}
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

			ButtonSearch_CheckAll_Accept.Text = isSearch ? Constns.iconCloseBox : Constns.iconSearch;
			CurrFolderPathInfo = isSearch ? "Founded - 0" : currentDirectory.GetFileSystemInfoFullName();
		}
		private void SetTransferUi(bool show) {
			MenuShown = false;
			ButtonUp.IsVisible = true;
			isAllChecked = false;
			if (show) {
				DirectoryList.SelectionMode = ListViewSelectionMode.Single;
				ButtonSearch_CheckAll_Accept.Text = Constns.iconCheck;
				ButtonSearch_CheckAll_Accept.TextColor = Color.FromHex("e8a600");
				ButtonMenu.TextColor = Color.FromHex("c40f02");
			} else {
				ButtonMenu.Text = Constns.iconMenu;
				ButtonSearch_CheckAll_Accept.Text = Constns.iconSearch;
				ButtonSearch_CheckAll_Accept.TextColor = Color.FromHex("ebebeb");
				ButtonMenu.TextColor = Color.FromHex("ebebeb");
				ButtonAddFolder.IsVisible = true;
			}
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
		private async void SetRenameDirectoryUi(bool show, string oldItemName = "") {
			if (show) {
				ModalBackGround.IsVisible = true;
				RenameWindowShown = true;
				LabelAddModalFolder.Text = oldItemName;
				await Task.Run(() => {
					Task.Delay(200).ContinueWith((args) => {
						EntryNewDirectoryField.Focus();
					});
				});
			} else {
				EntryNewDirectoryField.Unfocus();
				EntryNewDirectoryField.Text = "";
				ModalBackGround.IsVisible = false;
				RenameWindowShown = false;
				LabelAddModalFolder.Text = "Enter name of new folder";
				await Task.Run(() => {
					Task.Delay(100).ContinueWith((args) => {
						AddDirModalWinShown = false;
					});
				});
			}
		}
		private void SetInfoDirWindowUi(bool show) {
			if (show) {
				InfoWindowShown = true;
				ModalBackGround.IsVisible = true;
			} else {
				InfoWindowShown = false;
				ModalBackGround.IsVisible = false;
			}
		}
		private void InitUiElems() {
			BindingContext = this;
			ButtonSearch_CheckAll_Accept.Text = Constns.iconSearch;
			ButtonUp.Text = Constns.iconArrowUp;
			ButtonAddFolder.Text = Constns.iconAddFolder;
			ButtonMenu.Text = Constns.iconMenu;
			BtnTransfer.Text = Constns.iconTransfer;
			BtnRemove.Text = Constns.iconRemove;
			BtnCopy.Text = Constns.iconCopy;
			BtnRename.Text = Constns.iconRename;
			BtnInfo.Text = Constns.iconInfo;
		}
		private async void ShowErrorMessage(string message) {
			ErrorMessageText = message;
			ErrorMessageShown = true;
			await Task.Run(() => {
				Task.Delay(3000).ContinueWith((args) => {
					ErrorMessageShown = false;
					ErrorMessageText = "";
				});
			});
		}
		private async void SetBottomMenuUi() {
			DirectoryList.SelectionMode = MenuShown ? ListViewSelectionMode.None : ListViewSelectionMode.Single;
			ButtonMenu.Text = MenuShown ? Constns.iconCloseRaw : Constns.iconMenu;
			ButtonSearch_CheckAll_Accept.Text = MenuShown ? Constns.iconCheckAll : Constns.iconSearch;

			ButtonUp.IsVisible = !MenuShown;
			ButtonAddFolder.IsVisible = !MenuShown;

			isAllChecked = false;
			await Task.Run(() => {
				foreach (var item in DirList) {
					item.ItemChecked = isAllChecked;
				}
			});
		}
		/// <summary>
		/// Renames a directory name
		/// </summary>
		/// <param name="directory">The full directory of the folder</param>
		/// <param name="newFolderName">New name of the folder</param>
		/// <returns>Returns true if rename is successfull</returns>
		public static bool RenameFolder(string directory, string newFolderName) {
			try {
				if (string.IsNullOrWhiteSpace(directory) ||
					string.IsNullOrWhiteSpace(newFolderName)) {
					return false;
				}


				var oldDirectory = new DirectoryInfo(directory);

				if (!oldDirectory.Exists) {
					return false;
				}

				if (string.Equals(oldDirectory.Name, newFolderName, StringComparison.OrdinalIgnoreCase)) {
					//new folder name is the same with the old one.
					return false;
				}

				string newDirectory;

				if (oldDirectory.Parent == null) {
					//root directory
					newDirectory = Path.Combine(directory, newFolderName);
				} else {
					newDirectory = Path.Combine(oldDirectory.Parent.FullName, newFolderName);
				}

				if (Directory.Exists(newDirectory)) {
					//target directory already exists
					return false;
				}

				oldDirectory.MoveTo(newDirectory);

				return true;
			} catch {
				return false;
			}
		}
		private void MoveCopyDirItem(string oldDirPath, string newDirPath, bool copy = false) {
			if (copy) {
				DirectoryCopy(oldDirPath, newDirPath, true);
			}
			if (!Directory.Exists(newDirPath)) {
				Directory.Move(oldDirPath, newDirPath);
			}
		}
		private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs) {
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);
			DirectoryInfo[] dirs = dir.GetDirectories();

			// If the source directory does not exist, throw an exception.
			if (!dir.Exists) {
				throw new DirectoryNotFoundException(
					"Source directory does not exist or could not be found: "
					+ sourceDirName);
			}

			// If the destination directory does not exist, create it.
			if (!Directory.Exists(destDirName)) {
				Directory.CreateDirectory(destDirName);
			}

			// Get the file contents of the directory to copy.
			FileInfo[] files = dir.GetFiles();

			foreach (FileInfo file in files) {
				// Create the path to the new copy of the file.
				string temppath = Path.Combine(destDirName, file.Name);

				// Copy the file.
				file.CopyTo(temppath, false);
			}

			// If copySubDirs is true, copy the subdirectories.
			if (copySubDirs) {

				foreach (DirectoryInfo subdir in dirs) {
					// Create the subdirectory.
					string temppath = Path.Combine(destDirName, subdir.Name);

					// Copy the subdirectories.
					DirectoryCopy(subdir.FullName, temppath, copySubDirs);
				}
			}
		}
	}
}