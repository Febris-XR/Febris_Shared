// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using Febris.SharedServices;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Tests for <see cref="CookieHandler"/>.
    ///
    /// <para>
    /// <see cref="CookieHandler"/> reads the value of the <c>Febris.AuthCookie</c> cookie
    /// from the current HTTP request and stores it in the public <c>ThinMint</c> field
    /// when the handler is constructed. Despite the <c>async</c> signature on
    /// <c>CookieJarCrusade</c>, the method body has no <c>await</c> operators -- the
    /// implementation runs synchronously, so reading <c>ThinMint</c> directly after
    /// construction is safe and tests do not need to add delays.
    /// </para>
    /// </summary>
    public class CookieHandlerTests
    {
        // Builds a mocked IHttpContextAccessor whose request exposes the given cookie collection.
        private static IHttpContextAccessor BuildAccessor(IDictionary<string, string> cookies)
        {
            var requestCookies = new Mock<IRequestCookieCollection>();
            // IRequestCookieCollection's indexer returns null for missing keys.
            requestCookies.Setup(c => c[It.IsAny<string>()])
                          .Returns<string>(key => cookies != null && cookies.ContainsKey(key) ? cookies[key] : null);

            var request = new Mock<HttpRequest>();
            request.SetupGet(r => r.Cookies).Returns(requestCookies.Object);

            var httpContext = new Mock<HttpContext>();
            httpContext.SetupGet(c => c.Request).Returns(request.Object);

            var accessor = new Mock<IHttpContextAccessor>();
            accessor.SetupGet(a => a.HttpContext).Returns(httpContext.Object);
            return accessor.Object;
        }

        [Fact]
        public void Ctor_WhenAuthCookiePresent_ReadsItIntoThinMint()
        {
            var accessor = BuildAccessor(new Dictionary<string, string>
            {
                ["Febris.AuthCookie"] = "session-token-abc"
            });

            var sut = new CookieHandler(accessor);

            sut.ThinMint.Should().Be("session-token-abc");
        }

        [Fact]
        public void Ctor_WhenAuthCookieAbsent_LeavesThinMintEmpty()
        {
            // Missing cookie -> IRequestCookieCollection indexer returns null -> ThinMint is
            // assigned null (effectively overwriting the empty-string initializer).
            var accessor = BuildAccessor(new Dictionary<string, string>());

            var sut = new CookieHandler(accessor);

            // The implementation assigns whatever the cookies indexer returns, including null.
            sut.ThinMint.Should().BeNullOrEmpty();
        }

        [Fact]
        public void Ctor_WhenHttpContextIsNull_SwallowsExceptionAndLeavesThinMintEmpty()
        {
            // The handler's catch block swallows the NullReferenceException that occurs when
            // accessing HttpContext.Request on a null HttpContext.
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.SetupGet(a => a.HttpContext).Returns((HttpContext)null);

            Action act = () => new CookieHandler(accessor.Object);

            act.Should().NotThrow();
            new CookieHandler(accessor.Object).ThinMint.Should().BeNullOrEmpty();
        }

        [Fact]
        public void CookieName_Constant_IsFebrisAuthCookie()
        {
            // The constant is part of the public contract; downstream code may use it to set
            // the cookie symmetrically. Keep it stable.
            CookieHandler.CookieName.Should().Be("Febris.AuthCookie");
        }

        [Fact]
        public void CookieName_Constant_MatchesHardcodedStringInCookieJarCrusade_DocumentsRisk()
        {
            // There is a latent risk: CookieJarCrusade reads the cookie by the literal string
            // "Febris.AuthCookie" rather than by the CookieName constant. This test pins the
            // current alignment so a future rename of CookieName immediately surfaces here as
            // a reminder to also update the hardcoded literal in CookieJarCrusade.
            //
            // See ..\BUGS.md for the related cleanup item.
            const string hardcodedInImplementation = "Febris.AuthCookie";
            CookieHandler.CookieName.Should().Be(hardcodedInImplementation);
        }

        [Fact]
        public void ICookieHandler_ThinMint_ReturnsConstructorReadValue()
        {
            // Explicit interface implementation forwards to the concrete field. Callers depending
            // on the interface must see the same value as direct field access.
            var accessor = BuildAccessor(new Dictionary<string, string>
            {
                ["Febris.AuthCookie"] = "session-token-xyz"
            });

            ICookieHandler sut = new CookieHandler(accessor);

            sut.ThinMint.Should().Be("session-token-xyz");
        }
    }
}
