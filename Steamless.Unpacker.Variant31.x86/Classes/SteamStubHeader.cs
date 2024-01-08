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

namespace Steamless.Unpacker.Variant31.x86.Classes
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// SteamStub DRM Variant 3.1 Header
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SteamStub32Var31Header
    {
        public uint XorKey; // The base xor key, if defined, to unpack the file with.
        public uint Signature; // The signature to ensure the xor decoding was successful.
        public ulong ImageBase; // The base of the image that was protected.
        public ulong AddressOfEntryPoint; // The entry point that is set from the DRM.
        public uint BindSectionOffset; // The starting offset to the .bind section data. RVA(AddressOfEntryPoint - BindSectionOffset)
        public uint Unknown0000; // [Cyanic: This field is most likely the .bind code size.]
        public ulong OriginalEntryPoint; // The original entry point of the binary before it was protected.
        public uint Unknown0001; // [Cyanic: This field is most likely an offset to a string table.]
        public uint PayloadSize; // The size of the payload data.
        public uint DRMPDllOffset; // The offset to the SteamDrmp.dll file.
        public uint DRMPDllSize; // The size of the SteamDrmp.dll file.
        public uint SteamAppId; // The Steam application id of this program.
        public uint Flags; // The DRM flags used while protecting this program.
        public uint BindSectionVirtualSize; // The .bind section virtual size.
        public uint Unknown0002; // [Cyanic: This field is most likely a hash of some sort.]
        public ulong CodeSectionVirtualAddress; // The code section virtual address.
        public ulong CodeSectionRawSize; // The code section raw size.

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
        public byte[] AES_Key; // The AES encryption key.

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] AES_IV; // The AES encryption IV.

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] CodeSectionStolenData; // The first 16 bytes of the code section stolen.

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x04)]
        public uint[] EncryptionKeys; // Encryption keys used to decrypt the SteamDrmp.dll file.

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x08)]
        public uint[] Unknown0003; // Unknown unused data.

        public ulong GetModuleHandleA_Rva; // The rva to GetModuleHandleA.
        public ulong GetModuleHandleW_Rva; // The rva to GetModuleHandleW.
        public ulong LoadLibraryA_Rva; // The rva to LoadLibraryA.
        public ulong LoadLibraryW_Rva; // The rva to LoadLibraryW.
        public ulong GetProcAddress_Rva; // The rva to GetProcAddress.
    }
}