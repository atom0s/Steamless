/**
 * Steamless - Copyright (c) 2015 - 2018 atom0s [atom0s@live.com]
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

namespace Steamless.API.PE64
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

    public class Pe64Helpers
    {
        /// <summary>
        /// Converts a byte array to the given structure type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static T GetStructure<T>(byte[] data, int offset = 0)
        {
            var ptr = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, offset, ptr, data.Length - offset);
            var obj = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);

            return obj;
        }

        /// <summary>
        /// Converts the given object back to a byte array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] GetStructureBytes<T>(T obj)
        {
            var size = Marshal.SizeOf(obj);
            var data = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, data, 0, size);
            Marshal.FreeHGlobal(ptr);
            return data;
        }

        /// <summary>
        /// Obtains a section from the given file information.
        /// </summary>
        /// <param name="rawData"></param>
        /// <param name="index"></param>
        /// <param name="dosHeader"></param>
        /// <param name="ntHeaders"></param>
        /// <returns></returns>
        public static NativeApi64.ImageSectionHeader64 GetSection(byte[] rawData, int index, NativeApi64.ImageDosHeader64 dosHeader, NativeApi64.ImageNtHeaders64 ntHeaders)
        {
            var sectionSize = Marshal.SizeOf(typeof(NativeApi64.ImageSectionHeader64));
            var optionalHeaderOffset = Marshal.OffsetOf(typeof(NativeApi64.ImageNtHeaders64), "OptionalHeader").ToInt64();
            var dataOffset = dosHeader.e_lfanew + optionalHeaderOffset + ntHeaders.FileHeader.SizeOfOptionalHeader;

            return GetStructure<NativeApi64.ImageSectionHeader64>(rawData, (int)dataOffset + (index * sectionSize));
        }

        /// <summary>
        /// Scans the given data for the given pattern.
        /// 
        /// Notes:
        ///     Patterns are assumed to be 2 byte hex values with spaces.
        ///     Wildcards are represented by ??.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static uint FindPattern(byte[] data, string pattern)
        {
            try
            {
                // Trim the pattern from extra whitespace..
                var trimPattern = pattern.Replace(" ", "").Trim();

                // Convert the pattern to a byte array..
                var patternMask = new List<bool>();
                var patternData = Enumerable.Range(0, trimPattern.Length).Where(x => x % 2 == 0)
                                            .Select(x =>
                                            {
                                                var bt = trimPattern.Substring(x, 2);
                                                patternMask.Add(!bt.Contains('?'));
                                                return bt.Contains('?') ? (byte)0 : Convert.ToByte(bt, 16);
                                            }).ToArray();

                // Scan the given data for our pattern..
                for (var x = 0; x < data.Length; x++)
                {
                    if (!patternData.Where((t, y) => patternMask[y] && t != data[x + y]).Any())
                        return (uint)x;
                }

                return 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}