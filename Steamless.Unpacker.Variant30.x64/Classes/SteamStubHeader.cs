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

namespace Steamless.Unpacker.Variant30.x64.Classes
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// SteamStub DRM Variant 3.0 x64 Header
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SteamStub64Var30Header
    {
        public uint XorKey; // The base XOR key, if defined, to unpack the file with.
        public uint Signature; // 0xC0DEC0DE signature to validate this header is proper.
        public ulong ImageBase; // The base of the image that is protected.
        public uint AddressOfEntryPoint; // The entry point that is set from the DRM.
        public uint BindSectionOffset; // The starting offset to the bind section data. RVA(AddressOfEntryPoint - BindSectionOffset)
        public uint Unknown0000; // [Cyanic: This field is most likely the .bind code size.]
        public uint OriginalEntryPoint; // The original entry point of the binary before it was protected.
        public uint Unknown0001; // [Cyanic: This field is most likely an offset to a string table.]
        public uint PayloadSize; // The size of the payload data.
        public uint DRMPDllOffset; // The offset to the SteamDRMP.dll file.
        public uint DRMPDllSize; // The size of the SteamDRMP.dll file.
        public uint SteamAppId; // The Steam Application ID of this game.
        public uint Flags; // The DRM flags used while creating the protected executable.
        public uint BindSectionVirtualSize; // The bind section virtual size.
        public uint Unknown0002; // [Cyanic: This field is most likely a hash of some sort.]
        public uint CodeSectionVirtualAddress; // The cpde section virtual address.
        public uint CodeSectionRawSize; // The raw size of the code section.

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
        public byte[] AES_Key; // The AES encryption key.

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] AES_IV; // The AES encryption IV.

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] CodeSectionStolenData; // The first 16 bytes of the code section stolen.

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x04)]
        public uint[] EncryptionKeys; // Encryption keys used for decrypting SteamDRMP.dll file.

        public uint HasTlsCallback; // Flag that states if the file was protected with a TlsCallback present.
        public uint Unknown0004;
        public uint Unknown0005;
        public uint Unknown0006;
        public uint Unknown0007;
        public uint Unknown0008;
        public uint GetModuleHandleA_RVA; // The RVA to GetModuleHandleA.
        public uint GetModuleHandleW_RVA; // The RVA to GetModuleHandleW.
        public uint LoadLibraryA_RVA; // The RVA to LoadLibraryA.
        public uint LoadLibraryW_RVA; // The RVA to LoadLibraryW.
        public uint GetProcAddress_RVA; // The RVA to GetProcAddress.
        public uint Unknown0009;
        public uint Unknown0010;
        public uint Unknown0011;
    }
}