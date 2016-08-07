//
// NuGetPackageMetadataView.cs
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
using MonoDevelop.PackageManagement;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Xwt;

namespace MonoDevelop.NuGetPackageExplorer
{
	public class NuGetPackageMetadataView : Widget
	{
		VBox mainVBox;
		HBox errorHBox;
		Label errorLabel;
		HBox downloadingHBox;
		Button cancelDownloadButton;
		Label packageId;
		Label packageVersion;
		Label packageTitle;
		Label packageAuthors;
		Label packageOwners;
		Label packageTags;
		Label packageLanguage;
		Label packageCopyright;
		Label packageRequireLicenseAcceptance;
		Label packageDevelopmentDependency;
		Label packageMinClientVersion;
		LinkLabel packageLicenseUrl;
		LinkLabel packageProjectUrl;
		LinkLabel packageIconUrl;
		Label packageSummary;
		HBox packageSummaryHBox;
		Label packageDescription;
		Label packageReleaseNotes;
		HBox packageReleaseNotesHBox;
		VBox packageDependenciesVBox;
		HBox packageFrameworkReferencesLabelHBox;
		VBox packageFrameworkReferencesVBox;
		HBox packageReferencesLabelHBox;
		VBox packageReferencesVBox;

		public NuGetPackageMetadataView ()
		{
			Build ();
		}

		public Action OnCancelDownload = () => { };

		void Build ()
		{
			mainVBox = new VBox ();
			mainVBox.Margin = 10;
			Content = mainVBox;

			errorHBox = new HBox ();
			errorLabel = new Label ();
			errorHBox.PackStart (errorLabel);
			mainVBox.PackStart (errorHBox);
			errorHBox.Visible = false;

			downloadingHBox = new HBox ();
			var downloadingLabel = new Label ();
			downloadingLabel.Text = GettextCatalog.GetString ("Downloading...");
			downloadingHBox.PackStart (downloadingLabel);
			cancelDownloadButton = new Button ();
			cancelDownloadButton.Label = GettextCatalog.GetString ("Cancel");
			cancelDownloadButton.Clicked += CancelDownloadButtonClicked;
			downloadingHBox.PackStart (cancelDownloadButton);
			mainVBox.PackStart (downloadingHBox);
			downloadingHBox.Visible = false;

			packageId = AddMetadata (GettextCatalog.GetString ("Id"));
			packageVersion = AddMetadata (GettextCatalog.GetString ("Version"));
			packageTitle = AddMetadata (GettextCatalog.GetString ("Title"));
			packageAuthors = AddMetadata (GettextCatalog.GetString ("Authors"));
			packageOwners = AddMetadata (GettextCatalog.GetString ("Owners"));
			packageTags = AddMetadata (GettextCatalog.GetString ("Tags"));
			packageLanguage = AddMetadata (GettextCatalog.GetString ("Language"));
			packageCopyright = AddMetadata (GettextCatalog.GetString ("Copyright"));
			packageLicenseUrl = AddMetadataUrl (GettextCatalog.GetString ("License"));
			packageProjectUrl = AddMetadataUrl (GettextCatalog.GetString ("Project Page"));
			packageIconUrl = AddMetadataUrl (GettextCatalog.GetString ("Icon"));
			packageRequireLicenseAcceptance = AddMetadata (GettextCatalog.GetString ("Require License Acceptance"));
			packageDevelopmentDependency = AddMetadata (GettextCatalog.GetString ("Development Dependency"));
			packageMinClientVersion = AddMetadata (GettextCatalog.GetString ("Minimum Client Version"));

			// Summary.
			AddMetadataHBox (GettextCatalog.GetString ("Summary"));
			packageSummaryHBox = new HBox ();
			mainVBox.PackStart (packageSummaryHBox);
			packageSummary = new Label ();
			packageSummary.Wrap = WrapMode.Word;
			packageSummaryHBox.PackStart (packageSummary, true);

			// Description.
			AddMetadataHBox (GettextCatalog.GetString ("Description"));
			var hbox = new HBox ();
			mainVBox.PackStart (hbox);
			packageDescription = new Label ();
			packageDescription.Wrap = WrapMode.Word;
			hbox.PackStart (packageDescription, true);

			// Release Notes.
			AddMetadataHBox (GettextCatalog.GetString ("Release Notes"));
			packageReleaseNotesHBox = new HBox ();
			mainVBox.PackStart (packageReleaseNotesHBox);
			packageReleaseNotes = new Label ();
			packageReleaseNotes.Wrap = WrapMode.Word;
			packageReleaseNotesHBox.PackStart (packageReleaseNotes, true);

			// Dependencies.
			AddMetadataHBox (GettextCatalog.GetString ("Dependencies"));
			packageDependenciesVBox = new VBox ();
			mainVBox.PackStart (packageDependenciesVBox);

			// Framework assembly references.
			packageFrameworkReferencesLabelHBox = AddMetadataHBox (GettextCatalog.GetString ("Framework Assembly References"));
			packageFrameworkReferencesVBox = new VBox ();
			mainVBox.PackStart (packageFrameworkReferencesVBox);
			packageFrameworkReferencesLabelHBox.Visible = false;

			// References.
			packageReferencesLabelHBox = AddMetadataHBox (GettextCatalog.GetString ("References"));
			packageReferencesVBox = new VBox ();
			mainVBox.PackStart (packageReferencesVBox);
			packageReferencesLabelHBox.Visible = false;
		}

		static string BoldText (string text)
		{
			return string.Format ("<b>{0}</b>", text);
		}

		HBox AddMetadataHBox (string name)
		{
			var hbox = new HBox ();
			mainVBox.PackStart (hbox);

			var label = new Label ();
			label.Markup = BoldText (name);
			hbox.PackStart (label);

			return hbox;
		}

		Label AddMetadata (string name)
		{
			HBox hbox = AddMetadataHBox (name);

			var metadataValueLabel = new Label ();
			hbox.PackStart (metadataValueLabel);

			return metadataValueLabel;
		}

		LinkLabel AddMetadataUrl (string name)
		{
			HBox hbox = AddMetadataHBox (name);

			var metadataValueLabel = new LinkLabel ();
			hbox.PackStart (metadataValueLabel);

			return metadataValueLabel;
		}

		public void ShowMetadata (NuspecReader reader)
		{
			packageId.Text = reader.GetId ();
			packageVersion.Text = reader.GetVersion ().ToNormalizedString ();
			packageLanguage.Text = reader.GetLanguage ();
			packageCopyright.Text = reader.GetMetadataValue ("copyright");
			packageTitle.Text = reader.GetMetadataValue ("title");
			packageTags.Text = reader.GetMetadataValue ("tags");
			packageAuthors.Text = reader.GetMetadataValue ("authors");
			packageOwners.Text = reader.GetMetadataValue ("owners");
			packageRequireLicenseAcceptance.Text = reader.GetMetadataValue ("requireLicenseAcceptance");
			packageDevelopmentDependency.Text = reader.GetDevelopmentDependency ().ToString ();
			UpdateLinkLabel (packageLicenseUrl, reader, "licenseUrl");
			UpdateLinkLabel (packageProjectUrl, reader, "projectUrl");
			UpdateLinkLabel (packageIconUrl, reader, "iconUrl");
			packageSummary.Text = reader.GetMetadataValue ("summary");
			packageSummaryHBox.Visible = !String.IsNullOrEmpty (packageSummary.Text);
			packageDescription.Text = reader.GetMetadataValue ("description");
			packageReleaseNotes.Text = reader.GetMetadataValue ("releaseNotes");
			packageReleaseNotesHBox.Visible = !String.IsNullOrEmpty (packageReleaseNotes.Text);

			ShowDependencies (reader);
			ShowFrameworkReferences (reader);
			ShowFilteredAssemblyReferences (reader);

			NuGetVersion version = reader.GetMinClientVersion ();
			if (version != null)
				packageMinClientVersion.Text = version.ToNormalizedString ();
		}

		internal void ShowMetadata (PackageSearchResultViewModel package)
		{
			packageId.Text = package.Id;
			packageVersion.Text = package.Version.ToNormalizedString ();
			packageTitle.Text = package.Title;
			packageAuthors.Text = package.Author;
			packageOwners.Text = package.PackageMetadata?.Owners ?? string.Empty;
			packageSummary.Text = package.Summary;
			packageSummaryHBox.Visible = !String.IsNullOrEmpty (packageSummary.Text);
			packageDescription.Text = package.Description;
			packageTags.Text = package.PackageMetadata?.Tags ?? string.Empty;

			UpdateLinkLabel (packageLicenseUrl, package.LicenseUrl);
			UpdateLinkLabel (packageProjectUrl, package.ProjectUrl);
			UpdateLinkLabel (packageIconUrl, package.IconUrl);

			if (package.PackageMetadata?.HasDependencies == true) {
				ShowDependencies (package.PackageMetadata.DependencySets.Select (d => d.DependencyGroup));
			}
		}

		static void UpdateLinkLabel (LinkLabel label, NuspecReader reader, string metadataName)
		{
			string url = reader.GetMetadataValue (metadataName);
			UpdateLinkLabel (label, url);
		}

		static void UpdateLinkLabel (LinkLabel label, string url)
		{
			if (!string.IsNullOrEmpty (url)) {
				UpdateLinkLabel (label, new Uri (url));
			}
		}

		static void UpdateLinkLabel (LinkLabel label, Uri url)
		{
			if (url != null) {
				label.Uri = url;
				label.Text = url.ToString ();
			}
		}

		void ShowDependencies (NuspecReader reader)
		{
			var dependencyGroups = reader.GetDependencyGroups ().ToArray ();
			ShowDependencies (dependencyGroups);
		}

		void ShowDependencies (IEnumerable<PackageDependencyGroup> dependencyGroups)
		{
			packageDependenciesVBox.Clear ();

			if (!dependencyGroups.Any ()) {
				AddNoDependenciesLabel ();
				return;
			}

			foreach (PackageDependencyGroup dependencyGroup in dependencyGroups) {
				ShowDependencyGroup (dependencyGroup);
			}
		}

		void AddNoDependenciesLabel (double leftMargin = 20)
		{
			var label = new Label (GettextCatalog.GetString ("No dependencies"));
			label.MarginLeft = leftMargin;
			packageDependenciesVBox.PackStart (label);
		}

		void ShowDependencyGroup (PackageDependencyGroup dependencyGroup)
		{
			double leftMargin = 20;

			if (dependencyGroup.TargetFramework.IsSpecificFramework ()) {
				var label = new Label (GettextCatalog.GetString (dependencyGroup.TargetFramework.DotNetFrameworkName));
				label.MarginLeft = leftMargin;
				packageDependenciesVBox.PackStart (label);

				leftMargin += 20;
			}

			PackageDependency[] packages = dependencyGroup.Packages.ToArray ();
			if (!packages.Any ()) {
				AddNoDependenciesLabel (leftMargin);
				return;
			}

			var formatter = new VersionRangeFormatter ();

			foreach (PackageDependency package in dependencyGroup.Packages) {
				string text = String.Format("{0} {1}", package.Id, package.VersionRange.ToString ("P", formatter));
				var label = new Label (text);
				label.MarginLeft = leftMargin;
				packageDependenciesVBox.PackStart (label);
			}
		}

		void ShowFrameworkReferences (NuspecReader reader)
		{
			var frameworkReferenceGroups = reader.GetFrameworkReferenceGroups ().ToArray ();
			if (!frameworkReferenceGroups.Any ())
				return;

			packageFrameworkReferencesLabelHBox.Visible = true;

			foreach (FrameworkSpecificGroup frameworkSpecificGroup in frameworkReferenceGroups) {
				ShowFrameworkSpecificGroup (frameworkSpecificGroup, packageFrameworkReferencesVBox);
			}
		}

		void ShowFrameworkSpecificGroup (FrameworkSpecificGroup frameworkSpecificGroup, VBox vbox)
		{
			double leftMargin = 20;
			if (frameworkSpecificGroup.TargetFramework.IsSpecificFramework () ) {
				var label = new Label (GettextCatalog.GetString (frameworkSpecificGroup.TargetFramework.DotNetFrameworkName));
				label.MarginLeft = leftMargin;
				vbox.PackStart (label);

				leftMargin += 20;
			}

			foreach (string frameworkReference in frameworkSpecificGroup.Items) {
				var label = new Label (frameworkReference);
				label.MarginLeft = leftMargin;
				vbox.PackStart (label);
			}
		}

		void ShowFilteredAssemblyReferences (NuspecReader reader)
		{
			var referenceGroups = reader.GetReferenceGroups ().ToArray ();
			if (!referenceGroups.Any ())
				return;

			packageReferencesLabelHBox.Visible = true;

			foreach (FrameworkSpecificGroup frameworkSpecificGroup in referenceGroups) {
				ShowFrameworkSpecificGroup (frameworkSpecificGroup, packageReferencesVBox);
			}
		}

		protected override void Dispose (bool disposing)
		{
			cancelDownloadButton.Clicked -= CancelDownloadButtonClicked;

			base.Dispose (disposing);
		}

		void CancelDownloadButtonClicked (object sender, EventArgs e)
		{
			OnCancelDownload ();
		}

		public bool IsDownloading {
			get { return downloadingHBox.Visible; }
			set { downloadingHBox.Visible = value; }
		}

		public void ShowError (string message)
		{
			errorLabel.Text = message;
			errorHBox.Visible = true;
		}
	}
}

