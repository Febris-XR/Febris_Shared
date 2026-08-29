// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Febris.SharedServices
{
    public class Sha1Handler
    {       
        //***********************************************************************************************************************************
        // converting to Sha1
        //***********************************************************************************************************************************
        public static string TextToHash(string text)
        {
            var sh = SHA1.Create();
            var hash = new StringBuilder();
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            byte[] b = sh.ComputeHash(bytes);
            foreach (byte a in b)
            {
                var h = a.ToString("x2");
                hash.Append(h);
            }
            return hash.ToString();
        }

    }
    public class ShaHandler
    {
        
       
        //***********************************************************************************************************************************
        // converting to Sha2
        //***********************************************************************************************************************************
        public static string TextToSha2(string text)
        {
            var sh = SHA256.Create();
            var hash = new StringBuilder();
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            byte[] b = sh.ComputeHash(bytes);
            foreach (byte a in b)
            {
                var h = a.ToString("x2");
                hash.Append(h);
            }
            return hash.ToString();
        }

    }
}
