//
// NuGetPackageContentsView.cs
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
using NuGet.Packaging;
using Xwt;

namespace MonoDevelop.NuGetPackageExplorer
{
	public class NuGetPackageContentsView : TreeView
	{
		TreeStore treeStore;
		DataField<string> nameField;

		public NuGetPackageContentsView ()
		{
			nameField = new DataField<string> ();
			treeStore = new TreeStore (nameField);

			DataSource = treeStore;
			Columns.Add ("Name", nameField);
			HeadersVisible = false;
		}

		public void ShowContents (PackageArchiveReader reader)
		{
			foreach (NuGetPackageFilePath file in GetFiles (reader)) {
				if (!file.IsInternalPackageFile ()) {
					AddFile (file);
				}
			}
		}

		IEnumerable<NuGetPackageFilePath> GetFiles (PackageArchiveReader reader)
		{
			var files = reader
				.GetFiles ()
				.Select (file => new NuGetPackageFilePath (file))
				.ToList ();

			files.Sort (CompareFilePathsForDisplay);

			return files;
		}

		static int CompareFilePathsForDisplay (NuGetPackageFilePath x, NuGetPackageFilePath y)
		{
			if (x.HasRootFolder && !y.HasRootFolder) {
				return -1;
			} else if (!x.HasRootFolder && y.HasRootFolder) {
				return 1;
			} else {
				return string.Compare (x.ToString (), y.ToString (), true);
			}
		}

		void AddFile (NuGetPackageFilePath file)
		{
			if (file.HasRootFolder) {
				TreeNavigator parent = GetTreeNode (file.Directories);
				TreeNavigator fileNode = parent.AddChild ();
				fileNode.SetValue (nameField, file.FileName);
			} else {
				TreeNavigator fileNode = treeStore.AddNode ();
				fileNode.SetValue (nameField, file.ToString ());
			}
		}

		TreeNavigator GetTreeNode (string [] directories)
		{
			TreeNavigator parent = null;

			foreach (string directory in directories) {
				TreeNavigator current = GetTreeNode (parent, directory);
				parent = current;
			}

			return parent;
		}

		TreeNavigator GetTreeNode (TreeNavigator parent, string name)
		{
			TreeNavigator current = null;

			if (parent == null) {
				current = FindRootTreeNode (name);
				if (current == null) {
					current = treeStore.AddNode ();
					current.SetValue (nameField, name);
				}
				return current;
			}

			TreeNavigator child = parent;
			if (child.MoveToChild ()) {
				current = FindTreeNode (child, name);
				if (current != null) {
					return current;
				} else {
					child.MoveToParent ();
				}
			}

			current = parent.AddChild ();
			current.SetValue (nameField, name);

			return current;
		}

		TreeNavigator FindRootTreeNode (string name)
		{
			TreeNavigator currentNode = treeStore.GetFirstNode ();
			if (currentNode.CurrentPosition != null) {
				return FindTreeNode (currentNode, name);
			}

			return null;
		}

		TreeNavigator FindTreeNode (TreeNavigator sibling, string name)
		{
			TreeNavigator currentNode = sibling;

			while (currentNode != null) {
				string currentNodeName = currentNode.GetValue (nameField);
				if (currentNodeName.Equals (name, StringComparison.OrdinalIgnoreCase)) {
					return currentNode;
				}

				if (!currentNode.MoveNext ()) {
					return null;
				}
			}

			return null;
		}
	}
}

