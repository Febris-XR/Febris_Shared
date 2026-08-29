// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.IO;
using System.Security.Cryptography;
using Febris.SharedServices.Launcher;

namespace FebrisLocalLibrary.SharedDetails
{
    /// <summary>
    /// Secondary entropy for the DPAPI-protected launcher credential files.
    ///
    /// <para>
    /// Generated once per installation from a CSPRNG and cached beside the credential files it
    /// protects, so the value differs on every machine. It is deliberately not a compile-time
    /// constant: a constant that ships in source lets anyone holding a credential blob unprotect
    /// it with a value read straight out of the repository, which defeats the only barrier
    /// standing between a stolen user.dat/s.dat pair and the plaintext.
    /// </para>
    /// </summary>
    static class InternalStaticDetails
    {
        private const int EntropyLengthBytes = 32;
        private const string EntropyFileName = "entropy.dat";

        private static readonly object _gate = new object();
        private static byte[] _entropy;

        //credentials
        internal static byte[] entropy
        {
            get
            {
                if (_entropy == null)
                {
                    lock (_gate)
                    {
                        if (_entropy == null)
                        {
                            _entropy = LoadOrCreate();
                        }
                    }
                }

                return _entropy;
            }
        }

        /// <summary>
        /// Reads this installation's entropy, creating it on first use. A short or truncated file
        /// is treated as absent and regenerated. Failures to persist are allowed to propagate:
        /// silently continuing with entropy that never reached disk would encrypt credentials
        /// that no later run could ever decrypt.
        /// </summary>
        private static byte[] LoadOrCreate()
        {
            string path = Path.Combine(PCFileSystem.sLocation, EntropyFileName);

            if (File.Exists(path))
            {
                byte[] existing = File.ReadAllBytes(path);
                if (existing.Length == EntropyLengthBytes)
                {
                    return existing;
                }
            }

            byte[] generated = new byte[EntropyLengthBytes];
            RandomNumberGenerator.Fill(generated);

            Directory.CreateDirectory(PCFileSystem.sLocation);
            File.WriteAllBytes(path, generated);

            return generated;
        }
    }
}
