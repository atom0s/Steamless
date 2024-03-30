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

namespace Steamless.Unpacker.Variant30.x64
{
    using API;
    using API.Crypto;
    using API.Events;
    using API.Extensions;
    using API.Model;
    using API.PE64;
    using API.Services;
    using Classes;
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;

    [SteamlessApiVersion(1, 0)]
    public class Main : SteamlessPlugin
    {
        /// <summary>
        /// Internal logging service instance.
        /// </summary>
        private LoggingService m_LoggingService;

        /// <summary>
        /// Gets the author of this plugin.
        /// </summary>
        public override string Author => "atom0s";

        /// <summary>
        /// Gets the name of this plugin.
        /// </summary>
        public override string Name => "SteamStub Variant 3.0 Unpacker (x64)";

        /// <summary>
        /// Gets the description of this plugin.
        /// </summary>
        public override string Description => "Unpacker for the 64bit SteamStub variant 3.0.";

        /// <summary>
        /// Gets the version of this plugin.
        /// </summary>
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        /// <summary>
        /// Internal wrapper to log a message.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="type"></param>
        private void Log(string msg, LogMessageType type)
        {
            this.m_LoggingService.OnAddLogMessage(this, new LogMessageEventArgs(msg, type));
        }

        /// <summary>
        /// Initialize function called when this plugin is first loaded.
        /// </summary>
        /// <param name="logService"></param>
        /// <returns></returns>
        public override bool Initialize(LoggingService logService)
        {
            this.m_LoggingService = logService;
            return true;
        }

        /// <summary>
        /// Gets the SteamStub header size from the given file.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        private uint GetHeaderSize(Pe64File f)
        {
            // Obtain the bind section data..
            var bind = f.GetSectionData(".bind");

            // Attempt to locate the known v3.x signature..
            var variant = Pe64Helpers.FindPattern(bind, "E8 00 00 00 00 50 53 51 52 56 57 55 41 50");
            if (variant == -1)
                return 0;

            // Attempt to determine the variant version..
            var offset = Pe64Helpers.FindPattern(bind, "48 8D 91 ?? ?? ?? ?? 48"); // 3.0
            if (offset == -1)
                offset = Pe64Helpers.FindPattern(bind, "48 8D 91 ?? ?? ?? ?? 41"); // 3.1

            // Ensure a pattern was found..
            if (offset == -1)
                return 0;

            // Read the header size.. (The header size is only 32bit!)
            return (uint)Math.Abs(BitConverter.ToInt32(bind, (int)offset + 3));
        }

        /// <summary>
        /// Processing function called when a file is being unpacked. Allows plugins to check the file
        /// and see if it can handle the file for its intended purpose.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public override bool CanProcessFile(string file)
        {
            try
            {
                // Load the file..
                var f = new Pe64File(file);
                if (!f.Parse() || !f.IsFile64Bit() || !f.HasSection(".bind"))
                    return false;

                // Check for the known 3.0 header sizes..
                var headerSize = this.GetHeaderSize(f);
                return headerSize == 0xB0 || headerSize == 0xD0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Processing function called to allow the plugin to process the file.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override bool ProcessFile(string file, SteamlessOptions options)
        {
            // Initialize the class members..
            this.TlsAsOep = false;
            this.TlsOepRva = 0;
            this.Options = options;
            this.CodeSectionData = null;
            this.CodeSectionIndex = -1;
            this.XorKey = 0;

            // Parse the file..
            this.File = new Pe64File(file);
            if (!this.File.Parse())
                return false;

            // Announce we are being unpacked with this packer..
            this.Log("File is packed with SteamStub Variant 3.0!", LogMessageType.Information);

            this.Log("Step 1 - Read, decode and validate the SteamStub DRM header.", LogMessageType.Information);
            if (!this.Step1())
                return false;

            this.Log("Step 2 - Read, decode and process the payload data.", LogMessageType.Information);
            if (!this.Step2())
                return false;

            this.Log("Step 3 - Read, decode and dump the SteamDRMP.dll file.", LogMessageType.Information);
            if (!this.Step3())
                return false;

            this.Log("Step 4 - Handle .bind section. Find code section.", LogMessageType.Information);
            if (!this.Step4())
                return false;

            this.Log("Step 5 - Read, decrypt and process code section.", LogMessageType.Information);
            if (!this.Step5())
                return false;

            this.Log("Step 6 - Rebuild and save the unpacked file.", LogMessageType.Information);
            if (!this.Step6())
                return false;

            if (this.Options.RecalculateFileChecksum)
            {
                this.Log("Step 7 - Rebuild unpacked file checksum.", LogMessageType.Information);
                if (!this.Step7())
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Rebuilds the file TlsCallback information and repairs the proper OEP.
        /// </summary>
        /// <returns></returns>
        private bool RebuildTlsCallbackInformation()
        {
            // Ensure the modified main TlsCallback is within the .bind section..
            var section = this.File.GetOwnerSection(this.File.GetRvaFromVa(this.File.TlsCallbacks[0]));
            if (!section.IsValid || string.Compare(section.SectionName, ".bind", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.CompareOptions.IgnoreCase) != 0)
                return false;

            // Obtain the section that holds the Tls directory information..
            var addr = this.File.GetFileOffsetFromRva(this.File.GetRvaFromVa(this.File.TlsDirectory.AddressOfCallBacks));
            var tlsd = this.File.GetOwnerSection(addr);

            if (!tlsd.IsValid)
                return false;

            addr -= tlsd.PointerToRawData;

            // Restore the true original TlsCallback address..
            var callback = BitConverter.GetBytes(this.File.NtHeaders.OptionalHeader.ImageBase + this.StubHeader.OriginalEntryPoint);
            Array.Copy(callback, 0, this.File.GetSectionData(this.File.GetSectionIndex(tlsd)), (int)addr, callback.Length);

            // Find the original entry point function..
            var entry = this.File.GetFileOffsetFromRva(this.File.NtHeaders.OptionalHeader.AddressOfEntryPoint);
            var data = this.File.FileData.Skip((int)entry).Take(0x100).ToArray();

            // Find the XOR key from within the function..
            var res = Pe64Helpers.FindPattern(data, "48 81 EA ?? ?? ?? ?? 8B 12 81 F2");
            if (res == -1)
                return false;

            // Decrypt and recalculate the true OEP address..
            var key = (ulong)(this.StubHeader.XorKey ^ BitConverter.ToInt32(data, (int)res + 0x0B));
            var off = (ulong)((this.File.NtHeaders.OptionalHeader.ImageBase + this.File.NtHeaders.OptionalHeader.AddressOfEntryPoint) + key);

            // Store the proper OEP..
            this.TlsOepOverride = (uint)(off - this.File.NtHeaders.OptionalHeader.ImageBase);
            return true;
        }

        /// <summary>
        /// Step #1
        /// 
        /// Read, decode and validate the SteamStub DRM header.
        /// </summary>
        /// <returns></returns>
        private bool Step1()
        {
            // Obtain the header size..
            var headerSize = this.GetHeaderSize(this.File);

            // Obtain the DRM header data..
            var fileOffset = this.File.GetFileOffsetFromRva(this.File.NtHeaders.OptionalHeader.AddressOfEntryPoint);
            var headerData = new byte[headerSize];
            Array.Copy(this.File.FileData, (long)(fileOffset - headerSize), headerData, 0, headerSize);

            // Xor decode the header data..
            this.XorKey = SteamStubHelpers.SteamXor(ref headerData, headerSize);
            this.StubHeader = Pe64Helpers.GetStructure<SteamStub64Var30Header>(headerData);

            // Validate the structure signature..
            if (this.StubHeader.Signature == 0xC0DEC0DE)
                return true;

            // Try again using the Tls callback (if any) as the OEP instead..
            if (this.File.TlsCallbacks.Count == 0)
                return false;

            // Obtain the DRM header data..
            fileOffset = this.File.GetRvaFromVa(this.File.TlsCallbacks[0]);
            fileOffset = this.File.GetFileOffsetFromRva(fileOffset);
            headerData = new byte[headerSize];
            Array.Copy(this.File.FileData, (long)(fileOffset - headerSize), headerData, 0, headerSize);

            // Xor decode the header data..
            this.XorKey = SteamStubHelpers.SteamXor(ref headerData, headerSize);
            this.StubHeader = Pe64Helpers.GetStructure<SteamStub64Var30Header>(headerData);

            // Validate the structure signature..
            if (this.StubHeader.Signature != 0xC0DEC0DE)
                return false;

            // Tls was valid for the real oep..
            this.TlsAsOep = true;
            this.TlsOepRva = this.File.GetRvaFromVa(this.File.TlsCallbacks[0]);

            // Is the TlsCallback replacing the OEP..
            if (this.StubHeader.HasTlsCallback != 1 || this.File.TlsCallbacks[0] == 0)
                return true;

            // Rebuild the file Tls callback information..
            return this.RebuildTlsCallbackInformation();
        }

        /// <summary>
        /// Step #2
        /// 
        /// Read, decode and process the payload data.
        /// </summary>
        /// <returns></returns>
        private bool Step2()
        {
            // Obtain the payload address and size..
            var payloadAddr = this.File.GetFileOffsetFromRva(this.TlsAsOep ? this.TlsOepRva - this.StubHeader.BindSectionOffset : this.File.NtHeaders.OptionalHeader.AddressOfEntryPoint - this.StubHeader.BindSectionOffset);
            var payloadSize = (this.StubHeader.PayloadSize + 0x0F) & 0xFFFFFFF0;

            // Do nothing if there is no payload..
            if (payloadSize == 0)
                return true;

            this.Log(" --> File has payload data!", LogMessageType.Debug);

            // Obtain and decode the payload..
            var payload = new byte[payloadSize];
            Array.Copy(this.File.FileData, (long)payloadAddr, payload, 0, payloadSize);
            this.XorKey = SteamStubHelpers.SteamXor(ref payload, payloadSize, this.XorKey);

            try
            {
                if (this.Options.DumpPayloadToDisk)
                {
                    System.IO.File.WriteAllBytes(this.File.FilePath + ".payload", payload);
                    this.Log(" --> Saved payload to disk!", LogMessageType.Debug);
                }
            }
            catch
            {
                // Do nothing here since it doesn't matter if this fails..
            }

            return true;
        }

        /// <summary>
        /// Step #3
        /// 
        /// Read, decode and dump the SteamDRMP.dll file.
        /// </summary>
        /// <returns></returns>
        private bool Step3()
        {
            // Ensure there is a dll to process..
            if (this.StubHeader.DRMPDllSize == 0)
            {
                this.Log(" --> File does not contain a SteamDRMP.dll file.", LogMessageType.Debug);
                return true;
            }

            this.Log(" --> File has SteamDRMP.dll file!", LogMessageType.Debug);

            try
            {
                // Obtain the SteamDRMP.dll file address and data..
                var drmpAddr = this.File.GetFileOffsetFromRva(this.TlsAsOep ? this.TlsOepRva - this.StubHeader.BindSectionOffset + this.StubHeader.DRMPDllOffset : this.File.NtHeaders.OptionalHeader.AddressOfEntryPoint - this.StubHeader.BindSectionOffset + this.StubHeader.DRMPDllOffset);
                var drmpData = new byte[this.StubHeader.DRMPDllSize];
                Array.Copy(this.File.FileData, (long)drmpAddr, drmpData, 0, drmpData.Length);

                // Decrypt the data (xtea decryption)..
                SteamStubHelpers.SteamDrmpDecryptPass1(ref drmpData, this.StubHeader.DRMPDllSize, this.StubHeader.EncryptionKeys);

                try
                {
                    if (this.Options.DumpSteamDrmpToDisk)
                    {
                        var basePath = Path.GetDirectoryName(this.File.FilePath) ?? string.Empty;
                        System.IO.File.WriteAllBytes(Path.Combine(basePath, "SteamDRMP.dll"), drmpData);
                        this.Log(" --> Saved SteamDRMP.dll to disk!", LogMessageType.Debug);
                    }
                }
                catch
                {
                    // Do nothing here since it doesn't matter if this fails..
                }

                return true;
            }
            catch
            {
                this.Log(" --> Error trying to decrypt the files SteamDRMP.dll data!", LogMessageType.Error);
                return false;
            }
        }

        /// <summary>
        /// Step #4
        /// 
        /// Remove the bind section if requested.
        /// Find the code section.
        /// </summary>
        /// <returns></returns>
        private bool Step4()
        {
            // Remove the bind section if its not requested to be saved..
            if (!this.Options.KeepBindSection)
            {
                // Obtain the .bind section..
                var bindSection = this.File.GetSection(".bind");
                if (!bindSection.IsValid)
                    return false;

                // Remove the section..
                this.File.RemoveSection(bindSection);

                // Decrease the header section count..
                var ntHeaders = this.File.NtHeaders;
                ntHeaders.FileHeader.NumberOfSections--;
                this.File.NtHeaders = ntHeaders;

                this.Log(" --> .bind section was removed from the file.", LogMessageType.Debug);
            }
            else
                this.Log(" --> .bind section was kept in the file.", LogMessageType.Debug);

            // Skip finding the code section if the file is not encrypted..
            if ((this.StubHeader.Flags & (uint)SteamStubDrmFlags.NoEncryption) == (uint)SteamStubDrmFlags.NoEncryption)
                return true;

            // Find the code section..
            var codeSection = this.File.GetOwnerSection(this.StubHeader.CodeSectionVirtualAddress);
            if (codeSection.PointerToRawData == 0 || codeSection.SizeOfRawData == 0)
                return false;

            // Store the code sections index..
            this.CodeSectionIndex = this.File.GetSectionIndex(codeSection);

            return true;
        }

        /// <summary>
        /// Step #5
        /// 
        /// Read, decrypt and process the code section.
        /// </summary>
        /// <returns></returns>
        private bool Step5()
        {
            // Skip decryption if the code section is not encrypted..
            if ((this.StubHeader.Flags & (uint)SteamStubDrmFlags.NoEncryption) == (uint)SteamStubDrmFlags.NoEncryption)
            {
                this.Log(" --> Code section is not encrypted.", LogMessageType.Debug);
                return true;
            }

            try
            {
                // Obtain the code section..
                var codeSection = this.File.Sections[this.CodeSectionIndex];
                this.Log($" --> {codeSection.SectionName} linked as main code section.", LogMessageType.Debug);
                this.Log($" --> {codeSection.SectionName} section is encrypted.", LogMessageType.Debug);

                // Obtain the code section data..
                var codeSectionData = new byte[codeSection.SizeOfRawData + this.StubHeader.CodeSectionStolenData.Length];
                Array.Copy(this.StubHeader.CodeSectionStolenData, (long)0, codeSectionData, 0, this.StubHeader.CodeSectionStolenData.Length);
                Array.Copy(this.File.FileData, (long)this.File.GetFileOffsetFromRva(codeSection.VirtualAddress), codeSectionData, this.StubHeader.CodeSectionStolenData.Length, codeSection.SizeOfRawData);

                // Create the AES decryption helper..
                var aes = new AesHelper(this.StubHeader.AES_Key, this.StubHeader.AES_IV);
                aes.RebuildIv(this.StubHeader.AES_IV);

                // Decrypt the code section data..
                var data = aes.Decrypt(codeSectionData, CipherMode.CBC, PaddingMode.None);
                if (data == null)
                    return false;

                // Set the code section override data..
                this.CodeSectionData = data;

                return true;
            }
            catch
            {
                this.Log(" --> Error trying to decrypt the files code section data!", LogMessageType.Error);
                return false;
            }
        }

        /// <summary>
        /// Step #6
        /// 
        /// Rebuild and save the unpacked file.
        /// </summary>
        /// <returns></returns>
        private bool Step6()
        {
            FileStream fStream = null;

            try
            {
                // Zero the DosStubData if desired..
                if (this.Options.ZeroDosStubData && this.File.DosStubSize > 0)
                    this.File.DosStubData = Enumerable.Repeat((byte)0, (int)this.File.DosStubSize).ToArray();

                // Rebuild the file sections..
                this.File.RebuildSections(this.Options.DontRealignSections == false);

                // Open the unpacked file for writing..
                var unpackedPath = this.File.FilePath + ".unpacked.exe";
                fStream = new FileStream(unpackedPath, FileMode.Create, FileAccess.ReadWrite);

                // Write the DOS header to the file..
                fStream.WriteBytes(Pe64Helpers.GetStructureBytes(this.File.DosHeader));

                // Write the DOS stub to the file..
                if (this.File.DosStubSize > 0)
                    fStream.WriteBytes(this.File.DosStubData);

                // Update the NT headers..
                var ntHeaders = this.File.NtHeaders;
                if (this.StubHeader.HasTlsCallback != 1)
                    ntHeaders.OptionalHeader.AddressOfEntryPoint = this.StubHeader.OriginalEntryPoint;
                else
                    ntHeaders.OptionalHeader.AddressOfEntryPoint = this.TlsOepOverride;
                ntHeaders.OptionalHeader.CheckSum = 0;
                this.File.NtHeaders = ntHeaders;

                // Write the NT headers to the file..
                fStream.WriteBytes(Pe64Helpers.GetStructureBytes(ntHeaders));

                // Write the sections to the file..
                for (var x = 0; x < this.File.Sections.Count; x++)
                {
                    var section = this.File.Sections[x];
                    var sectionData = this.File.SectionData[x];

                    // Write the section header to the file..
                    fStream.WriteBytes(Pe64Helpers.GetStructureBytes(section));

                    // Set the file pointer to the sections raw data..
                    var sectionOffset = fStream.Position;
                    fStream.Position = section.PointerToRawData;

                    // Write the sections raw data..
                    var sectionIndex = this.File.Sections.IndexOf(section);
                    if (sectionIndex == this.CodeSectionIndex)
                        fStream.WriteBytes(this.CodeSectionData ?? sectionData);
                    else
                        fStream.WriteBytes(sectionData);

                    // Reset the file offset..
                    fStream.Position = sectionOffset;
                }

                // Set the stream to the end of the file..
                fStream.Position = fStream.Length;

                // Write the overlay data if it exists..
                if (this.File.OverlayData != null)
                    fStream.WriteBytes(this.File.OverlayData);

                this.Log(" --> Unpacked file saved to disk!", LogMessageType.Success);
                this.Log($" --> File Saved As: {unpackedPath}", LogMessageType.Success);

                return true;
            }
            catch
            {
                this.Log(" --> Error trying to save unpacked file!", LogMessageType.Error);
                return false;
            }
            finally
            {
                fStream?.Dispose();
            }
        }

        /// <summary>
        /// Step #7
        /// 
        /// Recalculate the file checksum.
        /// </summary>
        /// <returns></returns>
        private bool Step7()
        {
            var unpackedPath = this.File.FilePath + ".unpacked.exe";
            if (!Pe64Helpers.UpdateFileChecksum(unpackedPath))
            {
                this.Log(" --> Error trying to recalculate unpacked file checksum!", LogMessageType.Error);
                return false;
            }

            this.Log(" --> Unpacked file updated with new checksum!", LogMessageType.Success);
            return true;

        }

        /// <summary>
        /// Gets or sets if the Tls callback is being used as the Oep.
        /// </summary>
        private bool TlsAsOep { get; set; }

        /// <summary>
        /// Gets or sets the Tls Oep Rva if it is being used as the Oep.
        /// </summary>
        private ulong TlsOepRva { get; set; }

        /// <summary>
        /// Gets or sets the Tls Oep override value to use when the stub has set the HasTlsCallback flag.
        /// </summary>
        private uint TlsOepOverride { get; set; }

        /// <summary>
        /// Gets or sets the Steamless options this file was requested to process with.
        /// </summary>
        private SteamlessOptions Options { get; set; }

        /// <summary>
        /// Gets or sets the file being processed.
        /// </summary>
        private Pe64File File { get; set; }

        /// <summary>
        /// Gets or sets the current xor key being used against the file data.
        /// </summary>
        private uint XorKey { get; set; }

        /// <summary>
        /// Gets or sets the DRM stub header.
        /// </summary>
        private SteamStub64Var30Header StubHeader { get; set; }

        /// <summary>
        /// Gets or sets the index of the code section.
        /// </summary>
        private int CodeSectionIndex { get; set; }

        /// <summary>
        /// Gets or sets the decrypted code section data.
        /// </summary>
        private byte[] CodeSectionData { get; set; }
    }
}