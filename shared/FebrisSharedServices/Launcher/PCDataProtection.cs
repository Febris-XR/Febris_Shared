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
                _log.LogError(ex.Message);
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
                _log.LogError(ex.Message);
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
                _log.LogError(ex.Message);
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
                    _log.LogError(ex.Message);
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
                    _log.LogError(ex.Message);
                }
                if (userSecretGathered && userNameGathered)
                {
                    credsExist = true;
                }
                return (credsExist, userName, secret);

            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
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
                _log.LogError(ex.Message);
                return saved;
                //throw;
            }
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
                _log.LogError(ex.Message);
            }
            return exist;
        }
    }
}
