// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Models.DataModels;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Pins the pure recipient-binding predicate lifted out of the invite-accept flow for DEV-B10
    /// (InviteRecipientMatch.RecipientEmailMatches). This is the testable primitive the deferred
    /// structural wiring (DEV-M9) will call from ContentDeveloperInviteLogic.ConsumeAsync. The
    /// predicate itself changes no authorization decision today because nothing invokes it yet.
    /// </summary>
    public class InviteRecipientMatchTests
    {
        [Fact]
        public void RecipientEmailMatches_IdenticalAddresses_ReturnsTrue()
        {
            Assert.True(InviteRecipientMatch.RecipientEmailMatches("dev@example.com", "dev@example.com"));
        }

        [Fact]
        public void RecipientEmailMatches_DifferentCase_ReturnsTrue()
        {
            // The intended control is case-insensitive: an invitee typing a different case of the
            // invited address is still the same recipient.
            Assert.True(InviteRecipientMatch.RecipientEmailMatches("Dev@Example.com", "dev@example.COM"));
        }

        [Fact]
        public void RecipientEmailMatches_SurroundingWhitespace_ReturnsTrue()
        {
            Assert.True(InviteRecipientMatch.RecipientEmailMatches("dev@example.com", "  dev@example.com  "));
        }

        [Fact]
        public void RecipientEmailMatches_DifferentRecipient_ReturnsFalse()
        {
            // The whole point of the binding: a forwarded token used by a different address fails.
            Assert.False(InviteRecipientMatch.RecipientEmailMatches("dev@example.com", "attacker@evil.com"));
        }

        [Fact]
        public void RecipientEmailMatches_DifferentLocalPart_ReturnsFalse()
        {
            Assert.False(InviteRecipientMatch.RecipientEmailMatches("alice@example.com", "bob@example.com"));
        }

        [Fact]
        public void RecipientEmailMatches_NullSuppliedEmail_ReturnsFalse()
        {
            // A missing form field must fail closed rather than match.
            Assert.False(InviteRecipientMatch.RecipientEmailMatches("dev@example.com", null));
        }

        [Fact]
        public void RecipientEmailMatches_EmptySuppliedEmail_ReturnsFalse()
        {
            Assert.False(InviteRecipientMatch.RecipientEmailMatches("dev@example.com", ""));
        }

        [Fact]
        public void RecipientEmailMatches_WhitespaceOnlySuppliedEmail_ReturnsFalse()
        {
            Assert.False(InviteRecipientMatch.RecipientEmailMatches("dev@example.com", "   "));
        }

        [Fact]
        public void RecipientEmailMatches_NullInvitedEmail_ReturnsFalse()
        {
            Assert.False(InviteRecipientMatch.RecipientEmailMatches(null, "dev@example.com"));
        }

        [Fact]
        public void RecipientEmailMatches_BothNull_ReturnsFalse()
        {
            Assert.False(InviteRecipientMatch.RecipientEmailMatches(null, null));
        }
    }
}
