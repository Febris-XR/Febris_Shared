// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Febris.SharedServices
{
    public interface IPasswordGenerator
    {
        string PasswordRandomize();
    }
    public class PasswordGenerator:IPasswordGenerator
    {
        #region Password handling
        //***********************************************************************************************************************************
        //password randomizer
        //There is deliberately no build-configuration branch here: every configuration must
        //generate passwords of identical strength, and every character and insertion position
        //is drawn from a CSPRNG.
        //***********************************************************************************************************************************
        public string PasswordRandomize()
        {
            var opts = new PasswordOptions()
            {
                RequiredLength = 20,
                RequiredUniqueChars = 4,
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true
            };

            string[] randomChars = new[] {
                "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase
                "abcdefghijkmnopqrstuvwxyz",    // lowercase
                "0123456789",                   // digits
                "!@$?_-"                        // non-alphanumeric
            };
            List<char> chars = new List<char>();

            if (opts.RequireUppercase)
                chars.Insert(NextIndex(chars.Count),
                    randomChars[0][NextIndex(randomChars[0].Length)]);

            if (opts.RequireLowercase)
                chars.Insert(NextIndex(chars.Count),
                    randomChars[1][NextIndex(randomChars[1].Length)]);

            if (opts.RequireDigit)
                chars.Insert(NextIndex(chars.Count),
                    randomChars[2][NextIndex(randomChars[2].Length)]);

            if (opts.RequireNonAlphanumeric)
                chars.Insert(NextIndex(chars.Count),
                    randomChars[3][NextIndex(randomChars[3].Length)]);

            for (int i = chars.Count; i < opts.RequiredLength
                || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[NextIndex(randomChars.Length)];
                chars.Insert(NextIndex(chars.Count),
                    rcs[NextIndex(rcs.Length)]);
            }

            return new string(chars.ToArray());
        }

        // Uniform random index in [0, exclusiveUpperBound), CSPRNG-backed.
        // GetInt32 rejects a non-positive bound, so the empty-list case is mapped to 0
        // to preserve the insertion semantics the caller relies on.
        private static int NextIndex(int exclusiveUpperBound)
        {
            return exclusiveUpperBound <= 0 ? 0 : RandomNumberGenerator.GetInt32(exclusiveUpperBound);
        }
        #endregion
    }
}
