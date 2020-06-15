using Android.App;
using Android.Content;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using xFileBrowser.Droid;
using xFileBrowser.Models;

[assembly: Dependency(typeof(DocumentViewer))]
namespace xFileBrowser.Droid {
	public class DocumentViewer : IDocumentViewer {
		// Here I have not found a solution to the problem of opening the file by third-party programs :(
		public void ShowDocumentFile(string filepath, string mimeType) {
			try {
				var uri = Android.Net.Uri.Parse("content://" + filepath);
				var intent = new Intent(Intent.ActionView);
				intent.SetDataAndType(uri, mimeType);

				intent.AddFlags(ActivityFlags.GrantPersistableUriPermission | ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission | ActivityFlags.GrantPrefixUriPermission | ActivityFlags.ClearWhenTaskReset | ActivityFlags.NewTask | ActivityFlags.MultipleTask);

				Intent chooserIntent = Intent.CreateChooser(intent, "Open With");
				chooserIntent.AddFlags(ActivityFlags.NewTask | ActivityFlags.GrantReadUriPermission);


				Android.App.Application.Context.StartActivity(chooserIntent);
			} catch (Exception ex) {

			}
		}
	}
}