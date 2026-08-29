// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;

namespace Febris.ModelLibrary.Models.DataModels
{
    // Ownership record for a session video, so a recording can be served only to the learner it
    // belongs to or to staff.
    //
    // WHY THIS EXISTS. The recording filename is a bare Guid minted in
    // LauncherLogic.VideoAttachmentHandler and handed to the client inside the statement's xAPI
    // attachment. Nothing recorded which learner it belonged to: no table, no sidecar, nothing
    // encoded in the name, and the upload itself carries only a device token. So the Portal's
    // video loaders could not check anything, and served any recording to any signed-in end user
    // who knew the Guid. The Guid being unguessable was the only protection, which is secrecy of
    // the identifier rather than access control.
    //
    // The xAPI attachment looks like it should carry this, and in principle Attachment -> Statement
    // -> Actor is a join. It is not usable: the statement that mints the Guid is never persisted,
    // IAttachmentQueries is an empty interface with no read side, and on the PC fallback path the
    // client mints its own Guid the node has never seen. Recording ownership at mint time is the
    // smallest change that makes the question answerable.
    //
    // ActorUUID is denormalized from the Actor resolved one frame above the mint
    // (LauncherLogic.InitalizeStatement), matching how ParentLinkedStudent denormalizes
    // StudentActorId so access scoping resolves without a second lookup.
    //
    // BaseModel supplies Id, UUID, TimeStamp and LastUpdateTimeStamp, so TimeStamp dates the
    // recording without a bespoke column.
    public class Recording : BaseModel
    {
        /// <summary>
        /// The on-disk filename stem, which is the Guid string minted for the xAPI attachment and
        /// echoed back by the client as the upload's part-file base name. Stored as a string rather
        /// than a Guid because that is exactly what arrives on the query string, and a value that
        /// does not parse must be a lookup miss rather than a throw.
        /// </summary>
        public string Name { get; set; }

        /// <summary>The actor whose session this recording captures.</summary>
        public Guid ActorUUID { get; set; }

        /// <summary>
        /// The device that minted this recording name, used to bind the UPLOAD.
        /// <para>
        /// Without it, one authenticated device could upload parts under another device's recording
        /// name and overwrite that learner's session, because <c>SplitVideos/</c> and
        /// <c>recordings/</c> are one flat namespace and nothing correlated a part filename with
        /// the device sending it. <c>VideoUploadLogic</c> materialises the authenticated device on
        /// every request and then discards it.
        /// </para>
        /// <para>
        /// Safe to compare directly: the value in <c>HttpContext.Items["Hardware"]</c> is a
        /// field-for-field projection of the node's own <c>LocalHardware</c> row
        /// (<c>HardwareKeyAuthorization.ToHardwareClaim</c> copies <c>UUID</c> verbatim), so mint
        /// time and upload time are the same value in the same identity space. The CLR type is the
        /// central-shaped <c>Hardware</c> model, but that is type reuse, not shared identity with
        /// central's separate store.
        /// </para>
        /// </summary>
        public Guid HardwareUUID { get; set; }
    }
}
