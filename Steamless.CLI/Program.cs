/**
 * Steamless - Copyright (c) 2015 - 2024 atom0s [atom0s@live.com]
 *
 * This work is licensed under the Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International License.
 * To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-nd/4.0/ or send a letter to
 * Creative Commons, PO Box 1866, Mountain View, CA 94042, USA.
 *
 * By using Steamless, you agree to the above license and its terms.
 *
 *      Attribution - You must give appropriate credit, provide a link to the license and indicate if changes were
 *                    made. You must do so in any reasonable manner, but not in any way that suggests the licensor
 *                    endorses you or your use.
 *
 *   Non-Commercial - You may not use the material (Steamless) for commercial purposes.
 *
 *   No-Derivatives - If you remix, transform, or build upon the material (Steamless), you may not distribute the
 *                    modified material. You are, however, allowed to submit the modified works back to the original
 *                    Steamless project in attempt to have it added to the original project.
 *
 * You may not apply legal terms or technological measures that legally restrict others
 * from doing anything the license permits.
 *
 * No warranties are given.
 */

namespace Steamless.CLI
{
    using Steamless.API;
    using Steamless.API.Events;
    using Steamless.API.Model;
    using Steamless.API.Services;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    internal class Program
    {
        /// <summary>
        /// Steamless API Version
        /// 
        /// Main define for this is within DataService.cs and should match that value.
        /// </summary>
        private static readonly Version SteamlessApiVersion = new Version(1, 0);

        /// <summary>
        /// Prints the Steamless header information.
        /// </summary>
        static void PrintHeader()
        {
            Console.WriteLine("  _________ __                        .__                        ");
            Console.WriteLine(" /   _____//  |_  ____ _____    _____ |  |   ____   ______ ______");
            Console.WriteLine(" \\_____  \\\\   __\\/ __ \\\\__  \\  /     \\|  | _/ __ \\ /  ___//  ___/");
            Console.WriteLine(" /        \\|  | \\  ___/ / __ \\|  Y Y  \\  |_\\  ___/ \\___ \\ \\___ \\ ");
            Console.WriteLine("/_______  /|__|  \\___  >____  /__|_|  /____/\\___  >____  >____  >");
            Console.WriteLine("        \\/           \\/     \\/      \\/          \\/     \\/     \\/ \n");
            Console.WriteLine("Steamless - SteamStub DRM Remover");
            Console.WriteLine("by atom0s\n");
            Console.WriteLine("GitHub    : https://github.com/atom0s/Steamless");
            Console.WriteLine("Homepage  : https://atom0s.com");
            Console.WriteLine("Donations : https://paypal.me/atom0s");
            Console.WriteLine("Donations : https://github.com/sponsors/atom0s");
            Console.WriteLine("Donations : https://patreon.com/atom0s\n");
        }

        /// <summary>
        /// Prints the Steamless command line help information.
        /// </summary>
        static void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("    Steamless.CLI.exe [options] [file]\n\n");
            Console.WriteLine("Options:");
            Console.WriteLine("    --quiet          - Disables output of debug log messages.");
            Console.WriteLine("    --keepbind       - Keeps the .bind section in the unpacked file.");
            Console.WriteLine("    --keepstub       - Keeps the DOS stub in the unpacked file.");
            Console.WriteLine("    --dumppayload    - Dumps the stub payload to disk.");
            Console.WriteLine("    --dumpdrmp       - Dumps the SteamDRMP.dll to disk.");
            Console.WriteLine("    --realign        - Realigns the unpacked file sections.");
            Console.WriteLine("    --recalcchecksum - Recalculates the unpacked file checksum.");
            Console.WriteLine("    --exp            - Use experimental features.");
        }

        /// <summary>
        /// Obtains a list of available Steamless plugins.
        /// </summary>
        /// <returns></returns>
        static List<SteamlessPlugin> GetSteamlessPlugins(LoggingService logService)
        {
            try
            {
                // The list of valid plugins..
                var plugins = new List<SteamlessPlugin>();

                // Build a path to the plugins folder..
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");

                // Loop the DLL files and attempt to load them..
                foreach (var dll in Directory.GetFiles(path, "*.dll"))
                {
                    // Skip the Steamless.API.dll file..
                    if (dll.ToLower().Contains("steamless.api.dll"))
                        continue;

                    try
                    {
                        // Load the assembly..
                        var asm = Assembly.Load(File.ReadAllBytes(dll));

                        // Locate the class inheriting the plugin base..
                        var baseClass = asm.GetTypes().SingleOrDefault(t => t.BaseType == typeof(SteamlessPlugin));
                        if (baseClass == null)
                            continue;

                        // Locate the SteamlessApiVersion attribute on the base class..
                        var baseAttr = baseClass.GetCustomAttributes(typeof(SteamlessApiVersionAttribute), false);
                        if (baseAttr.Length == 0)
                            continue;

                        // Validate the interface version..
                        var apiVersion = (SteamlessApiVersionAttribute)baseAttr[0];
                        if (apiVersion.Version != SteamlessApiVersion)
                            continue;

                        // Create an instance of the plugin..
                        var plugin = (SteamlessPlugin)Activator.CreateInstance(baseClass);
                        if (!plugin.Initialize(logService))
                            continue;

                        plugins.Add(plugin);
                    }
                    catch
                    {
                    }
                }

                // Order the plugins by their name..
                return plugins.OrderBy(p => p.Name).ToList();
            }
            catch
            {
                return new List<SteamlessPlugin>();
            }
        }

        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args"></param>
        static int Main(string[] args)
        {
            // AssemblyResolve override to load modules from the Plugins folder..
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                // Obtain the name of the assembly being loaded..
                var name = e.Name.Contains(",") ? e.Name.Substring(0, e.Name.IndexOf(",", StringComparison.InvariantCultureIgnoreCase)) : e.Name.Replace(".dll", "");

                // Ignore resource assembly loading..
                if (name.ToLower().EndsWith(".resources"))
                    return null;

                // Build a full path to the possible embedded file..
                var fullName = $"{Assembly.GetExecutingAssembly().EntryPoint.DeclaringType?.Namespace}.Embedded.{new AssemblyName(e.Name).Name}.dll";
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullName))
                {
                    // If not embedded try to load from the plugin folder..
                    if (stream == null)
                    {
                        var f = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", name + ".dll");
                        return File.Exists(f) ? Assembly.Load(File.ReadAllBytes(f)) : null;
                    }

                    // Read and load the embedded resource..
                    var data = new byte[stream.Length];
                    stream.Read(data, 0, (int)stream.Length);
                    return Assembly.Load(data);
                }
            };

            return Program.Run(args);
        }

        /// <summary>
        /// Runs the Steamless command line operations.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static int Run(string[] args)
        {
            var logService = new LoggingService();
            var opts = new SteamlessOptions();
            var file = string.Empty;

            // Prepare the logging service..
            logService.AddLogMessage += (sender, e) =>
            {
                if (!opts.VerboseOutput && e.MessageType == LogMessageType.Debug)
                    return;

                try
                {
                    if (sender != null)
                        e.Message = $"[{sender.GetType().Assembly.GetName().Name}] {e.Message}";
                    else
                        e.Message = $"[Steamless] {e.Message}";
                }
                catch
                {
                }

                Console.WriteLine(e.Message);
            };

            // Print the program header..
            Program.PrintHeader();

            // Process command line arguments for the various Steamless options..
            foreach (var arg in args)
            {
                if (arg.ToLower() == "--quiet")
                    opts.VerboseOutput = false;
                if (arg.ToLower() == "--keepbind")
                    opts.KeepBindSection = true;
                if (arg.ToLower() == "--keepstub")
                    opts.ZeroDosStubData = false;
                if (arg.ToLower() == "--dumppayload")
                    opts.DumpPayloadToDisk = true;
                if (arg.ToLower() == "--dumpdrmp")
                    opts.DumpSteamDrmpToDisk = true;
                if (arg.ToLower() == "--realign")
                    opts.DontRealignSections = false;
                if (arg.ToLower() == "--recalcchecksum")
                    opts.RecalculateFileChecksum = true;
                if (arg.ToLower() == "--exp")
                    opts.UseExperimentalFeatures = true;
                if (!arg.StartsWith("--"))
                    file = arg;
            }

            // Ensure an input file was given..
            if (string.IsNullOrEmpty(file))
            {
                Program.PrintHelp();
                return 1;
            }

            // Ensure the input file exists..
            if (!File.Exists(file))
            {
                logService.OnAddLogMessage(null, new LogMessageEventArgs("Invalid input file given; cannot continue.", LogMessageType.Error));
                return 1;
            }

            // Collect the list of available plugins..
            var plugins = GetSteamlessPlugins(logService);
            plugins.ForEach(p => logService.OnAddLogMessage(null, new LogMessageEventArgs($"Loaded plugin: {p.Name} - by {p.Author} (v.{p.Version})", LogMessageType.Success)));

            // Ensure plugins were found and loaded..
            if (plugins.Count == 0)
            {
                logService.OnAddLogMessage(null, new LogMessageEventArgs("No plugins were loaded; be sure to fully extract Steamless before running!", LogMessageType.Error));
                return 1;
            }

            // Loop through the plugins and try to unpack the file..
            foreach (var p in plugins)
            {
                // Check if the plugin can process the file..
                if (p.CanProcessFile(file))
                {
                    var ret = p.ProcessFile(file, opts);

                    logService.OnAddLogMessage(null, !ret
                        ? new LogMessageEventArgs("Failed to unpack file.", LogMessageType.Error)
                        : new LogMessageEventArgs("Successfully unpacked file!", LogMessageType.Success));

                    if (ret) return 0;
                }
            }

            logService.OnAddLogMessage(null, new LogMessageEventArgs("All unpackers failed to unpack file.", LogMessageType.Error));
            return 1;
        }
    }
}