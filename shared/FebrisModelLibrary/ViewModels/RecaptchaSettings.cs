// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
namespace Febris.ModelLibrary.ViewModels
{
    /// <summary>
    /// Configuration bound from the <c>Recaptcha</c> section of
    /// <c>appsettings.json</c> via <c>services.Configure&lt;RecaptchaSettings&gt;</c>.
    /// Consumed by <c>RecaptchaVerifier</c> in
    /// <c>FebrisSharedLogicLayer/Logic/Authorization/</c>.
    /// <para>
    /// Required per-environment keys:
    /// <list type="bullet">
    ///   <item><c>SiteKey</c> -- public site key embedded in the Razor form's grecaptcha widget.</item>
    ///   <item><c>SecretKey</c> -- server-only key sent to siteverify.</item>
    ///   <item><c>MinimumScore</c> -- v3 score cutoff (0.0 bot .. 1.0 human); default 0.5.</item>
    ///   <item><c>BypassForLocalDev</c> -- DEBUG-only escape hatch so local
    ///   dev without internet access can still test the signup flow.
    ///   Production / staging appsettings MUST NOT set this true.</item>
    /// </list>
    /// </para>
    /// </summary>
    public class RecaptchaSettings
    {
        public string SiteKey { get; set; }
        public string SecretKey { get; set; }
        public double MinimumScore { get; set; } = 0.5;
        public bool BypassForLocalDev { get; set; }
    }
}
