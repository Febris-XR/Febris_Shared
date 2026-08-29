// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Febris.SharedServices
{
    /// <summary>
    /// Verifies a Google reCAPTCHA v3 token against Google's siteverify
    /// endpoint. v3 returns a probability score (0.0 = bot, 1.0 = human);
    /// callers configure the cutoff via <see cref="RecaptchaSettings.MinimumScore"/>.
    /// Used by the SSO Register page before any user-creation work happens
    /// so abusive POSTs don't even reach the DB.
    /// </summary>
    public interface IRecaptchaVerifier
    {
        /// <summary>
        /// Verify the token returned by grecaptcha.execute() on the client.
        /// </summary>
        /// <param name="token">The token from the form; safe to pass null --
        /// the method returns failure rather than throwing.</param>
        /// <param name="remoteIp">Optional originating IP. Passed to Google
        /// when present; improves their abuse signal but not required.</param>
        /// <param name="expectedAction">Optional v3 action name. When set,
        /// verification fails if Google's reported action doesn't match;
        /// helps detect token replay across endpoints.</param>
        Task<RecaptchaVerificationResult> VerifyAsync(string token, string remoteIp = null, string expectedAction = null);
    }

    /// <summary>
    /// Concrete verifier. POSTs to
    /// https://www.google.com/recaptcha/api/siteverify with the configured
    /// secret + the user's token and parses the JSON response. Honors
    /// <see cref="RecaptchaSettings.BypassForLocalDev"/> so local dev
    /// without internet access can still test the signup flow.
    /// </summary>
    public class RecaptchaVerifier : IRecaptchaVerifier
    {
        private const string SiteVerifyUrl = "https://www.google.com/recaptcha/api/siteverify";

        private readonly RecaptchaSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RecaptchaVerifier> _logger;

        /// <summary>
        /// DI constructor. Settings come from
        /// <c>IOptions&lt;RecaptchaSettings&gt;</c> bound in Startup;
        /// HttpClient comes from <c>IHttpClientFactory</c> to avoid socket
        /// exhaustion on bursty traffic.
        /// </summary>
        public RecaptchaVerifier(
            IOptions<RecaptchaSettings> settings,
            IHttpClientFactory httpClientFactory,
            ILogger<RecaptchaVerifier> logger)
        {
            _settings = settings?.Value ?? new RecaptchaSettings();
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<RecaptchaVerificationResult> VerifyAsync(string token, string remoteIp = null, string expectedAction = null)
        {
            // Local-dev bypass: lets the team iterate on the Register page
            // without standing up a reCAPTCHA test site. Production
            // appsettings MUST NOT set this flag.
            if (_settings.BypassForLocalDev)
            {
                _logger.LogWarning("RecaptchaVerifier bypass is enabled -- self-signup is not abuse-protected in this environment.");
                return new RecaptchaVerificationResult { Success = true, Score = 1.0 };
            }

            // Empty token short-circuits before any HTTP call -- the client
            // didn't run grecaptcha.execute() or the form was submitted via
            // a non-browser tool.
            if (string.IsNullOrWhiteSpace(token))
            {
                return Fail("missing-input-response");
            }

            if (string.IsNullOrWhiteSpace(_settings.SecretKey))
            {
                _logger.LogError("Recaptcha secret key is not configured. Refusing to verify.");
                return Fail("missing-secret-key");
            }

            string responseBody;
            try
            {
                // Build the POST body. siteverify expects
                // application/x-www-form-urlencoded with `secret`, `response`,
                // and optionally `remoteip`.
                var formFields = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("secret", _settings.SecretKey),
                    new KeyValuePair<string, string>("response", token)
                };
                if (!string.IsNullOrWhiteSpace(remoteIp))
                {
                    formFields.Add(new KeyValuePair<string, string>("remoteip", remoteIp));
                }

                using var content = new FormUrlEncodedContent(formFields);
                using HttpClient client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                HttpResponseMessage httpResponse = await client.PostAsync(SiteVerifyUrl, content);
                responseBody = await httpResponse.Content.ReadAsStringAsync();

                if (!httpResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Recaptcha siteverify returned HTTP {Status}.", httpResponse.StatusCode);
                    return Fail("siteverify-http-error");
                }
            }
            catch (Exception ex)
            {
                // Network failures shouldn't take down signup -- but they
                // also shouldn't open it up. Fail closed.
                _logger.LogError(ex, "Recaptcha siteverify call failed.");
                return Fail("siteverify-network-error");
            }

            SiteVerifyResponse parsed;
            try
            {
                parsed = JsonConvert.DeserializeObject<SiteVerifyResponse>(responseBody);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Recaptcha siteverify returned unparseable body: {Body}", responseBody);
                return Fail("siteverify-parse-error");
            }

            if (parsed == null || !parsed.Success)
            {
                return new RecaptchaVerificationResult
                {
                    Success = false,
                    Score = parsed?.Score ?? 0,
                    ErrorCode = parsed?.ErrorCodes != null && parsed.ErrorCodes.Count > 0 ? parsed.ErrorCodes[0] : "siteverify-rejected"
                };
            }

            // Action match (v3 only). Skipped when caller didn't specify.
            if (!string.IsNullOrWhiteSpace(expectedAction) && !string.Equals(parsed.Action, expectedAction, StringComparison.Ordinal))
            {
                return new RecaptchaVerificationResult
                {
                    Success = false,
                    Score = parsed.Score,
                    ErrorCode = "action-mismatch"
                };
            }

            // Score cutoff (v3 only). v2 responses don't include a score
            // and parsed.Score will be 0; treat 0 as below threshold so v2
            // callers must use a different mechanism.
            if (parsed.Score < _settings.MinimumScore)
            {
                return new RecaptchaVerificationResult
                {
                    Success = false,
                    Score = parsed.Score,
                    ErrorCode = "score-below-threshold"
                };
            }

            return new RecaptchaVerificationResult
            {
                Success = true,
                Score = parsed.Score
            };
        }

        /// <summary>
        /// Helper: build a failed result with the given error code.
        /// </summary>
        private static RecaptchaVerificationResult Fail(string errorCode) => new RecaptchaVerificationResult
        {
            Success = false,
            Score = 0,
            ErrorCode = errorCode
        };

        /// <summary>
        /// Mirror of Google's siteverify response JSON. Only the fields we
        /// actually inspect are mapped; `challenge_ts` and `hostname` are
        /// ignored.
        /// </summary>
        private class SiteVerifyResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("score")]
            public double Score { get; set; }

            [JsonProperty("action")]
            public string Action { get; set; }

            [JsonProperty("error-codes")]
            public List<string> ErrorCodes { get; set; }
        }
    }

    /// <summary>
    /// Outcome of <see cref="IRecaptchaVerifier.VerifyAsync"/>. Callers
    /// gate proceed/abort on <see cref="Success"/>; <see cref="ErrorCode"/>
    /// is for logs (never displayed to the user, since it would leak
    /// abuse-detection internals).
    /// </summary>
    public class RecaptchaVerificationResult
    {
        public bool Success { get; set; }

        /// <summary>v3 confidence score, 0.0 - 1.0. Always 0 on v2 or
        /// on failure paths that didn't reach Google.</summary>
        public double Score { get; set; }

        /// <summary>One of Google's error codes (e.g. "timeout-or-duplicate",
        /// "invalid-input-response") or a local code from this verifier
        /// ("missing-input-response", "siteverify-network-error", etc).
        /// Null on success.</summary>
        public string ErrorCode { get; set; }
    }
}
