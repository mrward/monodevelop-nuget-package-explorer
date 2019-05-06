//
// OpenNuGetPackageFromSourceHandler.cs
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
using System.Linq;
using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.PackageManagement;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;

namespace MonoDevelop.NuGetPackageExplorer
{
	public class OpenNuGetPackageFromSourceHandler : CommandHandler
	{
		public void Run (PackageDependency dependency)
		{
			string search = GetPackageIdSearch (dependency.Id);
			Run (search);
		}

		static string GetPackageIdSearch (string packageId)
		{
			return string.Format ("packageid:{0}", packageId);
		}

		public void Run (PackageIdentity package)
		{
			string search = GetPackageIdSearch (package.Id);
			Run (search);
		}

		protected override void Run ()
		{
			Run (initialSearch: null);
		}

		void Run (string initialSearch)
		{
			try {
				bool configurePackageSources = false;
				IEnumerable<PackageSearchResultViewModel> packages = null;
				IEnumerable<SourceRepository> repositories = null;
				ISettings settings = null;
				do {
					using (AddPackagesDialog dialog = CreateDialog (initialSearch)) {
						dialog.ShowWithParent ();
						configurePackageSources = dialog.ShowPreferencesForPackageSources;
						initialSearch = dialog.SearchText;
						packages = dialog.SelectedPackages;
						repositories = dialog.GetSourceRepositories ();
						settings = dialog.Settings;
					}
					if (configurePackageSources) {
						ShowPreferencesForPackageSources ();
					} else {
						OpenPackages (packages, repositories, settings);
					}
				} while (configurePackageSources);

			} catch (Exception ex) {
				LoggingService.LogInternalError (ex);
			}
		}

		AddPackagesDialog CreateDialog (string initialSearch)
		{
			var viewModel = AllPackagesViewModel.Create ();
			return new AddPackagesDialog (
				viewModel,
				initialSearch);
		}

		void ShowPreferencesForPackageSources ()
		{
			IdeApp.Workbench.ShowGlobalPreferencesDialog (null, "PackageSources");
		}

		void OpenPackages (
			IEnumerable<PackageSearchResultViewModel> packages,
			IEnumerable<SourceRepository> repositories,
			ISettings settings)
		{
			if (!packages.Any ())
				return;

			foreach (PackageSearchResultViewModel package in packages) {
				package.Parent = null;

				var view = new NuGetPackageView ();
				view.DownloadPackage (package, repositories, settings).Ignore ();

				var controller = new NuGetPackageDisplayBinding (view);
				IdeApp.Workbench.OpenDocument (controller, true).Ignore ();
			}
		}
	}
}

