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

namespace Steamless.API.PE32
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Portable Executable (32bit) Class
    /// </summary>
    public class Pe32File
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public Pe32File()
        {
        }

        /// <summary>
        /// Overloaded Constructor
        /// </summary>
        /// <param name="file"></param>
        public Pe32File(string file)
        {
            this.FilePath = file;
        }

        /// <summary>
        /// Parses a Win32 PE file.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool Parse(string file = null)
        {
            // Prepare the class variables..
            if (file != null)
                this.FilePath = file;

            this.FileData = null;
            this.DosHeader = new NativeApi32.ImageDosHeader32();
            this.NtHeaders = new NativeApi32.ImageNtHeaders32();
            this.DosStubSize = 0;
            this.DosStubOffset = 0;
            this.DosStubData = null;
            this.Sections = new List<NativeApi32.ImageSectionHeader32>();
            this.SectionData = new List<byte[]>();
            this.TlsDirectory = new NativeApi32.ImageTlsDirectory32();
            this.TlsCallbacks = new List<uint>();

            // Ensure a file path has been set..
            if (string.IsNullOrEmpty(this.FilePath) || !File.Exists(this.FilePath))
                return false;

            // Read the file data..
            this.FileData = File.ReadAllBytes(this.FilePath);

            // Ensure we have valid data by the overall length..
            if (this.FileData.Length < (Marshal.SizeOf(typeof(NativeApi32.ImageDosHeader32)) + Marshal.SizeOf(typeof(NativeApi32.ImageNtHeaders32))))
                return false;

            // Read the file DOS header..
            this.DosHeader = Pe32Helpers.GetStructure<NativeApi32.ImageDosHeader32>(this.FileData);
            if (!this.DosHeader.IsValid)
                return false;

            // Read the file NT headers..
            this.NtHeaders = Pe32Helpers.GetStructure<NativeApi32.ImageNtHeaders32>(this.FileData, this.DosHeader.e_lfanew);
            if (!this.NtHeaders.IsValid)
                return false;

            // Read and store the dos header if it exists..
            this.DosStubSize = (uint)(this.DosHeader.e_lfanew - Marshal.SizeOf(typeof(NativeApi32.ImageDosHeader32)));
            if (this.DosStubSize > 0)
            {
                this.DosStubOffset = (uint)Marshal.SizeOf(typeof(NativeApi32.ImageDosHeader32));
                this.DosStubData = new byte[this.DosStubSize];
                Array.Copy(this.FileData, this.DosStubOffset, this.DosStubData, 0, this.DosStubSize);
            }

            // Read the file sections..
            for (var x = 0; x < this.NtHeaders.FileHeader.NumberOfSections; x++)
            {
                var section = Pe32Helpers.GetSection(this.FileData, x, this.DosHeader, this.NtHeaders);
                this.Sections.Add(section);

                // Get the sections data..
                var sectionData = new byte[this.GetAlignment(section.SizeOfRawData, this.NtHeaders.OptionalHeader.FileAlignment)];
                Array.Copy(this.FileData, section.PointerToRawData, sectionData, 0, section.SizeOfRawData);
                this.SectionData.Add(sectionData);
            }

            try
            {
                // Obtain the file overlay if one exists..
                var lastSection = this.Sections.Last();
                var fileSize = lastSection.SizeOfRawData + lastSection.PointerToRawData;
                if (fileSize < this.FileData.Length)
                {
                    this.OverlayData = new byte[this.FileData.Length - fileSize];
                    Array.Copy(this.FileData, fileSize, this.OverlayData, 0, this.FileData.Length - fileSize);
                }
            }
            catch
            {
                return false;
            }

            // Read the files Tls information if available..
            if (this.NtHeaders.OptionalHeader.TLSTable.VirtualAddress != 0)
            {
                // Get the file offset to the Tls data..
                var tls = this.NtHeaders.OptionalHeader.TLSTable;
                var addr = this.GetFileOffsetFromRva(tls.VirtualAddress);

                // Read the Tls directory..
                this.TlsDirectory = Pe32Helpers.GetStructure<NativeApi32.ImageTlsDirectory32>(this.FileData, (int)addr);

                // Read the Tls callbacks..
                addr = this.GetRvaFromVa(this.TlsDirectory.AddressOfCallBacks);
                addr = this.GetFileOffsetFromRva(addr);

                // Loop until we hit a null pointer..
                var count = 0;
                while (true)
                {
                    var callback = BitConverter.ToUInt32(this.FileData, (int)addr + (count * 4));
                    if (callback == 0)
                        break;

                    this.TlsCallbacks.Add(callback);
                    count++;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines if the current file is 64bit.
        /// </summary>
        /// <returns></returns>
        public bool IsFile64Bit()
        {
            return (this.NtHeaders.FileHeader.Machine & (uint)NativeApi32.MachineType.X64) == (uint)NativeApi32.MachineType.X64;
        }

        /// <summary>
        /// Determines if the file has a section containing the given name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasSection(string name)
        {
            return this.Sections.Any(s => string.Compare(s.SectionName, name, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        /// <summary>
        /// Obtains a section by its name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public NativeApi32.ImageSectionHeader32 GetSection(string name)
        {
            return this.Sections.FirstOrDefault(s => string.Compare(s.SectionName, name, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        /// <summary>
        /// Obtains the owner section of the given rva.
        /// </summary>
        /// <param name="rva"></param>
        /// <returns></returns>
        public NativeApi32.ImageSectionHeader32 GetOwnerSection(uint rva)
        {
            foreach (var s in this.Sections)
            {
                var size = s.VirtualSize;
                if (size == 0)
                    size = s.SizeOfRawData;

                if ((rva >= s.VirtualAddress) && (rva < s.VirtualAddress + size))
                    return s;
            }

            return default(NativeApi32.ImageSectionHeader32);
        }

        /// <summary>
        /// Obtains the owner section of the given rva.
        /// </summary>
        /// <param name="rva"></param>
        /// <returns></returns>
        public NativeApi32.ImageSectionHeader32 GetOwnerSection(ulong rva)
        {
            foreach (var s in this.Sections)
            {
                var size = s.VirtualSize;
                if (size == 0)
                    size = s.SizeOfRawData;

                if ((rva >= s.VirtualAddress) && (rva < s.VirtualAddress + size))
                    return s;
            }

            return default(NativeApi32.ImageSectionHeader32);
        }

        /// <summary>
        /// Obtains a sections data by its index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public byte[] GetSectionData(int index)
        {
            if (index < 0 || index > this.Sections.Count)
                return null;

            return this.SectionData[index];
        }

        /// <summary>
        /// Obtains a sections data by its name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public byte[] GetSectionData(string name)
        {
            for (var x = 0; x < this.Sections.Count; x++)
            {
                if (string.Compare(this.Sections[x].SectionName, name, StringComparison.InvariantCultureIgnoreCase) == 0)
                    return this.SectionData[x];
            }

            return null;
        }

        /// <summary>
        /// Gets a sections index by its name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetSectionIndex(string name)
        {
            for (var x = 0; x < this.Sections.Count; x++)
            {
                if (string.Compare(this.Sections[x].SectionName, name, StringComparison.InvariantCultureIgnoreCase) == 0)
                    return x;
            }

            return -1;
        }

        /// <summary>
        /// Gets a sections index by its name.
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public int GetSectionIndex(NativeApi32.ImageSectionHeader32 section)
        {
            return this.Sections.IndexOf(section);
        }

        /// <summary>
        /// Removes a section from the files section list.
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public bool RemoveSection(NativeApi32.ImageSectionHeader32 section)
        {
            var index = this.Sections.IndexOf(section);
            if (index == -1)
                return false;

            this.Sections.RemoveAt(index);
            this.SectionData.RemoveAt(index);

            return true;
        }

        /// <summary>
        /// Rebuilds the sections by aligning them as needed. Updates the Nt headers to
        /// correct the new SizeOfImage after alignment is completed.
        /// 
        /// <param name="realign"></param>
        /// </summary>
        public void RebuildSections(bool realign = true)
        {
            for (var x = 0; x < this.Sections.Count; x++)
            {
                // Obtain the current section and realign the data..
                var section = this.Sections[x];

                if (realign)
                {
                    section.VirtualAddress = this.GetAlignment(section.VirtualAddress, this.NtHeaders.OptionalHeader.SectionAlignment);
                    section.VirtualSize = this.GetAlignment(section.VirtualSize, this.NtHeaders.OptionalHeader.SectionAlignment);
                    section.PointerToRawData = this.GetAlignment(section.PointerToRawData, this.NtHeaders.OptionalHeader.FileAlignment);
                    section.SizeOfRawData = this.GetAlignment(section.SizeOfRawData, this.NtHeaders.OptionalHeader.FileAlignment);
                }

                // Store the sections updates..
                this.Sections[x] = section;
            }

            // Update the size of the image..
            var ntHeaders = this.NtHeaders;
            ntHeaders.OptionalHeader.SizeOfImage = (uint)this.GetAlignment(this.Sections.Last().VirtualAddress + this.Sections.Last().VirtualSize, this.NtHeaders.OptionalHeader.SectionAlignment);
            this.NtHeaders = ntHeaders;
        }

        /// <summary>
        /// Obtains the relative virtual address from the given virtual address.
        /// </summary>
        /// <param name="va"></param>
        /// <returns></returns>
        public uint GetRvaFromVa(uint va)
        {
            return va - this.NtHeaders.OptionalHeader.ImageBase;
        }

        /// <summary>
        /// Obtains the file offset from the given relative virtual address.
        /// </summary>
        /// <param name="rva"></param>
        /// <returns></returns>
        public uint GetFileOffsetFromRva(uint rva)
        {
            var section = this.GetOwnerSection(rva);
            return (rva - (section.VirtualAddress - section.PointerToRawData));
        }

        /// <summary>
        /// Aligns the value based on the given alignment.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="align"></param>
        /// <returns></returns>
        public uint GetAlignment(uint val, uint align)
        {
            return (((val + align - 1) / align) * align);
        }

        /// <summary>
        /// Gets or sets the path to the file being processed.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the raw file data read from disk.
        /// </summary>
        public byte[] FileData { get; set; }

        /// <summary>
        /// Gets or sets the dos header of the file.
        /// </summary>
        public NativeApi32.ImageDosHeader32 DosHeader { get; set; }

        /// <summary>
        /// Gets or sets the NT headers of the file.
        /// </summary>
        public NativeApi32.ImageNtHeaders32 NtHeaders { get; set; }

        /// <summary>
        /// Gets or sets the optional dos stub size.
        /// </summary>
        public uint DosStubSize { get; set; }

        /// <summary>
        /// Gets or sets the optional dos stub offset.
        /// </summary>
        public uint DosStubOffset { get; set; }

        /// <summary>
        /// Gets or sets the optional dos stub data.
        /// </summary>
        public byte[] DosStubData { get; set; }

        /// <summary>
        /// Gets or sets the sections of the file.
        /// </summary>
        public List<NativeApi32.ImageSectionHeader32> Sections;

        /// <summary>
        /// Gets or sets the section data of the file.
        /// </summary>
        public List<byte[]> SectionData;

        /// <summary>
        /// Gets or sets the overlay data of the file.
        /// </summary>
        public byte[] OverlayData;

        /// <summary>
        /// Gets or sets the Tls directory of the file.
        /// </summary>
        public NativeApi32.ImageTlsDirectory32 TlsDirectory { get; set; }

        /// <summary>
        /// Gets or sets a list of Tls callbacks of the file.
        /// </summary>
        public List<uint> TlsCallbacks { get; set; }
    }
}