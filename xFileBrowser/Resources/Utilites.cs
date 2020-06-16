using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using xFileBrowser.Models;

namespace xFileBrowser.Resources {
	/// <summary>
	/// Class for all utility methods
	/// </summary>
	public static class Utilites {
		static readonly string[] SizeSuffixes =
				   { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

		/// <summary>
		/// Get size of file in b/kb/Mb etc
		/// </summary>
		/// <param name="value">Size of file in bytes</param>
		/// <param name="decimalPlaces">Precision</param>
		/// <returns>Formatted size</returns>
		public static string SizeSuffix(long value, int decimalPlaces = 1) {
			if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
			if (value < 0) { return $"-{SizeSuffix(-value)}"; }
			if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

			int mag = (int)Math.Log(value, 1024);
			decimal adjustedSize = (decimal)value / (1L << (mag * 10));

			if (Math.Round(adjustedSize, decimalPlaces) >= 1000) {
				mag += 1;
				adjustedSize /= 1024;
			}
			return string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize, SizeSuffixes[mag]);
		}
		/// <summary>
		/// Get size of directory in b/kb/Mb etc
		/// </summary>
		/// <param name="value">Size of file in bytes</param>
		/// <param name="decimalPlaces">Precision</param>
		/// <returns>Formatted size</returns>
		public static long SizeOfDirectory(string root) {
			long result = 0;
			foreach (var file in Directory.EnumerateFiles(root)) {
				try {
					result += new FileInfo(file).Length;
				} catch {
					continue;
				}
			}
			foreach (var subDir in Directory.EnumerateDirectories(root)) {
				try {
					result += SizeOfDirectory(subDir);
				} catch (UnauthorizedAccessException ex) {
					continue;
				}
			}
			return result;
		}
		/// <summary>
		/// Search directory items by fullpath in dierectories recursively
		/// </summary>
		/// <param name="root">Root directory</param>
		/// <param name="searchTerm">Search text</param>
		/// <returns>Search result</returns>
		public static IEnumerable<string> SearchAccessibleDirectoryItemsByFullName(string root, string searchTerm) {
			var files = new List<string>();

			foreach (var file in Directory.EnumerateFiles(root).Where(m => m.ToLower().Contains(searchTerm))) {
				files.Add(file);
			}
			foreach (var subDir in Directory.EnumerateDirectories(root)) {
				try {
					if (subDir.ToLower().Contains(searchTerm))
						files.Add(subDir);
					files.AddRange(SearchAccessibleDirectoryItemsByFullName(subDir, searchTerm));
				} catch (UnauthorizedAccessException ex) {
					continue;
				}
			}
			return files;
		}
		/// <summary>
		/// Get directories items of root directory and set to Obs collection
		/// </summary>
		/// <param name="root">Root directory</param>
		/// <param name="obsList">Obs collection</param>
		public static void SetDirectoriesToList(DirectoryInfo root, ObservableCollection<DirectoryItem> obsList) {
			obsList = obsList ?? new ObservableCollection<DirectoryItem>();
			try {
				var fileSysInfos = root.GetFileSystemInfos();
				FillDirsCollectionByItems(fileSysInfos, obsList);
			} catch {
			}
		}
		/// <summary>
		/// Fill obs collection with FileSystemInfos elements
		/// </summary>
		/// <param name="fileSysInfos">Elements</param>
		/// <param name="obsList">Obs collections</param>
		/// <param name="forSearch">Set description of elements for search presentation (full path of item)</param>
		public static void FillDirsCollectionByItems(IEnumerable<FileSystemInfo> fileSysInfos, ObservableCollection<DirectoryItem> obsList, bool forSearch = false) {
			try {
				foreach (var item in fileSysInfos.OrderByDescending(f => f.Extension == "").ThenBy(f => f.Name)) {
					try {
						if (item.Name == "self")
							continue;
						if (item.Extension == "") {
							var entriesCount = Directory.GetFileSystemEntries(item.GetFileSystemInfoFullName()).Count();
							obsList.Add(new DirectoryItem {
								FullPath = item.GetFileSystemInfoFullName(),
								Name = item.GetFileSystemInfoName(),
								Icon = Constns.iconFolder,
								ItemInfo = forSearch ? $"{item.FullName} | {item.LastWriteTime}" : $"Objects - {entriesCount} | {item.LastWriteTime}",
								IconColor = DarkTheme.themeColors["PrimaryTextColor"],
								IsFolder = true,
								 DateChange = item.LastWriteTime,
								  ReadOnly = item.Attributes == FileAttributes.ReadOnly ? "Yes" : "No",
								Hidden = item.Attributes == FileAttributes.Hidden ? "Yes" : "No",
								Archive = item.Attributes == FileAttributes.Archive ? "Yes" : "No",
							});
						} else {
							var found = Constns.fileApperanceDict.TryGetValue(item.Extension.ToLower(), out Constns.FileAppearance appearance);
							var size = $"{((item as FileInfo) != null ? SizeSuffix((item as FileInfo)?.Length ?? 0, 2) : "size is not computed")}";
							obsList.Add(new DirectoryItem {
								FullPath = item.FullName,
								Name = item.Name,
								Icon = found ? appearance.Icon : Constns.iconFile,
								ItemInfo = forSearch ? $"{item.FullName} | {item.LastWriteTime}" : $"{size} | {item.LastWriteTime}",
								FormattedSize = size,
								IconColor = found ? appearance.Color : DarkTheme.themeColors["PrimaryTextColor"],
								DateChange = item.LastWriteTime,
								ReadOnly = item.Attributes == FileAttributes.ReadOnly ? "Yes" : "No",
								Hidden = item.Attributes == FileAttributes.Hidden ? "Yes" : "No",
								Archive = item.Attributes == FileAttributes.Archive ? "Yes" : "No",
							});
						}
					} catch (Exception ex) { }
				}
			} catch (Exception ex) {

			}
		}
		/// <summary>
		/// Allow to skip "emulated" full path
		/// </summary>
		/// <param name="info">FileSystemInfo object</param>
		/// <returns></returns>
		public static string GetFileSystemInfoFullName(this FileSystemInfo info) {
			return info.FullName.EndsWith("emulated") ? Path.Combine(info.FullName, "0") : info.FullName;
		}
		/// <summary>
		/// Allow to skip "emulated" name
		/// </summary>
		/// <param name="info">FileSystemInfo object</param>
		/// <returns></returns>
		public static string GetFileSystemInfoName(this FileSystemInfo info) {
			return info.Name.EndsWith("emulated") ? Path.Combine(info.Name, "0") : info.Name;
		}
		/// <summary>
		/// Rename file or directory
		/// </summary>
		/// <param name="oldName">Old name of item</param>
		/// <param name="newName">New name of item</param>
		/// <returns></returns>
		public static bool RenameDirItem(string oldName, string newName) {
			try {
				if (string.IsNullOrWhiteSpace(oldName) ||
					string.IsNullOrWhiteSpace(newName)) {
					return false;
				}
				var oldItem = new DirectoryInfo(oldName);

				if (oldItem.Extension != "") {
					var file = new FileInfo(oldName);
					if (File.Exists(newName))
						return false;
					File.Move(oldItem.FullName, newName + oldItem.Extension);
					return true;
				}

				if (!oldItem.Exists) {
					return false;
				}

				if (string.Equals(oldItem.Name, newName, StringComparison.OrdinalIgnoreCase)) {
					//new folder name is the same with the old one.
					return false;
				}

				string newDirectory;

				if (oldItem.Parent == null) {
					//root directory
					newDirectory = Path.Combine(oldName, newName);
				} else {
					newDirectory = Path.Combine(oldItem.Parent.FullName, newName);
				}

				if (Directory.Exists(newDirectory)) {
					//target directory already exists
					return false;
				}

				oldItem.MoveTo(newDirectory);

				return true;
			} catch {
				return false;
			}
		}
		/// <summary>
		/// Moving or copieng items
		/// </summary>
		/// <param name="oldDirPath">Path of old directory</param>
		/// <param name="newDirPath">Path of new directory</param>
		/// <param name="copy">Copy items to new place</param>
		public static void MoveCopyDirItem(string oldDirPath, string newDirPath, bool copy = false) {
			if (copy) {
				DirectoryCopy(oldDirPath, newDirPath, true);
			} else {
				if (!Directory.Exists(newDirPath)) {
					Directory.Move(oldDirPath, newDirPath);
				}
			}
		}
		/// <summary>
		/// Method of copieng items
		/// </summary>
		/// <param name="sourceDirName">Name of source directory</param>
		/// <param name="destDirName">Name of destenation directory</param>
		/// <param name="copySubDirs">Perform recursively copieng</param>
		private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs) {
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);

			// if item is file
			if (dir.Extension != "") {
				var file = new FileInfo(sourceDirName);
				if (File.Exists(destDirName))
					return;

				// Copy the file.
				file.CopyTo(destDirName, false);
				return;
			}

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
