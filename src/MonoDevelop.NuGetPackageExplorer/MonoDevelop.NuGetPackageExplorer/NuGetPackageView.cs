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

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using MonoDevelop.PackageManagement;
using NuGet.Configuration;
using NuGet.Logging;
using NuGet.PackageManagement;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using Xwt;

namespace MonoDevelop.NuGetPackageExplorer
{
	public class NuGetPackageView : AbstractXwtViewContent
	{
		PackageReaderBase reader;
		HPaned pane;
		NuGetPackageMetadataView packageMetadataView;
		NuGetPackageContentsView packageContentsTreeView;
		NuSpecFileView nuspecFileView;
		CancellationTokenSource tokenSource;

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
						ShowMetadata (nuspecReader);
					});
				}
			});
		}

		void ShowMetadata (NuspecReader nuspecReader)
		{
			packageMetadataView.ShowMetadata (nuspecReader);
			packageContentsTreeView.ShowContents (reader);
			nuspecFileView.ShowXml (nuspecReader.Xml);
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
			if (tokenSource != null) {
				tokenSource.Cancel ();
				tokenSource = null;
			}

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
			packageMetadataView.OnCancelDownload = OnCancelDownload;
			topScrollView.Content = packageMetadataView;
			pane.Panel1.Content = topScrollView;

			// Bottom pane.
			var bottomScrollView = new ScrollView ();
			packageContentsTreeView = new NuGetPackageContentsView ();
			bottomScrollView.Content = packageContentsTreeView;
			pane.Panel2.Content = bottomScrollView;
		}

		internal async Task DownloadPackage (
			PackageSearchResultViewModel package,
			IEnumerable<SourceRepository> repositories,
			ISettings settings)
		{
			ContentName = package.Id + " " + package.SelectedVersion.ToNormalizedString ();
			packageMetadataView.ShowMetadata (package);
			packageMetadataView.IsDownloading = true;

			tokenSource = new CancellationTokenSource ();

			try {
				var packageIdentity = new PackageIdentity (package.Id, package.SelectedVersion);
				using (DownloadResourceResult result = await PackageDownloader.GetDownloadResourceResultAsync (
					repositories, packageIdentity, settings, NullLogger.Instance, tokenSource.Token)) {

					if (result.Status == DownloadResourceResultStatus.Available) {
						reader = result.PackageReader;
						var nuspecReader = new NuspecReader (reader.GetNuspec ());
						ShowMetadata (nuspecReader);
					} else if (result.Status == DownloadResourceResultStatus.NotFound) {
						packageMetadataView.ShowError (GettextCatalog.GetString ("Package not found."));
					}

					packageMetadataView.IsDownloading = false;
				}
			} catch (OperationCanceledException) {
				// Ignore.
			} catch (Exception ex) {
				packageMetadataView.IsDownloading = false;
				packageMetadataView.ShowError (GettextCatalog.GetString ("Package download failed: {0}", ex.Message));
				LoggingService.LogError ("Download failed.", ex);
			}
		}

		void OnCancelDownload ()
		{
			try {
				packageMetadataView.IsDownloading = false;

				if (tokenSource != null) {
					tokenSource.Cancel ();
					tokenSource = null;
				}
			} catch (Exception ex) {
				LoggingService.LogError ("Cancel download failed.", ex);
			}
		}
	}
}

