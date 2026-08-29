// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
namespace Febris.ModelLibrary.ViewModels
{
    /// <summary>
    /// Per-file result row produced by
    /// <c>FebrisSharedLogicLayer/Utility/FebrisSecurityMethods.CheckSumValidation</c>.
    /// Aggregated over the uploaded <c>IFormFileCollection</c> to decide
    /// whether the upload passes integrity check.
    /// <para>
    /// <see cref="Status"/> string is checked literally against "OK" by
    /// the validation aggregator, so the four possible values are:
    /// "Empty file", "No expected checksum provided", "Checksum mismatch",
    /// "OK".
    /// </para>
    /// </summary>
    public class FileUploadResult
    {
        public string FileName { get; set; }
        public string Status { get; set; }
        public string SHA256 { get; set; }
    }
}
