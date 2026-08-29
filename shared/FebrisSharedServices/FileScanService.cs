// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.IO;
using System.Threading.Tasks;

namespace Febris.SharedServices
{
    /// <summary>
    /// Result of an antivirus / malware scan. <see cref="Scanned"/> is false when no engine is
    /// configured (graceful degradation), in which case <see cref="IsClean"/> defaults to true so
    /// the upload still proceeds but is flagged as unscanned.
    /// </summary>
    public sealed class FileScanResult
    {
        public bool Scanned { get; }
        public bool IsClean { get; }
        public string Threat { get; }

        private FileScanResult(bool scanned, bool isClean, string threat)
        {
            Scanned = scanned;
            IsClean = isClean;
            Threat = threat;
        }

        public static FileScanResult Clean() => new FileScanResult(true, true, null);
        public static FileScanResult Infected(string threat) => new FileScanResult(true, false, threat);
        public static FileScanResult NotScanned() => new FileScanResult(false, true, null);
    }

    /// <summary>
    /// Scans an uploaded blob for malware before it is stored or served. Implementations wrap a real
    /// engine (ClamAV, a cloud antivirus service, etc). A host with no engine configured uses
    /// <see cref="NoOpFileScanner"/> so uploads degrade gracefully rather than failing.
    /// </summary>
    public interface IFileScanner
    {
        Task<FileScanResult> ScanAsync(Stream content, string fileName);
    }

    /// <summary>
    /// Default scanner used when no real engine is configured. Always reports
    /// <see cref="FileScanResult.NotScanned"/> so uploads proceed, flagged as unscanned.
    /// </summary>
    public sealed class NoOpFileScanner : IFileScanner
    {
        public Task<FileScanResult> ScanAsync(Stream content, string fileName)
            => Task.FromResult(FileScanResult.NotScanned());
    }

    /// <summary>
    /// Static facade over the configured <see cref="IFileScanner"/>, mirroring <see cref="FebrisLog"/>.
    /// Hand-constructed upload handlers (which are not DI-resolved everywhere) can scan through this
    /// without taking a constructor dependency. Hosts call <see cref="Configure"/> at startup when a
    /// real engine is available; otherwise the facade stays on <see cref="NoOpFileScanner"/>. A scanner
    /// failure never breaks an upload -- it is logged and treated as not-scanned.
    /// </summary>
    public static class FileScanService
    {
        private static IFileScanner _scanner = new NoOpFileScanner();

        /// <summary>True when a real (non no-op) scanner has been configured.</summary>
        public static bool IsConfigured => !(_scanner is NoOpFileScanner);

        public static void Configure(IFileScanner scanner)
        {
            _scanner = scanner ?? new NoOpFileScanner();
        }

        public static async Task<FileScanResult> ScanAsync(Stream content, string fileName)
        {
            try
            {
                return await _scanner.ScanAsync(content, fileName);
            }
            catch (Exception ex)
            {
                // A scanner outage must not block uploads. Log and degrade to not-scanned.
                FebrisLog.Error(ex, "FileScanService.ScanAsync: scanner error, treating as not scanned");
                return FileScanResult.NotScanned();
            }
        }
    }
}
