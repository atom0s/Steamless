/**
 * Steamless - Copyright (c) 2015 - 2019 atom0s [atom0s@live.com]
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

namespace Steamless.Model
{
    using API;
    using API.Model;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows;
    using ViewModel;

    public class DataService : IDataService
    {
        private static readonly Version SteamlessApiVersion = new Version(1, 0);

        /// <summary>
        /// Obtains the version of Steamless.
        /// </summary>
        /// <returns></returns>
        public Task<Version> GetSteamlessVersion()
        {
            return Task.Run(() =>
                {
                    try
                    {
                        return Assembly.GetExecutingAssembly().EntryPoint.DeclaringType?.Assembly.GetName().Version ?? new Version(0, 0, 0, 0);
                    }
                    catch
                    {
                        return new Version(0, 0, 0, 0);
                    }
                });
        }

        /// <summary>
        /// Obtains a list of available Steamless plugins.
        /// </summary>
        /// <returns></returns>
        public Task<List<SteamlessPlugin>> GetSteamlessPlugins()
        {
            return Task.Run(() =>
                {
                    try
                    {
                        // Obtain the view model locator..
                        var vml = Application.Current.FindResource("ViewModelLocator") as ViewModelLocator;
                        if (vml == null)
                            throw new Exception("Failed to obtain ViewModelLocator.");

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
                                {
                                    Debug.WriteLine($"Failed to load plugin; could not find SteamlessPlugin base class. ({Path.GetFileName(dll)})");
                                    continue;
                                }

                                // Locate the SteamlessApiVersion attribute on the base class..
                                var baseAttr = baseClass.GetCustomAttributes(typeof(SteamlessApiVersionAttribute), false);
                                if (baseAttr.Length == 0)
                                {
                                    Debug.WriteLine($"Failed to load plugin; could not find SteamlessApiVersion attribute. ({Path.GetFileName(dll)})");
                                    continue;
                                }

                                // Validate the interface version..
                                var apiVersion = (SteamlessApiVersionAttribute)baseAttr[0];
                                if (apiVersion.Version != SteamlessApiVersion)
                                {
                                    Debug.WriteLine($"Failed to load plugin; invalid API version is being used. ({Path.GetFileName(dll)})");
                                    continue;
                                }

                                // Create an instance of the plugin..
                                var plugin = (SteamlessPlugin)Activator.CreateInstance(baseClass);
                                if (!plugin.Initialize(vml.LoggingService))
                                {
                                    Debug.WriteLine($"Failed to load plugin; plugin failed to initialize. ({Path.GetFileName(dll)})");
                                    continue;
                                }

                                plugins.Add(plugin);
                            }
                            catch
                            {
                                Debug.WriteLine($"Failed to load DLL as a Steamless plugin: ({Path.GetFileName(dll)})");
                            }
                        }

                        // Order the plugins by their name..
                        return plugins.OrderBy(p => p.Name).ToList();
                    }
                    catch
                    {
                        return new List<SteamlessPlugin>();
                    }
                });
        }
    }
}