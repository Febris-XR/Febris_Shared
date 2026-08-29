// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
namespace Febris.ModelLibrary.Interfaces.ViewModelInterfaces
{
    public interface IJwtSettings
    {
        string Secret { get; set; }
        string Issuer { get; set; }
        string Audience { get; set; }
        string Subject { get; set; }
        double ExpiryTimeInSeconds { get; set; }
    }
}