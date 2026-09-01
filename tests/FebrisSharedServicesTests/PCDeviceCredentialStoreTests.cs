// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Febris.SharedServices.Launcher;
using FluentAssertions;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// NODE-9: the PC clients' on-device store for the credential the node mints.
    ///
    /// <para>
    /// WHY THIS EXISTS. Audit T9 changed the node to MINT the device credential at registration and
    /// keep only its hash, but the three PC clients kept deriving a licence from WMI and sending
    /// that. A self-computed value hashes to nothing the node holds, so authentication could never
    /// succeed, and it failed as a plain 401 indistinguishable from a wrong credential, which is
    /// how it went unnoticed. The fix gives the client somewhere to keep the string the node showed
    /// once. These tests pin that store.
    /// </para>
    ///
    /// <para>
    /// EVERY FAILURE HERE IS SILENT IN PRODUCTION. A store that loses the credential, mangles it,
    /// or reports a save that did not happen all present identically at the client: a 401. So the
    /// round trip is checked byte for byte against a REAL <see cref="DeviceCredential.Generate"/>
    /// value rather than a convenient literal.
    /// </para>
    ///
    /// <para>
    /// WINDOWS ONLY. The store is DPAPI (<c>ProtectedData</c>), which exists only on Windows.
    /// These skip elsewhere rather than passing vacuously.
    /// </para>
    /// </summary>
    public class PCDeviceCredentialStoreTests : IDisposable
    {
        private static readonly bool OnWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        private const string SkipReason = "DPAPI (ProtectedData) is Windows-only.";

        private readonly string _directory;
        private readonly string _path;

        public PCDeviceCredentialStoreTests()
        {
            // A temp directory, NOT PCFileSystem.deviceCredentialLocation. The production path is
            // under the operator's real Documents folder, and a test suite must not write there.
            _directory = Path.Combine(Path.GetTempPath(), "febris-node9-" + Guid.NewGuid().ToString("N"));
            _path = Path.Combine(_directory, "d.dat");
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_directory))
                {
                    Directory.Delete(_directory, true);
                }
            }
            catch (IOException)
            {
                // A leftover temp directory is not worth failing a test over.
            }
        }

        /// <summary>
        /// THE REGRESSION THIS SUITE WAS WRITTEN FOR.
        ///
        /// <para>
        /// "No credential stored yet" is the NORMAL state of a device nobody has registered, and it
        /// must read back as empty. The first cut of this code threw instead: every live caller
        /// constructs <c>new PCDataProtection()</c>, leaving the logger null, so the catch block
        /// reporting the missing file dereferenced null and raised a NullReferenceException out of
        /// a method documented to return empty. That would have crashed the launcher's
        /// authentication path on precisely the devices this change exists to help.
        /// </para>
        /// </summary>
        [SkippableFact]
        public void An_absent_credential_reads_as_empty_and_does_not_throw()
        {
            Skip.IfNot(OnWindows, SkipReason);

            PCDataProtection store = new PCDataProtection();   // no logger, exactly as production does

            store.GetDeviceCredential(_path).Should().BeEmpty();
        }

        /// <summary>
        /// The whole point: a real minted credential survives storage unchanged. Byte-for-byte
        /// matters because the node authenticates by hashing what arrives and matching a stored
        /// hash, so a single altered character fails exactly like a wrong credential.
        /// </summary>
        [SkippableFact]
        public void A_minted_credential_round_trips_byte_for_byte()
        {
            Skip.IfNot(OnWindows, SkipReason);

            string minted = DeviceCredential.Generate();
            PCDataProtection store = new PCDataProtection();

            store.SetDeviceCredential(minted, _path).Should().BeTrue();

            store.GetDeviceCredential(_path).Should().Be(minted);
        }

        /// <summary>
        /// Guards the encoding. The store writes through <c>UnicodeEncoding.ASCII</c>, which would
        /// silently replace any character outside ASCII with a question mark. That is safe only
        /// because <see cref="DeviceCredential.Generate"/> emits base64url. If either side ever
        /// changes, this fails here rather than as an unexplained 401 on a device.
        /// </summary>
        [SkippableFact]
        public void A_minted_credential_is_pure_ascii_so_the_stores_encoding_is_lossless()
        {
            Skip.IfNot(OnWindows, SkipReason);

            string minted = DeviceCredential.Generate();

            Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(minted)).Should().Be(minted);
            minted.Should().MatchRegex("^[A-Za-z0-9_-]+$", "the credential is base64url");
        }

        /// <summary>
        /// Trimming is not cosmetic. The credential is copied by hand from a portal page, and a
        /// trailing newline or space hashes to a completely different value: the device would be
        /// rejected with no indication that the only fault was an invisible character.
        /// </summary>
        [SkippableFact]
        public void Surrounding_whitespace_from_a_copy_paste_is_removed()
        {
            Skip.IfNot(OnWindows, SkipReason);

            string minted = DeviceCredential.Generate();
            PCDataProtection store = new PCDataProtection();

            store.SetDeviceCredential("  " + minted + " \r\n", _path).Should().BeTrue();

            store.GetDeviceCredential(_path).Should().Be(minted);
        }

        /// <summary>
        /// Saving nothing must report failure. Reporting success would leave the configuration
        /// screen looking saved while the device stays unregistered.
        /// </summary>
        [SkippableFact]
        public void Storing_an_empty_or_whitespace_credential_is_refused()
        {
            Skip.IfNot(OnWindows, SkipReason);

            PCDataProtection store = new PCDataProtection();

            store.SetDeviceCredential(null, _path).Should().BeFalse();
            store.SetDeviceCredential(string.Empty, _path).Should().BeFalse();
            store.SetDeviceCredential("   ", _path).Should().BeFalse();

            File.Exists(_path).Should().BeFalse("nothing should be written for a refused save");
        }

        /// <summary>
        /// Encrypted at rest. The credential authenticates the device to the node, so it must not
        /// be readable out of the file. This is the property audit T9 was about in the first place.
        /// </summary>
        [SkippableFact]
        public void The_stored_credential_is_not_readable_on_disk()
        {
            Skip.IfNot(OnWindows, SkipReason);

            string minted = DeviceCredential.Generate();
            new PCDataProtection().SetDeviceCredential(minted, _path);

            byte[] raw = File.ReadAllBytes(_path);

            Encoding.ASCII.GetString(raw).Should().NotContain(minted);
        }

        /// <summary>
        /// A file that exists but will not decrypt reads as empty rather than throwing. The
        /// realistic cause is a credential file copied from another machine: the protection scope
        /// is LocalMachine, so DPAPI refuses it. The client must then say "not registered", which
        /// is true and actionable, instead of crashing.
        /// </summary>
        [SkippableFact]
        public void An_undecryptable_credential_file_reads_as_empty()
        {
            Skip.IfNot(OnWindows, SkipReason);

            Directory.CreateDirectory(_directory);
            File.WriteAllBytes(_path, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });

            new PCDataProtection().GetDeviceCredential(_path).Should().BeEmpty();
        }

        /// <summary>
        /// Re-registering a device replaces the credential rather than appending to the file. The
        /// old value must not survive: it is the credential a node operator has just revoked.
        /// </summary>
        [SkippableFact]
        public void Re_registering_replaces_the_previous_credential()
        {
            Skip.IfNot(OnWindows, SkipReason);

            PCDataProtection store = new PCDataProtection();
            string first = DeviceCredential.Generate();
            string second = DeviceCredential.Generate();

            store.SetDeviceCredential(first, _path);
            store.SetDeviceCredential(second, _path);

            store.GetDeviceCredential(_path).Should().Be(second);
        }

        /// <summary>
        /// The store creates its own directory. FileSystemInitalizer normally makes it at startup,
        /// but this is reachable before that on a first run, and the FileStream failure would
        /// otherwise be swallowed and reported as a successful save.
        /// </summary>
        [SkippableFact]
        public void A_missing_credential_directory_is_created_rather_than_failing_silently()
        {
            Skip.IfNot(OnWindows, SkipReason);

            Directory.Exists(_directory).Should().BeFalse("the fixture has not created it");

            string minted = DeviceCredential.Generate();

            new PCDataProtection().SetDeviceCredential(minted, _path).Should().BeTrue();
            new PCDataProtection().GetDeviceCredential(_path).Should().Be(minted);
        }
    }
}
