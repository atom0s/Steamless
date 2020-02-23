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

namespace Steamless.API.PE32
{
    using System;
    using System.Runtime.InteropServices;

    public class NativeApi32
    {
        /// <summary>
        /// IMAGE_DOS_HEADER Structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ImageDosHeader32
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] e_magic;

            public ushort e_cblp;
            public ushort e_cp;
            public ushort e_crlc;
            public ushort e_cparhdr;
            public ushort e_minalloc;
            public ushort e_maxalloc;
            public ushort e_ss;
            public ushort e_sp;
            public ushort e_csum;
            public ushort e_ip;
            public ushort e_cs;
            public ushort e_lfarlc;
            public ushort e_ovno;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public ushort[] e_res1;

            public ushort e_oemid;
            public ushort e_oeminfo;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public ushort[] e_res2;

            public int e_lfanew;

            /// <summary>
            /// Gets if this structure is valid for a PE file.
            /// </summary>
            public bool IsValid => new string(this.e_magic) == "MZ";
        }

        /// <summary>
        /// IMAGE_NT_HEADERS Structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct ImageNtHeaders32
        {
            [FieldOffset(0)]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public char[] Signature;

            [FieldOffset(4)]
            public ImageFileHeader32 FileHeader;

            [FieldOffset(24)]
            public ImageOptionalHeader32 OptionalHeader;

            /// <summary>
            /// Gets if this structure is valid for a PE file.
            /// </summary>
            public bool IsValid => new string(this.Signature).Trim('\0') == "PE";
        }

        /// <summary>
        /// IMAGE_FILE_HEADER Structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ImageFileHeader32
        {
            public ushort Machine;
            public ushort NumberOfSections;
            public uint TimeDateStamp;
            public uint PointerToSymbolTable;
            public uint NumberOfSymbols;
            public ushort SizeOfOptionalHeader;
            public ushort Characteristics;
        }

        /// <summary>
        /// Machine Type Enumeration
        /// </summary>
        public enum MachineType : ushort
        {
            Native = 0,
            I386 = 0x014C,
            Itanium = 0x0200,
            X64 = 0x8664
        }

        /// <summary>
        /// Magic Type Enumeration
        /// </summary>
        public enum MagicType : ushort
        {
            ImageNtOptionalHdr32Magic = 0x10B,
            ImageNtOptionalHdr64Magic = 0x20B
        }

        /// <summary>
        /// Sub System Type Enumeration
        /// </summary>
        public enum SubSystemType : ushort
        {
            ImageSubsystemUnknown = 0,
            ImageSubsystemNative = 1,
            ImageSubsystemWindowsGui = 2,
            ImageSubsystemWindowsCui = 3,
            ImageSubsystemPosixCui = 7,
            ImageSubsystemWindowsCeGui = 9,
            ImageSubsystemEfiApplication = 10,
            ImageSubsystemEfiBootServiceDriver = 11,
            ImageSubsystemEfiRuntimeDriver = 12,
            ImageSubsystemEfiRom = 13,
            ImageSubsystemXbox = 14
        }

        /// <summary>
        /// Dll Characteristics Type Enumeration
        /// </summary>
        public enum DllCharacteristicsType : ushort
        {
            Reserved0 = 0x0001,
            Reserved1 = 0x0002,
            Reserved2 = 0x0004,
            Reserved3 = 0x0008,
            ImageDllCharacteristicsDynamicBase = 0x0040,
            ImageDllCharacteristicsForceIntegrity = 0x0080,
            ImageDllCharacteristicsNxCompat = 0x0100,
            ImageDllcharacteristicsNoIsolation = 0x0200,
            ImageDllcharacteristicsNoSeh = 0x0400,
            ImageDllcharacteristicsNoBind = 0x0800,
            Reserved4 = 0x1000,
            ImageDllcharacteristicsWdmDriver = 0x2000,
            ImageDllcharacteristicsTerminalServerAware = 0x8000
        }

        /// <summary>
        /// IMAGE_OPTIONAL_HEADER Structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct ImageOptionalHeader32
        {
            [FieldOffset(0)]
            public MagicType Magic;

            [FieldOffset(2)]
            public byte MajorLinkerVersion;

            [FieldOffset(3)]
            public byte MinorLinkerVersion;

            [FieldOffset(4)]
            public uint SizeOfCode;

            [FieldOffset(8)]
            public uint SizeOfInitializedData;

            [FieldOffset(12)]
            public uint SizeOfUninitializedData;

            [FieldOffset(16)]
            public uint AddressOfEntryPoint;

            [FieldOffset(20)]
            public uint BaseOfCode;

            // PE32 contains this additional field
            [FieldOffset(24)]
            public uint BaseOfData;

            [FieldOffset(28)]
            public uint ImageBase;

            [FieldOffset(32)]
            public uint SectionAlignment;

            [FieldOffset(36)]
            public uint FileAlignment;

            [FieldOffset(40)]
            public ushort MajorOperatingSystemVersion;

            [FieldOffset(42)]
            public ushort MinorOperatingSystemVersion;

            [FieldOffset(44)]
            public ushort MajorImageVersion;

            [FieldOffset(46)]
            public ushort MinorImageVersion;

            [FieldOffset(48)]
            public ushort MajorSubsystemVersion;

            [FieldOffset(50)]
            public ushort MinorSubsystemVersion;

            [FieldOffset(52)]
            public uint Win32VersionValue;

            [FieldOffset(56)]
            public uint SizeOfImage;

            [FieldOffset(60)]
            public uint SizeOfHeaders;

            [FieldOffset(64)]
            public uint CheckSum;

            [FieldOffset(68)]
            public SubSystemType Subsystem;

            [FieldOffset(70)]
            public DllCharacteristicsType DllCharacteristics;

            [FieldOffset(72)]
            public uint SizeOfStackReserve;

            [FieldOffset(76)]
            public uint SizeOfStackCommit;

            [FieldOffset(80)]
            public uint SizeOfHeapReserve;

            [FieldOffset(84)]
            public uint SizeOfHeapCommit;

            [FieldOffset(88)]
            public uint LoaderFlags;

            [FieldOffset(92)]
            public uint NumberOfRvaAndSizes;

            [FieldOffset(96)]
            public ImageDataDirectory32 ExportTable;

            [FieldOffset(104)]
            public ImageDataDirectory32 ImportTable;

            [FieldOffset(112)]
            public ImageDataDirectory32 ResourceTable;

            [FieldOffset(120)]
            public ImageDataDirectory32 ExceptionTable;

            [FieldOffset(128)]
            public ImageDataDirectory32 CertificateTable;

            [FieldOffset(136)]
            public ImageDataDirectory32 BaseRelocationTable;

            [FieldOffset(144)]
            public ImageDataDirectory32 Debug;

            [FieldOffset(152)]
            public ImageDataDirectory32 Architecture;

            [FieldOffset(160)]
            public ImageDataDirectory32 GlobalPtr;

            [FieldOffset(168)]
            public ImageDataDirectory32 TLSTable;

            [FieldOffset(176)]
            public ImageDataDirectory32 LoadConfigTable;

            [FieldOffset(184)]
            public ImageDataDirectory32 BoundImport;

            [FieldOffset(192)]
            public ImageDataDirectory32 IAT;

            [FieldOffset(200)]
            public ImageDataDirectory32 DelayImportDescriptor;

            [FieldOffset(208)]
            public ImageDataDirectory32 CLRRuntimeHeader;

            [FieldOffset(216)]
            public ImageDataDirectory32 Reserved;
        }

        /// <summary>
        /// IMAGE_DATA_DIRECTORY Structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ImageDataDirectory32
        {
            public uint VirtualAddress;
            public uint Size;
        }

        /// <summary>
        /// IMAGE_SECTION_HEADER Structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct ImageSectionHeader32
        {
            [FieldOffset(0)]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public char[] Name;

            [FieldOffset(8)]
            public uint VirtualSize;

            [FieldOffset(12)]
            public uint VirtualAddress;

            [FieldOffset(16)]
            public uint SizeOfRawData;

            [FieldOffset(20)]
            public uint PointerToRawData;

            [FieldOffset(24)]
            public uint PointerToRelocations;

            [FieldOffset(28)]
            public uint PointerToLinenumbers;

            [FieldOffset(32)]
            public ushort NumberOfRelocations;

            [FieldOffset(34)]
            public ushort NumberOfLinenumbers;

            [FieldOffset(36)]
            public DataSectionFlags Characteristics;

            /// <summary>
            /// Gets the section name of this current section object.
            /// </summary>
            public string SectionName => new string(this.Name).Trim('\0');

            /// <summary>
            /// Gets if this structure is valid for a PE file.
            /// </summary>
            public bool IsValid => this.SizeOfRawData != 0 && this.PointerToRawData != 0;
        }

        /// <summary>
        /// Data Section Flags Enumeration
        /// </summary>
        [Flags]
        public enum DataSectionFlags : uint
        {
            /// <summary>
            /// Reserved for future use.
            /// </summary>
            TypeReg = 0x00000000,

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            TypeDsect = 0x00000001,

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            TypeNoLoad = 0x00000002,

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            TypeGroup = 0x00000004,

            /// <summary>
            /// The section should not be padded to the next boundary. This flag is obsolete and is replaced by IMAGE_SCN_ALIGN_1BYTES. This is valid only for object files.
            /// </summary>
            TypeNoPadded = 0x00000008,

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            TypeCopy = 0x00000010,

            /// <summary>
            /// The section contains executable code.
            /// </summary>
            ContentCode = 0x00000020,

            /// <summary>
            /// The section contains initialized data.
            /// </summary>
            ContentInitializedData = 0x00000040,

            /// <summary>
            /// The section contains uninitialized data.
            /// </summary>
            ContentUninitializedData = 0x00000080,

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            LinkOther = 0x00000100,

            /// <summary>
            /// The section contains comments or other information. The .drectve section has this type. This is valid for object files only.
            /// </summary>
            LinkInfo = 0x00000200,

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            TypeOver = 0x00000400,

            /// <summary>
            /// The section will not become part of the image. This is valid only for object files.
            /// </summary>
            LinkRemove = 0x00000800,

            /// <summary>
            /// The section contains COMDAT data. For more information, see section 5.5.6, COMDAT Sections (Object Only). This is valid only for object files.
            /// </summary>
            LinkComDat = 0x00001000,

            /// <summary>
            /// Reset speculative exceptions handling bits in the TLB entries for this section.
            /// </summary>
            NoDeferSpecExceptions = 0x00004000,

            /// <summary>
            /// The section contains data referenced through the global pointer (GP).
            /// </summary>
            RelativeGp = 0x00008000,

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            MemPurgeable = 0x00020000,

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            Memory16Bit = 0x00020000,

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            MemoryLocked = 0x00040000,

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            MemoryPreload = 0x00080000,

            /// <summary>
            /// Align data on a 1-byte boundary. Valid only for object files.
            /// </summary>
            Align1Bytes = 0x00100000,

            /// <summary>
            /// Align data on a 2-byte boundary. Valid only for object files.
            /// </summary>
            Align2Bytes = 0x00200000,

            /// <summary>
            /// Align data on a 4-byte boundary. Valid only for object files.
            /// </summary>
            Align4Bytes = 0x00300000,

            /// <summary>
            /// Align data on an 8-byte boundary. Valid only for object files.
            /// </summary>
            Align8Bytes = 0x00400000,

            /// <summary>
            /// Align data on a 16-byte boundary. Valid only for object files.
            /// </summary>
            Align16Bytes = 0x00500000,

            /// <summary>
            /// Align data on a 32-byte boundary. Valid only for object files.
            /// </summary>
            Align32Bytes = 0x00600000,

            /// <summary>
            /// Align data on a 64-byte boundary. Valid only for object files.
            /// </summary>
            Align64Bytes = 0x00700000,

            /// <summary>
            /// Align data on a 128-byte boundary. Valid only for object files.
            /// </summary>
            Align128Bytes = 0x00800000,

            /// <summary>
            /// Align data on a 256-byte boundary. Valid only for object files.
            /// </summary>
            Align256Bytes = 0x00900000,

            /// <summary>
            /// Align data on a 512-byte boundary. Valid only for object files.
            /// </summary>
            Align512Bytes = 0x00A00000,

            /// <summary>
            /// Align data on a 1024-byte boundary. Valid only for object files.
            /// </summary>
            Align1024Bytes = 0x00B00000,

            /// <summary>
            /// Align data on a 2048-byte boundary. Valid only for object files.
            /// </summary>
            Align2048Bytes = 0x00C00000,

            /// <summary>
            /// Align data on a 4096-byte boundary. Valid only for object files.
            /// </summary>
            Align4096Bytes = 0x00D00000,

            /// <summary>
            /// Align data on an 8192-byte boundary. Valid only for object files.
            /// </summary>
            Align8192Bytes = 0x00E00000,

            /// <summary>
            /// The section contains extended relocations.
            /// </summary>
            LinkExtendedRelocationOverflow = 0x01000000,

            /// <summary>
            /// The section can be discarded as needed.
            /// </summary>
            MemoryDiscardable = 0x02000000,

            /// <summary>
            /// The section cannot be cached.
            /// </summary>
            MemoryNotCached = 0x04000000,

            /// <summary>
            /// The section is not pageable.
            /// </summary>
            MemoryNotPaged = 0x08000000,

            /// <summary>
            /// The section can be shared in memory.
            /// </summary>
            MemoryShared = 0x10000000,

            /// <summary>
            /// The section can be executed as code.
            /// </summary>
            MemoryExecute = 0x20000000,

            /// <summary>
            /// The section can be read.
            /// </summary>
            MemoryRead = 0x40000000,

            /// <summary>
            /// The section can be written to.
            /// </summary>
            MemoryWrite = 0x80000000
        }

        /// <summary>
        /// IMAGE_TLS_DIRECTORY Structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ImageTlsDirectory32
        {
            public uint StartAddressOfRawData;
            public uint EndAddressOfRawData;
            public uint AddressOfIndex;
            public uint AddressOfCallBacks;
            public uint SizeOfZeroFill;
            public uint Characteristics;
        }
    }
}