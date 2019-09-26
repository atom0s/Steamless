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

namespace Steamless.API.Crypto
{
    using System.Runtime.InteropServices;

    public static class CheckSumHelper
    {
        [DllImport("imagehlp", EntryPoint = "MapFileAndCheckSumW", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint MapFileAndCheckSum(string Filename, out uint HeaderSum, out uint CheckSum);

        public enum CheckSum_Result
        {
            /// <summary>
            /// Initializing
            ///
            /// The function succeeded.
            /// </summary>
            CHECKSUM_SUCCESS = 0,

            /// <summary>
            /// Initializing
            ///
            /// Could not open the file.
            /// </summary>
            CHECKSUM_OPEN_FAILURE = 1,

            /// <summary>
            /// Initializing
            ///
            /// Could not map the file.
            /// </summary>
            CHECKSUM_MAP_FAILURE = 2,

            /// <summary>
            /// Initializing
            ///
            /// Could not map a view of the file.
            /// </summary>
            CHECKSUM_MAPVIEW_FAILURE = 3,

            /// <summary>
            /// Initializing
            ///
            /// Could not convert the file name to Unicode.
            /// </summary>
            CHECKSUM_UNICODE_FAILURE = 4
        }
    }
}