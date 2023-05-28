/**
 * Steamless - Copyright (c) 2015 - 2023 atom0s [atom0s@live.com]
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

namespace Steamless.Unpacker.Variant20.x86.Classes
{
    using System;

    public static class SteamStubHelpers
    {
        /// <summary>
        /// Xor decrypts the given data starting with the given key, if any.
        /// 
        /// @note    If no key is given (0) then the first key is read from the first
        ///          4 bytes inside of the data given.
        /// </summary>
        /// <param name="data">The data to xor decode.</param>
        /// <param name="size">The size of the data to decode.</param>
        /// <param name="key">The starting xor key to decode with.</param>
        /// <returns></returns>
        public static uint SteamXor(ref byte[] data, uint size, uint key = 0)
        {
            var offset = (uint)0;

            // Read the first key as the base xor key if we had none given..
            if (key == 0)
            {
                offset += 4;
                key = BitConverter.ToUInt32(data, 0);
            }

            // Decode the data..
            for (var x = offset; x < size; x += 4)
            {
                var val = BitConverter.ToUInt32(data, (int)x);
                Array.Copy(BitConverter.GetBytes(val ^ key), 0, data, x, 4);

                key = val;
            }

            return key;
        }
    }
}