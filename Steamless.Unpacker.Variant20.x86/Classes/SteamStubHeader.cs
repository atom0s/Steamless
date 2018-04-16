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

namespace Steamless.Unpacker.Variant20.x86.Classes
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// SteamStub DRM Variant 2.0 Header
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SteamStub32Var20Header
    {
        public uint XorKey; // The base XOR key, if defined, to unpack the file with.
        public uint GetModuleHandleA_idata; // The address of GetModuleHandleA inside of the .idata section.
        public uint GetModuleHandleW_idata; // The address of GetModuleHandleW inside of the .idata section.
        public uint GetProcAddress_idata; // The address of GetProcAddress inside of the .idata section.
        public uint LoadLibraryA_idata; // The address of LoadLibraryA inside of the .idata section.
        public uint Unknown0000; // Unknown (Was 0 when testing. Possibly LoadLibraryW.)
        public uint BindSectionVirtualAddress; // The virtual address to the .bind section.
        public uint BindStartFunctionSize; // The size of the start function from the .bind section.
        public uint PayloadKeyMatch; // The key inside of the SteamDRMP.dll file that is matched to this structures data. (This matches the first 4 bytes of the payload data.)
        public uint PayloadDataVirtualAddress; // The virtual address to the payload data.
        public uint PayloadDataSize; // The size of the payload data.
        public uint SteamAppID; // The steam application id of the packed file.
        public uint Unknown0001; // Unknown

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x08)]
        public byte[] SteamAppIDString; // The SteamAppID of the packed file, in string format.

        public uint SteamDRMPDllVirtualAddress; // The offset inside of the payload data holding the virtual address to the SteamDRMP.dll file data.
        public uint SteamDRMPDllSize; // The offset inside of the payload data holding the size of the SteamDRMP.dll file data.
        public uint XTeaKeys; // The offset inside of the payload data holding the address to the Xtea keys to decrypt the SteamDRMP.dll file.

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x31C)]
        public byte[] StubData; // Misc stub data, such as strings, error messages, etc.
    }
}