//
// NuGetPackageDisplayBinding.cs
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

using System.Threading.Tasks;
using MonoDevelop.Components;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui.Documents;

namespace MonoDevelop.NuGetPackageExplorer
{
	[ExportFileDocumentController (
		Id = "NuGetPackageExplorer",
		Name = "NuGet Package Explorer",
		FileExtension = ".nupkg",
		CanUseAsDefault = true,
		InsertBefore = "DefaultDisplayBinding")]
	public class NuGetPackageDisplayBinding : FileDocumentController
	{
		NuGetPackageView packageView;
		FileDescriptor fileDescriptor;

		public NuGetPackageDisplayBinding ()
		{
		}

		public NuGetPackageDisplayBinding (NuGetPackageView packageView)
		{
			this.packageView = packageView;
		}

		protected override bool ControllerIsViewOnly {
			get { return true; }
		}

		protected override async Task OnInitialize (ModelDescriptor modelDescriptor, Properties status)
		{
			await base.OnInitialize (modelDescriptor, status);
			fileDescriptor = (FileDescriptor)modelDescriptor;
		}

		protected override async Task<DocumentView> OnInitializeView ()
		{
			var container = new DocumentViewContainer ();
			container.SupportedModes = DocumentViewContainerMode.Tabs;

			if (packageView == null) {
				packageView = new NuGetPackageView ();
				await packageView.Load (fileDescriptor.FilePath);
			}

			DocumentTitle = packageView.ContentName;

			var control = new XwtControl (packageView);
			AddView (container, control, GettextCatalog.GetString ("Package"));
			AddView (container, packageView.NuSpecFileView.Control, GettextCatalog.GetString ("NuSpec"));

			return container;
		}

		static DocumentViewContent AddView (
			DocumentViewContainer container,
			Control control,
			string title)
		{
			var documentView = new DocumentViewContent (() => control);
			documentView.Title = title;
			container.Views.Add (documentView);

			return documentView;
		}
	}
}

