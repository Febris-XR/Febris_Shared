// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.IO;
using System.Threading.Tasks;
using Febris.SharedServices;
using FluentAssertions;
using Xunit;

namespace Febris.SharedServices.Tests
{
    public class FileScanServiceTests : IDisposable
    {
        // Reset the static facade after each test so configured scanners do not leak.
        public void Dispose() => FileScanService.Configure(null);

        private static Stream Empty() => new MemoryStream(new byte[] { 1, 2, 3 });

        private sealed class InfectedScanner : IFileScanner
        {
            public Task<FileScanResult> ScanAsync(Stream content, string fileName)
                => Task.FromResult(FileScanResult.Infected("Eicar-Test-Signature"));
        }

        private sealed class CleanScanner : IFileScanner
        {
            public Task<FileScanResult> ScanAsync(Stream content, string fileName)
                => Task.FromResult(FileScanResult.Clean());
        }

        private sealed class ThrowingScanner : IFileScanner
        {
            public Task<FileScanResult> ScanAsync(Stream content, string fileName)
                => throw new InvalidOperationException("scanner down");
        }

        [Fact]
        public async Task NoOpScanner_ReportsNotScannedButClean()
        {
            var r = await new NoOpFileScanner().ScanAsync(Empty(), "x.bin");
            r.Scanned.Should().BeFalse();
            r.IsClean.Should().BeTrue("an unscanned upload must still proceed");
            r.Threat.Should().BeNull();
        }

        [Fact]
        public async Task Facade_DefaultsToNotConfiguredAndNotScanned()
        {
            FileScanService.IsConfigured.Should().BeFalse();
            var r = await FileScanService.ScanAsync(Empty(), "x.bin");
            r.Scanned.Should().BeFalse();
            r.IsClean.Should().BeTrue();
        }

        [Fact]
        public async Task Facade_WithInfectedScanner_ReportsInfected()
        {
            FileScanService.Configure(new InfectedScanner());
            FileScanService.IsConfigured.Should().BeTrue();
            var r = await FileScanService.ScanAsync(Empty(), "evil.bin");
            r.Scanned.Should().BeTrue();
            r.IsClean.Should().BeFalse();
            r.Threat.Should().Be("Eicar-Test-Signature");
        }

        [Fact]
        public async Task Facade_WithCleanScanner_ReportsClean()
        {
            FileScanService.Configure(new CleanScanner());
            var r = await FileScanService.ScanAsync(Empty(), "ok.bin");
            r.Scanned.Should().BeTrue();
            r.IsClean.Should().BeTrue();
        }

        [Fact]
        public async Task Facade_ScannerThrows_DegradesToNotScanned()
        {
            FileScanService.Configure(new ThrowingScanner());
            var r = await FileScanService.ScanAsync(Empty(), "x.bin");
            r.Scanned.Should().BeFalse("a scanner outage must not block uploads");
            r.IsClean.Should().BeTrue();
        }

        [Fact]
        public void Configure_Null_ResetsToNoOp()
        {
            FileScanService.Configure(new InfectedScanner());
            FileScanService.IsConfigured.Should().BeTrue();
            FileScanService.Configure(null);
            FileScanService.IsConfigured.Should().BeFalse();
        }
    }
}
