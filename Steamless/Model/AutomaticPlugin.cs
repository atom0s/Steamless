/**
 * Steamless - Copyright (c) 2015 - 2017 atom0s [atom0s@live.com]
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
    using API.Services;
    using System;
    using System.Linq;
    using System.Windows;
    using ViewModel;

    [SteamlessApiVersion(1, 0)]
    internal class AutomaticPlugin : SteamlessPlugin
    {
        /// <summary>
        /// Gets the author of this plugin.
        /// </summary>
        public override string Author => "Steamless Development Team";

        /// <summary>
        /// Gets the name of this plugin.
        /// </summary>
        public override string Name => "Automatic";

        /// <summary>
        /// Gets the description of this plugin.
        /// </summary>
        public override string Description => "Automatically finds which plugin to use for the given file.";

        /// <summary>
        /// Gets the version of this plugin.
        /// </summary>
        public override Version Version => new Version(1, 0, 0, 0);

        /// <summary>
        /// Initialize function called when this plugin is first loaded.
        /// </summary>
        /// <param name="logService"></param>
        /// <returns></returns>
        public override bool Initialize(LoggingService logService)
        {
            return true;
        }

        /// <summary>
        /// Processing function called when a file is being unpacked. Allows plugins to check the file
        /// and see if it can handle the file for its intended purpose.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public override bool CanProcessFile(string file)
        {
            return true;
        }

        /// <summary>
        /// Processing function called to allow the plugin to process the file.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override bool ProcessFile(string file, SteamlessOptions options)
        {
            // Obtain the view model locator..
            var vml = Application.Current.FindResource("ViewModelLocator") as ViewModelLocator;

            // Obtain the plugins..
            var plugins = vml?.MainWindow.Plugins;
            if (plugins == null || plugins.Count == 0)
                return false;

            // Query the plugin list for a plugin to process the file..
            return (from p in plugins where p != this where p.CanProcessFile(file) select p.ProcessFile(file, options)).FirstOrDefault();
        }
    }
}