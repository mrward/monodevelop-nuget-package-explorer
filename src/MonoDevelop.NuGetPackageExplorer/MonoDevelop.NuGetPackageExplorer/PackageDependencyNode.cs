﻿//
// PackageDependencyNode.cs
//
// Author:
//       Matt Ward <matt.ward@xamarin.com>
//
// Copyright (c) 2017 Xamarin Inc. (http://xamarin.com)
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

using System;
using System.Reflection;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui.Pads;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace MonoDevelop.NuGetPackageExplorer
{
	class PackageDependencyNode
	{
		public PackageDependencyNode (object dataItem)
		{
			Init (dataItem);
		}

		public string Id { get; private set; }
		public NuGetVersion Version { get; private set; }
		public PackageIdentity Identity { get; private set; }

		public static PackageDependencyNode GetSelectedNode ()
		{
			var solutionPad = IdeApp.Workbench.Pads?.SolutionPad?.Content as SolutionPad;
			var treeView = solutionPad?.Controller;
			var node = treeView?.GetSelectedNode ();
			var dataItem = node?.DataItem;
			if (dataItem != null && dataItem.GetType ().FullName == "MonoDevelop.DotNetCore.NodeBuilders.PackageDependencyNode") {
				return new PackageDependencyNode (node.DataItem);
			}
			return null;
		}

		void Init (object dataItem)
		{
			var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
			Type type = dataItem.GetType ();
			var property = type.GetProperty ("Name", bindingFlags);
			Id = (string)property?.GetValue (dataItem);

			var field = type.GetField ("version", bindingFlags | BindingFlags.NonPublic);
			string version = field?.GetValue (dataItem)?.ToString ();

			if (string.IsNullOrEmpty (version) || string.IsNullOrEmpty (Id))
				return;

			VersionRange versionRange = null;
			if (!VersionRange.TryParse (version, out versionRange))
				return;

			if (versionRange.IsFloating)
				Version = new NuGetVersion (versionRange.MinVersion.Version);
			else
				Version = versionRange.MinVersion;

			Identity = new PackageIdentity (Id, Version);
		}
	}
}
