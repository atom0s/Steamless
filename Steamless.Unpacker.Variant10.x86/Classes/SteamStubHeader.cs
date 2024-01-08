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

namespace Steamless.Unpacker.Variant10.x86.Classes
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// SteamStub DRM Variant 1.0 Header
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SteamStub32Var10Header
    {
        public uint GetModuleHandleA_idata;     // The address of GetModuleHandleA inside of the .idata section.
        public uint GetProcAddress_idata;       // The address of GetProcAddress inside of the .idata section.
        public uint BindFunction;               // The .bind unpacker function address.
        public uint BindCodeSize;               // The .bind unpacker function size.
        public uint Checksum;                   // The checksum of the header data after its initialized. (This is done via addition chunking.)
        public uint AppId;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x08)]
        public byte[] SteamAppIDString;         // The SteamAppID of the packed file, in string format.

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] str_kernel32dll;          // String: kernel32.dll

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] str_user32dll;            // String: user32.dll

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] str_shell32dll;           // String: shell32.dll

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] str_loadlibraryexa;       // String: LoadLibraryExA

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] str_freelibrary;          // String: FreeLibrary

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] str_messageboxa;          // String: MessageBoxA

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x18)]
        public byte[] str_getmodulefilenamea;   // String: GetModuleFileNameA

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] str_lstrlena;             // String: lstrlenA

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] str_lstrcata;             // String lstrcatA

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] str_exitprocess;          // String: ExitProcess

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] str_shellexecutea;        // String: ShellExecuteA

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] str_steamerror;           // String: Steam Error

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] str_steamdll;             // String: Steam.dll

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x18)]
        public byte[] str_steamisappsubscribed; // String: SteamIsAppSubscribed

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] str_steamstartup;         // String: SteamStartup

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] str_steamcleanup;         // String: SteamCleanup

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
        public byte[] str_failedtofindsteam;    // String: Failed to find Steam

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
        public byte[] str_failedtoloadsteam;    // String: Failed to load Steam

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x18)]
        public byte[] str_steamstoreurl;        // String: steam://store/

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x18)]
        public byte[] str_steamrunurl;          // String: steam://run/

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x08)]
        public byte[] str_open;                 // String: open
    }
}