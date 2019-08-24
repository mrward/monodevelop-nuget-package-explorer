#!/bin/sh

mono ./src/packages/ILRepack.2.0.17/tools/ILRepack.exe \
  /targetPlatform:v4 \
  /lib:src/packages/Newtonsoft.Json.9.0.1/lib/net45/ \
  /lib:/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/4.7.2-api/ \
  /out:lib/NuGet.Merged.dll \
  src/packages/NuGet.Commands.5.2.0/lib/net472/NuGet.Commands.dll \
  src/packages/NuGet.Common.5.2.0/lib/net472/NuGet.Common.dll \
  src/packages/NuGet.Configuration.5.2.0/lib/net472/NuGet.Configuration.dll \
  src/packages/NuGet.Credentials.5.2.0/lib/net472/NuGet.Credentials.dll \
  src/packages/NuGet.DependencyResolver.Core.5.2.0/lib/net472/NuGet.DependencyResolver.Core.dll \
  src/packages/NuGet.Frameworks.5.2.0/lib/net472/NuGet.Frameworks.dll \
  src/packages/NuGet.Indexing.5.2.0/lib/net472/NuGet.Indexing.dll \
  src/packages/NuGet.LibraryModel.5.2.0/lib/net472/NuGet.LibraryModel.dll \
  src/packages/NuGet.PackageManagement.5.2.0/lib/net472/NuGet.PackageManagement.dll \
  src/packages/NuGet.Packaging.5.2.0/lib/net472/NuGet.Packaging.dll \
  src/packages/NuGet.Packaging.Core.5.2.0/lib/net472/NuGet.Packaging.Core.dll \
  src/packages/NuGet.ProjectModel.5.2.0/lib/net472/NuGet.ProjectModel.dll \
  src/packages/NuGet.Protocol.5.2.0/lib/net472/NuGet.Protocol.dll \
  src/packages/NuGet.Resolver.5.2.0/lib/net472/NuGet.Resolver.dll \
  src/packages/NuGet.Versioning.5.2.0/lib/net472/NuGet.Versioning.dll \
  src/packages/Lucene.Net.3.0.3/lib/NET40/Lucene.Net.dll
