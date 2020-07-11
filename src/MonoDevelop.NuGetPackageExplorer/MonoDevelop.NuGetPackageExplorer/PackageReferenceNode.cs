//
// PackageReferenceNode.cs
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
using System.Reflection;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui.Pads;
using NuGet.Versioning;
using NuGet.Packaging.Core;

namespace MonoDevelop.NuGetPackageExplorer
{
	public class PackageReferenceNode
	{
		public PackageReferenceNode (object dataItem)
		{
			Init (dataItem);
		}

		public string Id { get; private set; }
		public NuGetVersion Version { get; private set; }
		public PackageIdentity Identity { get; private set; }

		public static PackageReferenceNode GetSelectedNode ()
		{
			var solutionPad = IdeApp.Workbench.Pads?.SolutionPad?.Content as SolutionPad;
			var treeView = solutionPad?.Controller;
			var node = treeView?.GetSelectedNode ();
			var dataItem = node?.DataItem;
			if (dataItem != null && dataItem.GetType ().FullName == "MonoDevelop.PackageManagement.NodeBuilders.PackageReferenceNode") {
				return new PackageReferenceNode (node.DataItem);
			}
			return null;
		}

		void Init (object dataItem)
		{
			var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
			Type type = dataItem.GetType ();

			var property = type.GetProperty ("Id", bindingFlags);
			Id = (string)property?.GetValue (dataItem);

			property = type.GetProperty ("Version", bindingFlags);
			string version = property?.GetValue (dataItem)?.ToString ();

			if (string.IsNullOrEmpty (version) || string.IsNullOrEmpty (Id))
				return;

			VersionRange versionRange = null;
			if (!VersionRange.TryParse (version, out versionRange))
				return;

			// Just use MinRange here which is different to how the PackageDependencyNode
			// works. The version property here will return 10.0-- for wild cards unlike
			// PackageDependencyNode which returns the correct wildcard. So just use the
			// MinVersion which will fail to resolve a NuGet package and open the dialog.
			Version = versionRange.MinVersion;

			Identity = new PackageIdentity (Id, Version);
		}
	}
}

