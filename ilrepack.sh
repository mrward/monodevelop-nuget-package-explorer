#!/bin/sh

nuget install packages.config -OutputDirectory packages

MDBinDir="/Applications/Visual Studio (Preview).app/Contents/MonoBundle"

mono ./packages/ILRepack.MSBuild.Task.2.0.13/tools/ilrepack.exe \
  /lib:"$MDBinDir" \
  /out:lib/NuGet.Merged.dll \
  "$MDBinDir/NuGet.Commands.dll" \
  "$MDBinDir/NuGet.Common.dll" \
  "$MDBinDir/NuGet.Configuration.dll" \
  "$MDBinDir/NuGet.Credentials.dll" \
  "$MDBinDir/NuGet.DependencyResolver.Core.dll" \
  "$MDBinDir/NuGet.Frameworks.dll" \
  "$MDBinDir/NuGet.Indexing.dll" \
  "$MDBinDir/NuGet.LibraryModel.dll" \
  "$MDBinDir/NuGet.PackageManagement.dll" \
  "$MDBinDir/NuGet.Packaging.dll" \
  "$MDBinDir/NuGet.ProjectModel.dll" \
  "$MDBinDir/NuGet.Protocol.dll" \
  "$MDBinDir/NuGet.Resolver.dll" \
  "$MDBinDir/NuGet.Versioning.dll" \
  "$MDBinDir/Lucene.Net.dll"
