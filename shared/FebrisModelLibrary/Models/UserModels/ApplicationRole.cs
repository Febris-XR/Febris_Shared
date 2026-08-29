// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.Models.UserModels
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        //not sure if either of these are needed.
        public ApplicationRole() : base() { }

        public ApplicationRole(string roleName) : base(roleName) { }

    }
}
