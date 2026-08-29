// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Febris.SharedServices
{
    /// <summary>
    /// Strips secret VALUES out of a request query string before anything stores it.
    ///
    /// <para>
    /// WHY (audit H-26, a cut line A publication blocker). ASP.NET Identity puts its
    /// password-reset and email-confirmation tokens in the QUERY STRING of the emailed link --
    /// <c>/Identity/Account/ResetPassword?code=...</c>. The analytics middleware records
    /// <c>Request.QueryString</c> verbatim on EVERY request, so the moment a user clicks their
    /// reset link the token lands in the analytics table. <c>LocalAnalyticsController</c> is
    /// <c>[Authorize(Roles = OrgAdmins)]</c> and the views render <c>Query</c> directly, so any Org
    /// Admin could read a live reset token for any account -- including another admin's -- and the
    /// row is never purged.
    /// </para>
    ///
    /// <para>
    /// REDACTED AT CAPTURE, not at render. Redacting on display would leave the secret in the
    /// database, in every backup, and in any future consumer of the table.
    /// </para>
    ///
    /// <para>
    /// KEYS ARE KEPT AND NON-SENSITIVE VALUES ARE KEPT VERBATIM, deliberately. The stored
    /// <c>Query</c> has a real reader: the bot/attack heuristic in <c>AnalyticsLogic</c> scans it
    /// for fingerprints such as <c>SELECT</c>, <c>.git</c>, <c>.env</c> and <c>wp-includes</c>.
    /// Blanking the field outright would have quietly disabled that detection -- a write-side fix
    /// that breaks a read side nobody looked at.
    /// </para>
    ///
    /// <para>
    /// Never throws. The input is attacker-controlled, and an exception here would fault a
    /// fire-and-forget analytics task rather than fail visibly.
    /// </para>
    /// </summary>
    public static class SensitiveQueryRedactor
    {
        /// <summary>What replaces a secret value. Deliberately obvious in a report.</summary>
        public const string Placeholder = "[REDACTED]";

        /// <summary>
        /// Query keys whose VALUE is a credential. Matched case-insensitively and exactly (not by
        /// substring), so a parameter merely containing one of these words is untouched.
        ///
        /// <para>
        /// <c>code</c> is the important one: it is the ASP.NET Identity convention for BOTH the
        /// password-reset and the email-confirmation token. The rest are defence in depth for the
        /// external-login and API flows.
        /// </para>
        ///
        /// <para>
        /// NOT included: <c>userId</c> and <c>email</c>. Neither is a takeover credential on its
        /// own, and treating identifiers as secrets here would be a PII retention decision that
        /// belongs with the PII map, not smuggled in behind a token fix.
        /// </para>
        /// </summary>
        public static readonly IReadOnlyCollection<string> SensitiveKeys = new HashSet<string>(
            new[]
            {
                "code",
                "token",
                "access_token",
                "refresh_token",
                "id_token",
                "password",
                "pwd",
                "secret",
                "apikey",
                "api_key"
            },
            StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Returns the query string with the values of <see cref="SensitiveKeys"/> replaced.
        /// Input and output both include the leading '?' when present.
        /// </summary>
        public static string Redact(string queryString)
        {
            if (string.IsNullOrEmpty(queryString))
            {
                return queryString;
            }

            try
            {
                bool leadingQuestionMark = queryString[0] == '?';
                string body = leadingQuestionMark ? queryString.Substring(1) : queryString;

                if (body.Length == 0)
                {
                    return queryString;
                }

                StringBuilder builder = new StringBuilder(queryString.Length);
                if (leadingQuestionMark)
                {
                    builder.Append('?');
                }

                string[] pairs = body.Split('&');
                for (int i = 0; i < pairs.Length; i++)
                {
                    if (i > 0)
                    {
                        builder.Append('&');
                    }

                    string pair = pairs[i];
                    int equals = pair.IndexOf('=');

                    // A bare flag with no '=' carries no value to leak, so it survives untouched.
                    if (equals < 0)
                    {
                        builder.Append(pair);
                        continue;
                    }

                    string key = pair.Substring(0, equals);
                    if (SensitiveKeys.Contains(key))
                    {
                        builder.Append(key).Append('=').Append(Placeholder);
                    }
                    else
                    {
                        builder.Append(pair);
                    }
                }

                return builder.ToString();
            }
            catch (Exception ex)
            {
                // Redaction failing OPEN would defeat the point, so fail CLOSED: drop the query
                // entirely rather than risk storing a token we failed to parse.
                FebrisLog.Error(ex, "SensitiveQueryRedactor: failed to parse a query string; dropping it rather than storing it.");
                return Placeholder;
            }
        }
    }
}
