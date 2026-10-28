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

namespace Steamless.API.Crypto
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;

    public class AesHelper : IDisposable
    {
        /// <summary>
        /// Internal original key set by the user of this class.
        /// </summary>
        private readonly byte[] m_OriginalKey;

        /// <summary>
        /// Internal original iv set by the user of this class.
        /// </summary>
        private readonly byte[] m_OriginalIv;

        /// <summary>
        /// Internal AES crypto provider.
        /// </summary>
        private AesCryptoServiceProvider m_AesCryptoProvider;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <param name="mode"></param>
        /// <param name="padding"></param>
        public AesHelper(byte[] key, byte[] iv, CipherMode mode = CipherMode.ECB, PaddingMode padding = PaddingMode.None)
        {
            // Store the original key and iv..
            this.m_OriginalKey = key;
            this.m_OriginalIv = iv;

            // Create the AES crypto provider..
            this.m_AesCryptoProvider = new AesCryptoServiceProvider
            {
                Key = key,
                IV = iv,
                Mode = mode,
                Padding = padding
            };
        }

        /// <summary>
        /// Default Deconstructor
        /// </summary>
        ~AesHelper()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// IDispose implementation.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// IDispose implementation.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            this.m_AesCryptoProvider?.Dispose();
            this.m_AesCryptoProvider = null;
        }

        /// <summary>
        /// Rebuilds the current iv (or the one given).
        /// </summary>
        /// <param name="iv"></param>
        /// <returns></returns>
        public bool RebuildIv(byte[] iv = null)
        {
            // Use the current iv if none is set..
            if (iv == null)
                iv = this.m_OriginalIv;

            try
            {
                using (var decryptor = this.m_AesCryptoProvider.CreateDecryptor())
                {
                    return decryptor.TransformBlock(iv, 0, iv.Length, this.m_OriginalIv, 0) > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Decrypts the given data using the given mode and padding.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="mode"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public byte[] Decrypt(byte[] data, CipherMode mode, PaddingMode padding)
        {
            if (data == null || data.Length == 0)
                return null;

            ICryptoTransform decryptor = null;

            try
            {
                // Update the mode and padding for the decryption..
                this.m_AesCryptoProvider.Mode = mode;
                this.m_AesCryptoProvider.Padding = padding;

                // Create the decryptor..
                decryptor = this.m_AesCryptoProvider.CreateDecryptor(this.m_OriginalKey, this.m_OriginalIv);

                // Decrypt into a single preallocated buffer instead of growing a List<byte>
                // 16 bytes at a time. The old approach doubled the list capacity (up to 2x
                // the input) and then copied everything again via ToArray(), needing roughly
                // 2.5x the input size on top of the already loaded file data. For a large
                // packed executable that exhausts the address space of a 32bit process.
                var output = new byte[data.Length];
                var written = 0;
                var offset = 0;

                // Process in 64KiB chunks (a multiple of the AES block size)..
                const int chunkSize = 0x10000;
                while (data.Length - offset >= chunkSize)
                {
                    written += decryptor.TransformBlock(data, offset, chunkSize, output, written);
                    offset += chunkSize;
                }

                // Flush the final chunk (and any block the transformer held back)..
                var tail = decryptor.TransformFinalBlock(data, offset, data.Length - offset);
                if (tail.Length > 0)
                {
                    Buffer.BlockCopy(tail, 0, output, written, tail.Length);
                    written += tail.Length;
                }

                // PaddingMode.None preserves the length; trim defensively if it does not..
                if (written != output.Length)
                    Array.Resize(ref output, written);

                return output;
            }
            catch
            {
                return null;
            }
            finally
            {
                decryptor?.Dispose();
            }
        }
    }
}