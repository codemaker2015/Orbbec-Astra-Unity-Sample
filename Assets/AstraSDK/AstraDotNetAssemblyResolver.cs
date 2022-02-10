﻿using System;
using System.IO;
using System.Reflection;

public static class AstraDotNetAssemblyResolver
{
    /// <summary>
    /// Adds assembly resolve hook to <paramref name="appDomain"/>.
    /// Call this method before any call to method that uses <c>AstraDotNet</c> inside his body.
    /// </summary>
    /// <param name="appDomain">Application domain. If null (default value) then current application domain is used.</param>
    public static void Init(AppDomain appDomain = null)
    {
        if (appDomain == null)
            appDomain = AppDomain.CurrentDomain;
        appDomain.AssemblyResolve += HandleAssemblyResolve;
    }

    public static string AssemblyName { get; set; } = "AstraDotNet";
    public static string DirName { get; set; } = "Astra";
    public static string SubDir32bit { get; set; } = "x86";
    public static string SubDir64bit { get; set; } = "amd64";

    private static Assembly HandleAssemblyResolve(object sender, ResolveEventArgs args)
    {
        if (args.Name != null
            && args.Name.IndexOf(".resources,", StringComparison.InvariantCultureIgnoreCase) < 0
            && args.Name.StartsWith(AssemblyName, StringComparison.InvariantCultureIgnoreCase))
        {
            var appDomain = (AppDomain)sender;
            var path = Path.Combine(appDomain.BaseDirectory, DirName, Environment.Is64BitProcess ? SubDir64bit : SubDir32bit);
            path = Path.Combine(path, AssemblyName + ".dll");
            if (File.Exists(path))
                return Assembly.LoadFrom(path);
        }
        return null;
    }
}

