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

namespace Steamless.Unpacker.Variant10.x86
{
    using API;
    using API.Events;
    using API.Extensions;
    using API.Model;
    using API.PE32;
    using API.Services;
    using Classes;
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

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
        public override string Name => "SteamStub Variant 1.0 Unpacker (x86)";

        /// <summary>
        /// Gets the description of this plugin.
        /// </summary>
        public override string Description => "Unpacker for the 32bit SteamStub variant 1.0.";

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
                var f = new Pe32File(file);
                if (!f.Parse() || f.IsFile64Bit() || !f.HasSection(".bind"))
                    return false;

                // Obtain the bind section data..
                var bind = f.GetSectionData(".bind");

                // Attempt to locate the known v1.x signature..
                var variant = Pe32Helpers.FindPattern(bind, "60 81 EC 00 10 00 00 BE ?? ?? ?? ?? B9 6A");
                if (variant == -1)
                    return false;

                return true;
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
            this.Options = options;
            this.OriginalEntryPoint = 0;

            // Parse the file..
            this.File = new Pe32File(file);
            if (!this.File.Parse())
                return false;

            // Announce we are being unpacked with this packer..
            this.Log("File is packed with SteamStub Variant 1.0!", LogMessageType.Information);

            this.Log("Step 1 - Read, decode and validate the SteamStub DRM header.", LogMessageType.Information);
            if (!this.Step1())
                return false;

            this.Log("Step 2 - Handle .bind section.", LogMessageType.Information);
            if (!this.Step2())
                return false;

            this.Log("Step 3 - Rebuild and save the unpacked file.", LogMessageType.Information);
            if (!this.Step3())
                return false;

            if (this.Options.RecalculateFileChecksum)
            {
                this.Log("Step 4 - Rebuild unpacked file checksum.", LogMessageType.Information);
                if (!this.Step4())
                    return false;
            }

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
            // Obtain the bind section..
            var section = this.File.GetSection(".bind");
            if (!section.IsValid)
                return false;

            // Find the header information from the unpacker call..
            var bind = this.File.GetSectionData(".bind");
            var offset = Pe32Helpers.FindPattern(bind, "60 81 EC 00 10 00 00 BE ?? ?? ?? ?? B9 6A");
            if (offset == -1)
                return false;

            // Read the needed header information..
            var headerPointer = BitConverter.ToUInt32(bind, (int)offset + 8);
            var headerSize = BitConverter.ToUInt32(bind, (int)offset + 13) * 4;

            // Calculate the file offset from the pointer..
            var fileOffset = this.File.GetFileOffsetFromRva(headerPointer - this.File.NtHeaders.OptionalHeader.ImageBase);

            // Read the header data..
            var headerData = new byte[headerSize];
            Array.Copy(this.File.FileData, fileOffset, headerData, 0, headerSize);

            // Decrypt the header data..
            for (var x = 0; x < headerSize; x++)
                headerData[x] ^= (byte)(x * x);

            // Store the header and validate it..
            this.StubHeader = Pe32Helpers.GetStructure<SteamStub32Var10Header>(headerData);

            // Validate the header via the unpacker function matching the file entry point..
            if (this.StubHeader.BindFunction - this.File.NtHeaders.OptionalHeader.ImageBase != this.File.NtHeaders.OptionalHeader.AddressOfEntryPoint)
                return false;

            // Find the OEP from the unpacker function..
            offset = Pe32Helpers.FindPattern(bind, "61 B8 ?? ?? ?? ?? FF E0");
            if (offset == -1)
                return false;

            // Read and store the real OEP..
            this.OriginalEntryPoint = BitConverter.ToUInt32(bind, (int)offset + 2) - this.File.NtHeaders.OptionalHeader.ImageBase;

            return true;
        }

        /// <summary>
        /// Step #2
        /// 
        /// Remove the bind section if requested.
        /// Find the code section.
        /// </summary>
        /// <returns></returns>
        private bool Step2()
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

            return true;
        }

        /// <summary>
        /// Step #3
        /// 
        /// Rebuild and save the unpacked file.
        /// </summary>
        /// <returns></returns>
        private bool Step3()
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
                fStream.WriteBytes(Pe32Helpers.GetStructureBytes(this.File.DosHeader));

                // Write the DOS stub to the file..
                if (this.File.DosStubSize > 0)
                    fStream.WriteBytes(this.File.DosStubData);

                // Update the NT headers..
                var ntHeaders = this.File.NtHeaders;
                ntHeaders.OptionalHeader.AddressOfEntryPoint = this.OriginalEntryPoint;
                ntHeaders.OptionalHeader.CheckSum = 0;
                this.File.NtHeaders = ntHeaders;

                // Write the NT headers to the file..
                fStream.WriteBytes(Pe32Helpers.GetStructureBytes(ntHeaders));

                // Write the sections to the file..
                for (var x = 0; x < this.File.Sections.Count; x++)
                {
                    var section = this.File.Sections[x];
                    var sectionData = this.File.SectionData[x];

                    // Write the section header to the file..
                    fStream.WriteBytes(Pe32Helpers.GetStructureBytes(section));

                    // Set the file pointer to the sections raw data..
                    var sectionOffset = fStream.Position;
                    fStream.Position = section.PointerToRawData;

                    // Write the sections raw data..
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
        /// Step #4
        /// 
        /// Recalculate the file checksum.
        /// </summary>
        /// <returns></returns>
        private bool Step4()
        {
            var unpackedPath = this.File.FilePath + ".unpacked.exe";
            if (!Pe32Helpers.UpdateFileChecksum(unpackedPath))
            {
                this.Log(" --> Error trying to recalculate unpacked file checksum!", LogMessageType.Error);
                return false;
            }

            this.Log(" --> Unpacked file updated with new checksum!", LogMessageType.Success);
            return true;

        }

        /// <summary>
        /// Gets or sets the Steamless options this file was requested to process with.
        /// </summary>
        private SteamlessOptions Options { get; set; }

        /// <summary>
        /// Gets or sets the file being processed.
        /// </summary>
        private Pe32File File { get; set; }

        /// <summary>
        /// Gets or sets the DRM stub header.
        /// </summary>
        private SteamStub32Var10Header StubHeader { get; set; }

        /// <summary>
        /// Gets or sets the true entry point take from the bind unpacker function.
        /// </summary>
        private uint OriginalEntryPoint { get; set; }
    }
}