/**
 * Steamless - Copyright (c) 2015 - 2020 atom0s [atom0s@live.com]
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
        public uint XorKey1; // Xor key used to encode the header data.
        public uint XorKey2; // Xor key used to encode the header data.
        public uint GetModuleHandleA_idata; // The address of GetModuleHandleA inside of the .idata section.
        public uint GetProcAddress_idata; // The address of GetProcAddress inside of the .idata section.
        public uint GetModuleHandleW_idata; // The address of GetModuleHandleW inside of the .idata section.
        public uint GetProcAddress_bind; // The address of the .bind sections custom GetProcAddress instance.
        public uint Flags; // Protection flags used with the file.
        public uint Unknown0000; // Unknown (Was 0xEC227021 when testing.) (Only used if (Flags & 0x10) is set. Used in part of a hash check.)
        public uint BindSectionVirtualAddress; // The virtual address to the .bind section.
        public uint BindSectionCodeSize; // The size of the code stub inside of the .bind section.
        public uint ValidationHash; // Hash that is calculated based on the .bind code section and .bind stub header data. (Only used if (Flags & 1) is set.)
        public uint OEP; // The original file OEP to be invoked after the stub has finished.
        public uint CodeSectionVirtualAddress; // The virtual address to the code section. (.text) (Was 0x0401000 when testing. Possibly original OEP?)
        public uint CodeSectionSize; // The size of the code section.
        public uint CodeSectionXorKey; // The starting key to xor decode against. (Only used if (Flags & 4) is set.)
        public uint SteamAppID; // The steam application id of the packed file.

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x0C)]
        public byte[] SteamAppIDString; // The SteamAppID of the packed file, in string format.

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x36C)]
        public byte[] StubData; // Misc stub data, such as strings, error messages, etc.
    }
}