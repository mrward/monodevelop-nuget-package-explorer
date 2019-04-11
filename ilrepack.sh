#!/bin/sh

mono ./src/packages/ILRepack.2.0.16/tools/ILRepack.exe \
  /targetPlatform:v4 \
  /lib:src/packages/Newtonsoft.Json.9.0.1/lib/net45/ \
  /out:lib/NuGet.Merged.dll \
  src/packages/NuGet.Commands.4.8.0/lib/net46/NuGet.Commands.dll \
  src/packages/NuGet.Common.4.8.0/lib/net46/NuGet.Common.dll \
  src/packages/NuGet.Configuration.4.8.0/lib/net46/NuGet.Configuration.dll \
  src/packages/NuGet.Credentials.4.8.0/lib/net46/NuGet.Credentials.dll \
  src/packages/NuGet.DependencyResolver.Core.4.8.0/lib/net46/NuGet.DependencyResolver.Core.dll \
  src/packages/NuGet.Frameworks.4.8.0/lib/net46/NuGet.Frameworks.dll \
  src/packages/NuGet.Indexing.4.8.0/lib/net46/NuGet.Indexing.dll \
  src/packages/NuGet.LibraryModel.4.8.0/lib/net46/NuGet.LibraryModel.dll \
  src/packages/NuGet.PackageManagement.4.8.0/lib/net46/NuGet.PackageManagement.dll \
  src/packages/NuGet.Packaging.4.8.0/lib/net46/NuGet.Packaging.dll \
  src/packages/NuGet.Packaging.Core.4.8.0/lib/net46/NuGet.Packaging.Core.dll \
  src/packages/NuGet.ProjectModel.4.8.0/lib/net46/NuGet.ProjectModel.dll \
  src/packages/NuGet.Protocol.4.8.0/lib/net46/NuGet.Protocol.dll \
  src/packages/NuGet.Resolver.4.8.0/lib/net46/NuGet.Resolver.dll \
  src/packages/NuGet.Versioning.4.8.0/lib/net46/NuGet.Versioning.dll \
  src/packages/Lucene.Net.3.0.3/lib/NET40/Lucene.Net.dll
