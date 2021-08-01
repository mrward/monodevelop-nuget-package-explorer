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
using AppKit;
using CoreGraphics;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.PackageManagement;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.PackageManagement;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using Xwt;

namespace MonoDevelop.NuGetPackageExplorer
{
	public class NuGetPackageView : Widget
	{
		PackageReaderBase reader;
		HPaned pane;
		NuGetPackageMetadataView packageMetadataView;
		NuGetPackageContentsView packageContentsTreeView;
		CancellationTokenSource tokenSource;
		string packageExtractTempDirectory;

		public NuGetPackageView ()
			: this (new NuSpecFileView ())
		{
		}

		public NuGetPackageView (NuSpecFileView nuspecFileView)
		{
			BuildView ();
			NuSpecFileView = nuspecFileView;
		}

		public NuSpecFileView NuSpecFileView { get; }

		public string ContentName { get; private set; }

		public Task Load (FilePath fileName)
		{
			ContentName = fileName.FileName;

			return Task.Run (() => {
				var stream = File.OpenRead (fileName);
				reader = new PackageArchiveReader (stream, leaveStreamOpen: true);
				var nuspecReader = new NuspecReader (reader.GetNuspec ());

				Runtime.RunInMainThread (() => {
					ShowMetadata (nuspecReader);
				});
			});
		}

		void ShowMetadata (NuspecReader nuspecReader)
		{
			packageMetadataView.ShowMetadata (nuspecReader);
			packageContentsTreeView.ShowContents (reader);
			NuSpecFileView.ShowXml (nuspecReader.Xml);
		}

		protected override void Dispose (bool disposing)
		{
			if (!disposing)
				return;

			if (tokenSource != null) {
				tokenSource.Cancel ();
				tokenSource = null;
			}

			if (reader != null) {
				reader.Dispose ();
				reader = null;
			}

			NuSpecFileView.Dispose ();
		}

		void BuildView ()
		{
			pane = new HPaned ();

			// Top pane.
			var topScrollView = new ScrollView ();
			packageMetadataView = new NuGetPackageMetadataView ();
			packageMetadataView.OnCancelDownload = OnCancelDownload;
			packageMetadataView.OnOpenPackageDependency = OnOpenPackageDependency;
			packageMetadataView.OnOpenFile = OnOpenPackageFile;
			topScrollView.Content = packageMetadataView;
			pane.Panel1.Content = topScrollView;

			// Bottom pane.
			var bottomScrollView = new ScrollView ();
			packageContentsTreeView = new NuGetPackageContentsView ();
			packageContentsTreeView.OnOpenFile = OnOpenPackageFile;
			bottomScrollView.Content = packageContentsTreeView;
			pane.Panel2.Content = bottomScrollView;

			// Set an initial width for the metadata view. Otherwise the splitter is all the way
			// over to the right hand side of the window.
			var view = topScrollView.Surface.NativeWidget as NSView;
			view.SetFrameSize (new CGSize (600, view.Frame.Size.Height));

			Content = pane;
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
				string folder = SettingsUtility.GetGlobalPackagesFolder (settings);
				using (var sourceCacheContext = new SourceCacheContext ()) {
					var context = new PackageDownloadContext (sourceCacheContext);
					using (DownloadResourceResult result = await PackageDownloader.GetDownloadResourceResultAsync (
						repositories, packageIdentity, context, folder, NullLogger.Instance, tokenSource.Token)) {

						if (result.Status == DownloadResourceResultStatus.Available) {
							reader = result.PackageReader;
							var nuspecReader = new NuspecReader (reader.GetNuspec ());
							ShowMetadata (nuspecReader);
						} else if (result.Status == DownloadResourceResultStatus.NotFound) {
							packageMetadataView.ShowError (GettextCatalog.GetString ("Package not found."));
						}

						packageMetadataView.IsDownloading = false;
					}
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

		void OnOpenPackageDependency (PackageDependency dependency)
		{
			try {
				var handler = new OpenNuGetPackageFromSourceHandler ();
				handler.Run (dependency);
			} catch (Exception ex) {
				LoggingService.LogError ("Unable to open package dependency.", ex);
			}
		}

		void OnOpenPackageFile (string packageFile)
		{
			try {
				foreach (string extractedFile in ExtractFilesFromPackage (packageFile)) {
					IdeApp.Workbench.OpenDocument (extractedFile, null, true);
				}

			} catch (Exception ex) {
				LoggingService.LogError ("Unable to open file.", ex);
			}
		}

		IEnumerable<string> ExtractFilesFromPackage (string packageFile)
		{
			return reader.CopyFiles (
				GetPackageExtractDirectory (),
				new [] { packageFile },
				ExtractFile,
				NullLogger.Instance,
				CancellationToken.None);
		}

		string GetPackageExtractDirectory ()
		{
			if (packageExtractTempDirectory == null) {
				packageExtractTempDirectory = Path.Combine (
					Path.GetTempPath (),
					"MonoDevelop.NuGetExplorer",
					Guid.NewGuid ().ToString ());
			}

			return packageExtractTempDirectory;
		}

		string ExtractFile (string sourceFile, string targetPath, Stream fileStream)
		{
			Directory.CreateDirectory (Path.GetDirectoryName (targetPath));

			using (FileStream outputStream = File.Create (targetPath)) {
				fileStream.CopyTo (outputStream);
			}

			return targetPath;
		}
	}
}

