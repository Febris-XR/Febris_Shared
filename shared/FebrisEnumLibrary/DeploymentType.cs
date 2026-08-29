// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.EnumLibrary
{
    public enum DeploymentType
    {
        None =0,
        TestServer =100,
        Azure = 200,
        AWS =300,
        GoogleCloud = 400,
        DigitalOcean = 500,
        IBMCloud = 600,
        PrivateDeployment = 700
    }
}
