using System;
using System.Collections.Generic;
using System.Text;

namespace xFileBrowser.Models {
	public interface IDocumentViewer {
		void ShowDocumentFile(string filepaht, string mimeType);
	}
}
