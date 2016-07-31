//
// NuGetPackageFilePath.cs
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
using System.Linq;
using NuGet.Packaging;

namespace MonoDevelop.NuGetPackageExplorer
{
	public class NuGetPackageFilePath
	{
		string file;
		string[] parts;
		string[] directories = new string[0];
		string rootFolder = string.Empty;

		public NuGetPackageFilePath (string file)
		{
			this.file = file;
			parts = file.Split ('/');

			if (parts.Length > 1) {
				rootFolder = parts[0];
				HasRootFolder = true;
				directories = parts.Take (parts.Length - 1).ToArray ();
			}
		}

		public bool HasRootFolder { get; private set; }

		public string FileName {
			get {
				return parts[parts.Length - 1];
			}
		}

		public string[] Directories {
			get { return directories; }
		}

		public bool IsInternalPackageFile ()
		{
			if (HasFolder ()) {
				return !IsKnownFolder ();
			}

			return IsInternalPackageFile (parts[0]);
		}

		bool IsInternalPackageFile (string name)
		{
			return name.EndsWith (".nuspec", StringComparison.OrdinalIgnoreCase) ||
				name.Equals ("[Content_Types].xml", StringComparison.OrdinalIgnoreCase);
		}

		bool HasFolder ()
		{
			return directories.Length > 0;
		}

		bool IsKnownFolder ()
		{
			if (!HasFolder ())
				return true;

			return PackagingConstants.Folders.Known.Any (MatchesRootFolder);
		}

		bool MatchesRootFolder (string folder)
		{
			return rootFolder.Equals (folder, StringComparison.OrdinalIgnoreCase);
		}

		public override string ToString ()
		{
			return file;
		}
	}
}

