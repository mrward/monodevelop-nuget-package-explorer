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
using MonoDevelop.Core;
using NuGet.Packaging;
using Xwt;

namespace MonoDevelop.NuGetPackageExplorer
{
	public class NuGetPackageContentsView : TreeView
	{
		TreeStore treeStore;
		DataField<string> nameField;
		DataField<string> fileField;
		Menu contextMenu = new Menu ();

		public NuGetPackageContentsView ()
		{
			nameField = new DataField<string> ();
			fileField = new DataField<string> ();
			treeStore = new TreeStore (nameField, fileField);

			DataSource = treeStore;
			Columns.Add ("Name", nameField);

			HeadersVisible = false;

			var openFileMenuItem = new MenuItem (GettextCatalog.GetString ("Open"));
			openFileMenuItem.Clicked += OpenFileMenuItemClicked;
			contextMenu.Items.Add (openFileMenuItem);
		}

		public void ShowContents (PackageReaderBase reader)
		{
			foreach (NuGetPackageFilePath file in GetFiles (reader)) {
				if (!file.IsInternalPackageFile ()) {
					AddFile (file);
				}
			}
		}

		IEnumerable<NuGetPackageFilePath> GetFiles (PackageReaderBase reader)
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
				fileNode.SetValue (fileField, file.ToString ());
			} else {
				TreeNavigator fileNode = treeStore.AddNode ();
				fileNode.SetValue (nameField, file.ToString ());
				fileNode.SetValue (fileField, file.ToString ());
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

		protected override void OnRowActivated (TreeViewRowEventArgs a)
		{
			base.OnRowActivated (a);

			OpenSelectedFile ();
		}

		protected override void OnButtonPressed (ButtonEventArgs args)
		{
			base.OnButtonPressed (args);

			if (args.Button != PointerButton.Right) {
				return;
			}

			TreePosition treePosition;
			RowDropPosition rowDropPosition;
			if (GetDropTargetRow (args.X, args.Y, out rowDropPosition, out treePosition)) {
				string file = GetFile (treePosition);
				if (file != null) {
					contextMenu.Popup (this, args.X, args.Y);
				}
			}
		}

		string GetFile (TreePosition position)
		{
			TreeNavigator node = treeStore.GetNavigatorAt (position);
			if (node != null) {
				return node.GetValue (fileField);
			}
			return null;
		}

		public Action<string> OnOpenFile = file => { };

		void OpenFileMenuItemClicked (object sender, EventArgs e)
		{
			OpenSelectedFile ();
		}

		void OpenSelectedFile ()
		{
			try {
				string file = GetFile (SelectedRow);
				if (file != null) {
					OnOpenFile (file);
				}
			} catch (Exception ex) {
				LoggingService.LogError ("Unable to open file.", ex);
			}
		}
	}
}

