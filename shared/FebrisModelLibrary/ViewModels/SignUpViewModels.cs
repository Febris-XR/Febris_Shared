// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System.ComponentModel.DataAnnotations;

namespace Febris.ModelLibrary.ViewModels
{
    /// <summary>
    /// Input payload for the SSO self-signup flow at <c>/Identity/Account/Register</c>.
    /// Captures the founding-admin user fields plus the brand-new
    /// <see cref="Febris.ModelLibrary.Models.DataModels.ContentDeveloper"/> org
    /// fields in one form. The backend creates the ContentDeveloper row with
    /// <c>PendingSelfSignUp = true</c> and <c>IsLockedOut = true</c>, then
    /// creates the linked ApplicationUser as the org's admin, sends a
    /// verification email, and surfaces the row in the AdminPortal queue.
    /// </summary>
    public class ContentDeveloperSignUpViewModel
    {
        // ----- founding-admin user fields -----

        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone")]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Password must be at least {2} and at most {1} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare(nameof(Password), ErrorMessage = "The password and confirmation do not match.")]
        public string ConfirmPassword { get; set; }

        // ----- new content-developer org fields -----

        [Required]
        [Display(Name = "Organization name")]
        public string OrgName { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }

        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "State / Region")]
        public string State { get; set; }

        [Display(Name = "Zip code")]
        public string ZipCode { get; set; }

        [Display(Name = "Country")]
        public string Country { get; set; }

        // ----- anti-abuse -----

        /// <summary>
        /// reCAPTCHA v3 token produced client-side by grecaptcha.execute()
        /// and verified server-side against Google's siteverify endpoint
        /// before the rest of the form is processed. Populated by JS, not by
        /// the user; never displayed in the form.
        /// </summary>
        public string RecaptchaToken { get; set; }
    }

    /// <summary>
    /// Input payload for the invite-acceptance flow at
    /// <c>/Identity/Account/AcceptInvite</c>. The token comes in via the
    /// query string; the rest of the form collects what the accept page
    /// needs to actually create the user. Org name and invited email are
    /// already resolved from the token server-side -- the view shows them
    /// read-only for confirmation.
    /// </summary>
    public class ContentDeveloperUserInviteAcceptViewModel
    {
        /// <summary>The invite token (ContentDeveloperUserInvite.UUID).
        /// Comes from the link in the invite email.</summary>
        [System.ComponentModel.DataAnnotations.Required]
        public System.Guid Token { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.Display(Name = "First name")]
        public string FirstName { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.Display(Name = "Last name")]
        public string LastName { get; set; }

        [System.ComponentModel.DataAnnotations.Phone]
        [System.ComponentModel.DataAnnotations.Display(Name = "Phone")]
        public string PhoneNumber { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(100, ErrorMessage = "Password must be at least {2} and at most {1} characters long.", MinimumLength = 8)]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Password")]
        public string Password { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare(nameof(Password), ErrorMessage = "The password and confirmation do not match.")]
        public string ConfirmPassword { get; set; }
    }

    /// <summary>
    /// Input payload for the activation-link flow at
    /// <c>/Identity/Account/SetPassword</c>. <see cref="UserId"/> +
    /// <see cref="Code"/> come from the link in the activation email
    /// (round-tripped as hidden fields once the form renders). The user
    /// supplies the new password they want to use.
    /// </summary>
    public class SetInitialPasswordViewModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        public System.Guid UserId { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public string Code { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(100, ErrorMessage = "Password must be at least {2} and at most {1} characters long.", MinimumLength = 8)]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Password")]
        public string Password { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare(nameof(Password), ErrorMessage = "The password and confirmation do not match.")]
        public string ConfirmPassword { get; set; }
    }

}
