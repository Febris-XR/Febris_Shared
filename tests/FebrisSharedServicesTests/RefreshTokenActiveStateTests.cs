// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using Febris.ModelLibrary.Models.TicketModels;
using FluentAssertions;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Pins the BaseRefreshLicenseToken.IsActive contract the B-06 fix relies on: a refresh token whose
    /// Revoked timestamp is set is NOT active, so the device/license refresh path
    /// (if (!refreshToken.IsActive) return null) rejects a rotated-out token -- once the rotation
    /// actually persists that revocation back to the cache (the B-06 fix). The shared
    /// License/Hardware rotation previously mutated the old token but never wrote it back, so its cached
    /// copy stayed active and kept minting JWTs.
    /// </summary>
    public class RefreshTokenActiveStateTests
    {
        [Fact]
        public void NotRevoked_AndNotExpired_IsActive()
        {
            new BaseRefreshLicenseToken { Expires = DateTime.UtcNow.AddDays(7), Revoked = null }
                .IsActive.Should().BeTrue();
        }

        [Fact]
        public void Revoked_IsNotActive_EvenWhenNotExpired()
        {
            // The B-06 case: a rotated-out token is marked Revoked, so once that state is persisted the
            // token is inactive and can no longer mint JWTs.
            new BaseRefreshLicenseToken { Expires = DateTime.UtcNow.AddDays(7), Revoked = DateTime.UtcNow }
                .IsActive.Should().BeFalse("a revoked refresh token must not be active");
        }

        [Fact]
        public void Expired_IsNotActive()
        {
            new BaseRefreshLicenseToken { Expires = DateTime.UtcNow.AddDays(-1), Revoked = null }
                .IsActive.Should().BeFalse();
        }
    }
}
