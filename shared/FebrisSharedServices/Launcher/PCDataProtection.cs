// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using FebrisLocalLibrary.SharedDetails;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Febris.SharedServices.Launcher
{
    public class PCDataProtection
    {
        // NULL LOGGER IS THE NORM HERE, NOT THE EXCEPTION. Every live construction of this class in
        // the three PC clients uses the parameterless constructor, so _log is null in production.
        // The error paths below are therefore null-conditional throughout: an unguarded
        // _log.LogError inside a catch converts a HANDLED failure into a NullReferenceException
        // that escapes the method, which is strictly worse than the failure it was reporting.
        // Found by the NODE-9 tests, where "no credential stored yet" -- the normal state of an
        // unregistered device -- came back as an NRE instead of an empty string.
        private ILogger _log;
        private IConfiguration _config;

        public PCDataProtection(ILogger log, IConfiguration config)
        {
            _log = log;
            _config = config;
        }

        public PCDataProtection()
        {
        }

        public PCDataProtection(ILogger log)
        {
            _log = log;
        }
        #region encrypt
        private void EncryptInput(object input, string path)
        {
            try
            {
                //converts string to byte array
                byte[] toEncrypt = UnicodeEncoding.ASCII.GetBytes(input.ToString());
                //create the file or open it
                FileStream fStream = new FileStream(path, FileMode.OpenOrCreate);
                //encrypt a copy of the data to the stream
                int bytesWritten = EncryptDataToStream(toEncrypt, InternalStaticDetails.entropy, DataProtectionScope.LocalMachine, fStream);
                fStream.Close();
            }
            catch (Exception ex)
            {
                _log?.LogError(ex.Message);
            }
        }

        private static int EncryptDataToStream(byte[] Buffer, byte[] Entropy, DataProtectionScope Scope, Stream stream)
        {
            if (Buffer.Length <= 0)
                throw new ArgumentException("Buffer");
            if (Buffer.Length == 0)
                throw new ArgumentException("Buffer");
            //if (Entropy.Length <= 0)
            //    throw new ArgumentException("Entropy");
            //if (Entropy.Length == 0)
            //    throw new ArgumentException("Entropy");
            //if (stream.Length == 0)
            //    throw new ArgumentException("stream");

            int length = 0;

            //encrypt the data in memory. the result is stored in the same array as the original data. 
            byte[] encryptedData = ProtectedData.Protect(Buffer, Entropy, Scope);

            //write the encrypted data to a stream
            if (stream.CanWrite && encryptedData != null)
            {
                stream.Write(encryptedData, 0, encryptedData.Length);

                length = encryptedData.Length;
            }

            //return length
            return length;
        }

        #endregion


        #region decrypt
        public void DecryptFile(out string output, string path)
        {
            try
            {
                //get file location
                FileStream fStream = new FileStream(path, FileMode.Open, FileAccess.Read);

                //converts string to byte array
                byte[] decryptedData = DecryptDataFromStream(InternalStaticDetails.entropy, DataProtectionScope.LocalMachine, fStream);

                output = UnicodeEncoding.ASCII.GetString(decryptedData);

                fStream.Close();
            }
            catch (Exception ex)
            {
                _log?.LogError(ex.Message);
                throw;
            }
        }

        private byte[] DecryptDataFromStream(byte[] entropy, DataProtectionScope scope, Stream fStream)
        {
            try
            {
                //get data from file
                int length = (int)fStream.Length;
                byte[] buffer = new byte[length];
                int count;
                int sum = 0;

                while ((count = fStream.Read(buffer, sum, length - sum)) > 0)
                    sum += count;


                byte[] data = ProtectedData.Unprotect(buffer, entropy, scope);
                return data;
            }
            catch (Exception ex)
            {
                _log?.LogError(ex.Message);
            }
            throw new NotImplementedException();
        }
        #endregion

        public async Task<(bool CredsExist, string UserName, string Secret)> GetCredentials()
        {
            bool userNameGathered = false;
            bool userSecretGathered = false;
            string userName = string.Empty;
            string secret = string.Empty;
            bool credsExist = false;
            try
            {
                try
                {
                    DecryptFile(out userName, PCFileSystem.userNameLocation);
                    if (userName != string.Empty)
                    {
                        userNameGathered = true;
                    }
                }
                catch (Exception ex)
                {
                    _log?.LogError(ex.Message);
                }
                try
                {
                    DecryptFile(out secret, PCFileSystem.passwordLocation);

                    if (secret != string.Empty)
                    {
                        userSecretGathered = true;
                    }
                }
                catch (Exception ex)
                {
                    _log?.LogError(ex.Message);
                }
                if (userSecretGathered && userNameGathered)
                {
                    credsExist = true;
                }
                return (credsExist, userName, secret);

            }
            catch (Exception ex)
            {
                _log?.LogError(ex.Message);
            }
            return (credsExist, userName, secret);
        }

        public bool SetCredentials(string userName, string secret)
        {
            bool saved = false;
            try
            {
                using (MemoryMappedFile mmf = MemoryMappedFile.CreateNew("creds", 1000))
                {

                    try
                    {
                        EncryptInput(userName, PCFileSystem.userNameLocation);
                    }
                    catch (Exception)
                    {
                        //throw;
                    }
                    try
                    {
                        EncryptInput(secret, PCFileSystem.passwordLocation);
                    }
                    catch (Exception)
                    {
                        //throw;
                    }
                    saved = true;
                    mmf.Dispose();
                }
                return saved;
            }
            catch (Exception ex)
            {
                _log?.LogError(ex.Message);
                return saved;
                //throw;
            }
        }

        /// <summary>
        /// The device credential this client authenticates to a node with (NODE-9).
        ///
        /// <para>
        /// Clients used to derive a licence from WMI (processor id plus motherboard serial) and
        /// send that. Audit T9 changed the node to MINT the credential at registration and store
        /// only its hash, so a self-computed value cannot match any row: the derived-licence path
        /// authenticates nothing. The node shows the minted string once, and the operator pastes
        /// it into this client, which keeps it here encrypted at rest.
        /// </para>
        ///
        /// <para>
        /// Returns empty when nothing is stored. Callers must treat that as "not registered yet"
        /// and say so, rather than falling back to a derived value that the node will reject with
        /// an indistinguishable 401.
        /// </para>
        /// </summary>
        public string GetDeviceCredential()
        {
            return GetDeviceCredential(PCFileSystem.deviceCredentialLocation);
        }

        /// <summary>
        /// Path-explicit overload. Exists so the round trip can be exercised against a temporary
        /// directory rather than writing into the operator's real Documents folder.
        /// </summary>
        internal string GetDeviceCredential(string path)
        {
            // Absence is the NORMAL state of a device nobody has registered yet, so it is checked
            // rather than caught. Relying on the exception would run the common path through
            // DecryptFile's catch, which rethrows.
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return string.Empty;
            }

            string credential = string.Empty;
            try
            {
                DecryptFile(out credential, path);
            }
            catch (Exception ex)
            {
                // The file exists but will not decrypt. The realistic cause is a credential file
                // copied from another machine: the scope is LocalMachine, so DPAPI refuses it.
                // Report empty so the caller says "not registered" rather than crashing.
                _log?.LogError(ex.Message);
                return string.Empty;
            }
            return credential ?? string.Empty;
        }

        /// <summary>
        /// Stores the credential the node minted. Whitespace is trimmed because the value is
        /// copied by hand from a portal page and a trailing space would otherwise hash differently
        /// and fail authentication with no clue why.
        /// </summary>
        public bool SetDeviceCredential(string credential)
        {
            return SetDeviceCredential(credential, PCFileSystem.deviceCredentialLocation);
        }

        /// <summary>Path-explicit overload. See <see cref="GetDeviceCredential(string)"/>.</summary>
        internal bool SetDeviceCredential(string credential, string path)
        {
            string trimmed = (credential ?? string.Empty).Trim();
            if (trimmed.Length == 0)
            {
                // Refuse rather than write nothing. An empty stored credential reads back as "not
                // registered" anyway, and EncryptInput throws on a zero-length buffer, so
                // reporting success here would be a lie either way.
                return false;
            }

            try
            {
                // The credential directory is created by FileSystemInitalizer at startup, but this
                // is reachable before that on a first run, and the resulting
                // DirectoryNotFoundException would be swallowed by EncryptInput and reported as a
                // successful save.
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                EncryptInput(trimmed, path);
            }
            catch (Exception ex)
            {
                _log?.LogError(ex.Message);
                return false;
            }

            // EncryptInput swallows its own failures, so "no exception" is not evidence that the
            // write happened. Confirm the file exists before telling the caller it saved.
            return File.Exists(path);
        }

        public async Task<bool> CredentialsExist()
        {
            bool exist = false;
            try
            {
                bool credentialsGathered = false;
                string user = string.Empty;
                string secret = string.Empty;
                (credentialsGathered, user, secret) = await GetCredentials().ConfigureAwait(false);
                //Cant use this internal static stuff because it will not remain constant
                if (user != string.Empty && secret != string.Empty && credentialsGathered == true)
                {
                    exist = true;
                }

            }
            catch (Exception ex)
            {
                _log?.LogError(ex.Message);
            }
            return exist;
        }
    }
}
