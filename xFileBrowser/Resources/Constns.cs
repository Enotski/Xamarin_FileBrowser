﻿using Xamarin.Forms;
using System.Collections.Generic;
using System.Text;
using Android.Media;

namespace xFileBrowser.Resources {
	public static class Constns {

		public static readonly Dictionary<string, FileAppearance> fileApperanceDict = fileApperanceDict = new Dictionary<string, FileAppearance> {
			{".mp3", new FileAppearance("\U000F0E2A", Color.FromHex("00bee8"))},
			{".flac", new FileAppearance("\U000F0E2A", Color.FromHex("e8a600"))},
			{".aac", new FileAppearance("\U000F0E2A", Color.FromHex("754600")) },
			{".cda", new FileAppearance("\U000F0E2A", Color.FromHex("96083c")) },
			{".midi", new FileAppearance("\U000F0E2A", Color.FromHex("089696")) },
			{".ogg", new FileAppearance("\U000F0E2A", Color.FromHex("a80000")) },
			{".voc", new FileAppearance("\U000F0E2A", Color.FromHex("700e07")) },
			{".wav", new FileAppearance("\U000F0E2A", Color.FromHex("2d8f00")) },
			{".wma", new FileAppearance("\U000F0E2A", Color.FromHex("1102b5")) },

			{".png", new FileAppearance("\U000F0EB0", Color.FromHex("700e07")) },
			{".jpg", new FileAppearance("\U000F0EB0", Color.FromHex("7cb502")) },
			{".jpeg", new FileAppearance("\U000F0EB0", Color.FromHex("7cb502")) },
			{".psd", new FileAppearance("\U000F0EB0", Color.FromHex("0093e8")) },
			{".gif", new FileAppearance("\U000F0EB0", Color.FromHex("c7c7c7")) },
			{".bmp", new FileAppearance("\U000F0EB0", Color.FromHex("1102b5")) },
			{".svg", new FileAppearance("\U000F0EB0", Color.FromHex("96083c")) },

			{".mkv", new FileAppearance("\U000F0E2C", Color.FromHex("7cb502")) },
			{".mp4", new FileAppearance("\U000F0E2C", Color.FromHex("0093e8")) },
			{".mov", new FileAppearance("\U000F0E2C", Color.FromHex("96083c")) },
			{".avi", new FileAppearance("\U000F0E2C", Color.FromHex("2d8f00")) },

			{".htm", new FileAppearance("\U000F102B", Color.FromHex("d660b5")) },
			{".html", new FileAppearance("\U000F102B", Color.FromHex("d660b5")) },
			{".js", new FileAppearance("\U000F0BC3", Color.FromHex("fae100")) },
			{".xml", new FileAppearance("\U000F102B", Color.FromHex("00a0c4")) },
			{".json", new FileAppearance("\U000F102B", Color.FromHex("9e7100")) },
			{".txt", new FileAppearance("\U000F09EE", Color.FromHex("cfcfcf")) },
			{".rtf", new FileAppearance("\U000F0C7F", Color.FromHex("9e7100")) },
			{".bin", new FileAppearance("\U000F0C7F", Color.FromHex("9e7100")) },
			{".zip", new FileAppearance("\U000F0E2C", Color.FromHex("9f00e8")) },
			{".rar", new FileAppearance("\U000F0E2C", Color.FromHex("9f00e8")) },
			{".bat", new FileAppearance("\U000F0C7F", Color.FromHex("9e7100")) },
			{".doc", new FileAppearance("\U000F103E", Color.FromHex("00bee8")) },
			{".docx", new FileAppearance("\U000F103E", Color.FromHex("00bee8")) },
			{".pdf", new FileAppearance("\U000F0E2D", Color.FromHex("c40f02")) },
			{".epub", new FileAppearance("\U000F09EE", Color.FromHex("9f00e8")) },
			{".dll", new FileAppearance("\U000F1178", Color.FromHex("9e7100")) },
			{".exe", new FileAppearance("\U000F107C", Color.FromHex("cfcfcf")) },
		};

		public struct FileAppearance {
			public string Icon { get; set; }
			public Color Color { get; set; }
			public FileAppearance(string ico, Color color) {
				Icon = ico;
				Color = color;
			}
		}
	}
}
