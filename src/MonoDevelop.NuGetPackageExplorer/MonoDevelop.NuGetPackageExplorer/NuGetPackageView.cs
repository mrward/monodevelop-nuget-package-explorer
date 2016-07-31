//
// NuGetPackageView.cs
//
// Author:
//       Matt Ward <ward.matt@gmail.com>
//
// Copyright (c) 2016 Matthew Ward
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System.IO;
using System.Threading.Tasks;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using NuGet.Packaging;
using Xwt;

namespace MonoDevelop.NuGetPackageExplorer
{
	public class NuGetPackageView : AbstractXwtViewContent
	{
		PackageArchiveReader reader;
		HPaned pane;
		NuGetPackageMetadataView packageMetadataView;
		NuGetPackageContentsView packageContentsTreeView;
		NuSpecFileView nuspecFileView;

		public NuGetPackageView ()
		{
			BuildView ();
			nuspecFileView = new NuSpecFileView ();
		}

		public override Widget Widget {
			get { return pane; }
		}

		public override Task Load (FileOpenInformation fileOpenInformation)
		{
			ContentName = fileOpenInformation.FileName;

			return Task.Run (() => {
				using (var stream = File.OpenRead (fileOpenInformation.FileName)) {
					reader = new PackageArchiveReader (stream);
					var nuspecReader = new NuspecReader (reader.GetNuspec ());

					Runtime.RunInMainThread (() => {
						packageMetadataView.ShowMetadata (nuspecReader);
						packageContentsTreeView.ShowContents (reader);
						nuspecFileView.ShowXml (nuspecReader.Xml);
					});
				}
			});
		}

		public override string TabPageLabel {
			get {
				return GettextCatalog.GetString ("Package");
			}
		}

		protected override void OnWorkbenchWindowChanged ()
		{
			if (WorkbenchWindow != null) {
				WorkbenchWindow.AttachViewContent (nuspecFileView);
			}
		}

		public override bool IsViewOnly {
			get { return true; }
		}

		public override void Dispose ()
		{
			if (reader != null) {
				reader.Dispose ();
				reader = null;
			}
		}

		void BuildView ()
		{
			pane = new HPaned ();

			// Top pane.
			var topScrollView = new ScrollView ();
			packageMetadataView = new NuGetPackageMetadataView ();
			topScrollView.Content = packageMetadataView;
			pane.Panel1.Content = topScrollView;

			// Bottom pane.
			var bottomScrollView = new ScrollView ();
			packageContentsTreeView = new NuGetPackageContentsView ();
			bottomScrollView.Content = packageContentsTreeView;
			pane.Panel2.Content = bottomScrollView;
		}
	}
}

