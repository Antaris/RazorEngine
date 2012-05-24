//-----------------------------------------------------------------------------
// <copyright file="SharedAssemblyInfo.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
using System.Reflection;

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("RazorEngine Project")]

[assembly: AssemblyProduct("RazorEngine")]
[assembly: AssemblyCopyright("Copyright © RazorEngine Project 2011")]

[assembly: AssemblyVersion("3.0.8.0")]
[assembly: AssemblyFileVersion("3.0.8.0")]