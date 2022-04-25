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

namespace Steamless.Unpacker.Variant21.x86.Classes
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// SteamStub DRM Variant 2.1 Header
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SteamStub32Var21Header
    {
        public uint XorKey; // The base XOR key, if defined, to unpack the file with.
        public uint GetModuleHandleA; // The address of GetModuleHandleA. (If set.)
        public uint GetModuleHandleW; // The address of GetModuleHandleW. (If set.)
        public uint GetProcAddress; // The address of GetProcAddress. (If set.)
        public uint LoadLibraryA; // The address of LoadLibraryA. (If set.)
        public uint LoadLibraryW; // The address of LoadLibraryW. (If set.)
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

        //
        // The 'StubData' field is dynamically sized based on the needs of the stub version and used options. This is effectively
        // impossible to 'size' correctly for all header versions, so instead it will be treated separately.
        //
        // [MarshalAs(UnmanagedType.ByValArray, SizeConst = ???)]
        // public byte[] StubData; // Misc stub data, such as strings, error messages, etc.
        //
    }

    /// <summary>
    /// SteamStub DRM Variant 2.1 Header (Header Size: 0xD0 Variant)
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SteamStub32Var21Header_D0Variant
    {
        public uint XorKey; // The base XOR key, if defined, to unpack the file with.
        public uint GetModuleHandleA; // The address of GetModuleHandleA. (If set.)
        public uint GetModuleHandleW; // The address of GetModuleHandleW. (If set.)
        public uint GetProcAddress; // The address of GetProcAddress. (If set.)
        public uint LoadLibraryA; // The address of LoadLibraryA. (If set.)
        public uint BindSectionVirtualAddress; // The virtual address to the .bind section.
        public uint BindStartFunctionSize; // The size of the start function from the .bind section.
        public uint PayloadKeyMatch; // The key inside of the SteamDRMP.dll file that is matched to this structures data. (This matches the first 4 bytes of the payload data.)
        public uint PayloadDataVirtualAddress; // The virtual address to the payload data.
        public uint PayloadDataSize; // The size of the payload data.
        public uint SteamAppID; // The steam application id of the packed file.
        public uint Unknown0000; // Unknown

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x08)]
        public byte[] SteamAppIDString; // The SteamAppID of the packed file, in string format.

        public uint SteamDRMPDllVirtualAddress; // The offset inside of the payload data holding the virtual address to the SteamDRMP.dll file data.
        public uint SteamDRMPDllSize; // The offset inside of the payload data holding the size of the SteamDRMP.dll file data.
        public uint XTeaKeys; // The offset inside of the payload data holding the address to the Xtea keys to decrypt the SteamDRMP.dll file.

        //
        // The 'StubData' field is dynamically sized based on the needs of the stub version and used options. This is effectively
        // impossible to 'size' correctly for all header versions, so instead it will be treated separately.
        //
        // [MarshalAs(UnmanagedType.ByValArray, SizeConst = ???)]
        // public byte[] StubData; // Misc stub data, such as strings, error messages, etc.
        //
    }
}