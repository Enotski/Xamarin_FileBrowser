using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using xFileBrowser.Models;

namespace xFileBrowser.Resources {
	public static class Utilits {
		static readonly string[] SizeSuffixes =
				   { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

		public static string SizeSuffix(long value, int decimalPlaces = 1) {
			if(decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
			if(value < 0) { return $"-{SizeSuffix(-value)}"; }
			if(value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

			int mag = (int)Math.Log(value, 1024);
			decimal adjustedSize = (decimal)value / (1L << (mag * 10));

			if(Math.Round(adjustedSize, decimalPlaces) >= 1000) {
				mag += 1;
				adjustedSize /= 1024;
			}
			return string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize, SizeSuffixes[mag]);
		}

		public static IEnumerable<string> SearchAccessibleFiles(string root, string searchTerm) {
			var files = new List<string>();

			foreach (var file in Directory.EnumerateFiles(root).Where(m => m.ToLower().Contains(searchTerm))) {
				files.Add(file);
			}
			foreach (var subDir in Directory.EnumerateDirectories(root)) {
				try {
					if (subDir.ToLower().Contains(searchTerm))
						files.Add(subDir);
					files.AddRange(SearchAccessibleFiles(subDir, searchTerm));
				} catch (UnauthorizedAccessException ex) {
				}
			}
			return files;
		}

		public static void SetDirectoriesToList(DirectoryInfo dirInfo, ObservableCollection<DirectoryItem> obsList) {
			obsList = obsList ?? new ObservableCollection<DirectoryItem>();
			try {
				var dirs = dirInfo.GetDirectories();
				var files = dirInfo.GetFiles();

				foreach (var dir in dirs) {
					if (dir.Name == "self")
						continue;
					var entriesCount = Directory.GetFileSystemEntries(dir.GetDirrectoryInfoFullName()).Count();
					obsList.Add(new DirectoryItem {
						FullPath = dir.GetDirrectoryInfoFullName(),
						Name = dir.GetDirrectoryInfoName(),
						Icon = Constns.iconFolder,
						ItemInfo = $"Objects - {entriesCount} | {dir.LastWriteTime}",
						IconColor = Color.FromHex("ebebeb")
					});
				}
				foreach (var file in files) {
					var found = Constns.fileApperanceDict.TryGetValue(file.Extension.ToLower(), out Constns.FileAppearance appearance);
					obsList.Add(new DirectoryItem {
						FullPath = file.FullName,
						Name = file.Name,
						Icon = found ? appearance.Icon : Constns.iconFile,
						ItemInfo = $"{SizeSuffix(file.Length, 2)} | {file.LastWriteTime}",
						IconColor = found ? appearance.Color : Color.FromHex("#ebebeb"),
					});
				}
			} catch {
			}
		}

		public static string GetDirrectoryInfoFullName(this DirectoryInfo dir) {
			var res = dir.FullName.EndsWith("emulated") ? Path.Combine(dir.FullName, "0") : dir.FullName;
			return res;
		}
		public static string GetDirrectoryInfoName(this DirectoryInfo dir) {
			return dir.Name.EndsWith("emulated") ? dir.Name + "/0" : dir.Name;
		}
	}
}
