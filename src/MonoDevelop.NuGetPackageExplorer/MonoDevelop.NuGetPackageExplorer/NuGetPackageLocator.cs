//
// NuGetPackageLocator.cs
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
using MonoDevelop.Core;
using NuGet.PackageManagement;
using NuGet.ProjectManagement;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using MonoDevelop.Projects;
using NuGet.Common;
using System.IO;
using NuGet.Packaging;

namespace MonoDevelop.NuGetPackageExplorer
{
	public class NuGetPackageLocator
	{
		/// <summary>
		/// Tries to find the package in the global packages directory, then tries the 
		/// solution's packages directory. If the NuGet package cannot be found then this
		/// method returns null.
		/// </summary>
		public static string GetNuGetPackagePath (Project project, PackageIdentity packageIdentity)
		{
			var settings = Settings.LoadDefaultSettings (project.ParentSolution.BaseDirectory, null, null);

			string globalPackagesFolder = SettingsUtility.GetGlobalPackagesFolder (settings);
			var packagePathResolver = new VersionFolderPathResolver (globalPackagesFolder, normalizePackageId: false);
			string packagePath = packagePathResolver.GetPackageFilePath (packageIdentity.Id, packageIdentity.Version);
			if (File.Exists (packagePath))
				return packagePath;

			string path = PackagesFolderPathUtility.GetPackagesFolderPath (project.ParentSolution.BaseDirectory, settings);
			string fullPath = new FilePath (path).FullPath;

			var folder = new FolderNuGetProject (fullPath);
			packagePath = folder.GetInstalledPackageFilePath (packageIdentity);
			if (File.Exists (packagePath))
				return packagePath;

			return null;
		}
	}
}

