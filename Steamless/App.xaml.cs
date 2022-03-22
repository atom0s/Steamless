/**
 * Steamless - Copyright (c) 2015 - 2022 atom0s [atom0s@live.com]
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

namespace Steamless
{
    using System;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public App()
        {
            // Override the assembly resolve event..
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }

        /// <summary>
        /// Assembly resolve override to allow loading of embedded modules.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Obtain the name of the assembly being loaded..
            var name = args.Name.Contains(",") ? args.Name.Substring(0, args.Name.IndexOf(",", StringComparison.InvariantCultureIgnoreCase)) : args.Name.Replace(".dll", "");

            // Ignore resource assembly loading..
            if (name.ToLower().EndsWith(".resources"))
                return null;

            // Build a full path to the possible embedded file..
            var fullName = $"{Assembly.GetExecutingAssembly().EntryPoint.DeclaringType?.Namespace}.Embedded.{new AssemblyName(args.Name).Name}.dll";
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullName))
            {
                // If not embedded try to load from the plugin folder..
                if (stream == null)
                {
                    var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", name + ".dll");
                    return File.Exists(file) ? Assembly.Load(File.ReadAllBytes(file)) : null;
                }

                // Read and load the embedded resource..
                var data = new byte[stream.Length];
                stream.Read(data, 0, (int)stream.Length);
                return Assembly.Load(data);
            }
        }
    }
}