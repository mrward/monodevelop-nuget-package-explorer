//
// NuSpecFileView.cs
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
using System.Xml.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using MonoDevelop.Components;
using MonoDevelop.Ide.Composition;

namespace MonoDevelop.NuGetPackageExplorer
{
	public class NuSpecFileView : IDisposable
	{
		ICocoaTextViewHost textViewHost;

		public NuSpecFileView ()
		{
			var textBufferFactory = CompositionManager.Instance.GetExportedValue<ITextBufferFactoryService> ();
			var contentTypeRegistry = CompositionManager.Instance.GetExportedValue<IContentTypeRegistryService> ();

			IContentType contentType = contentTypeRegistry.GetContentType ("xml");

			var optionsFactory = CompositionManager.Instance.GetExportedValue<IEditorOptionsFactoryService> ();
			IEditorOptions options = optionsFactory.CreateOptions ();
			options.SetOptionValue (DefaultTextViewHostOptions.ChangeTrackingId, false);
			options.SetOptionValue (DefaultTextViewHostOptions.GlyphMarginId, false);
			options.SetOptionValue (DefaultTextViewHostOptions.LineNumberMarginId, false);
			options.SetOptionValue (DefaultTextViewHostOptions.OutliningMarginId, false);
			options.SetOptionValue (DefaultTextViewHostOptions.ShowEnhancedScrollBarOptionId, false);
			options.SetOptionValue (DefaultCocoaViewOptions.ZoomLevelId, ZoomConstants.DefaultZoom);

			var editorFactory = CompositionManager.Instance.GetExportedValue<ICocoaTextEditorFactoryService> ();
			ITextViewRoleSet roles = editorFactory.CreateTextViewRoleSet (
				PredefinedTextViewRoles.Interactive
			);
			ITextBuffer textBuffer = textBufferFactory.CreateTextBuffer (string.Empty, contentType);
			ICocoaTextView textView = editorFactory.CreateTextView (textBuffer, roles, options);
			textViewHost = editorFactory.CreateTextViewHost (textView, true);
		}

		public Control Control {
			get { return textViewHost.HostControl; }
		}

		public void ShowXml (XDocument document)
		{
			ITextBuffer textBuffer = textViewHost.TextView.TextBuffer;
			var span = new Span (0, textBuffer.CurrentSnapshot.Length);
			textBuffer.Replace (span, document.ToString ());
		}

		public void Dispose ()
		{
			if (textViewHost != null) {
				textViewHost.Close ();
				textViewHost = null;
			}
		}
	}
}

