// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;

namespace Febris.ModelLibrary.ViewModels.XApi
{
    /// <summary>
    /// Composite type produced by the raw-bytes capturing model binder for
    /// xAPI statement ingest. Bundles the typed (parsed) statement DTO
    /// with the verbatim request body bytes so BOTH forms reach the BLL
    /// and persistence layer.
    /// <para>
    /// The goal is audit-grade preservation of what the producer actually
    /// sent (PC launcher, mobile companion, simulation plugin, third-party
    /// xAPI client) -- not a regeneration of our typed view of it.
    /// </para>
    /// <para>
    /// Encoding: <see cref="RawBody"/> holds the body bytes verbatim as
    /// received off the wire. <see cref="RawBodyEncoding"/> records the
    /// charset the binder used to interpret them as text for the DTO
    /// parse step. Producers SHOULD send UTF-8 (RFC 8259); the binder
    /// honours an explicit <c>charset=</c> parameter on the
    /// <c>Content-Type</c> header but defaults to UTF-8 otherwise.
    /// </para>
    /// </summary>
    public class XApiStatementSubmission
    {
        /// <summary>
        /// The typed-bound statement. Null if the body was unparseable
        /// as JSON (in which case the controller should reject the
        /// request -- the binder doesn't 400 on its own, it leaves the
        /// rejection policy to the action).
        /// </summary>
        public XApiStatementDto Dto { get; set; }

        /// <summary>
        /// Verbatim request body. The bytes the producer actually sent.
        /// Stored alongside the persisted Statement so future audit /
        /// replay / spec-debate has the canonical form available.
        /// </summary>
        public byte[] RawBody { get; set; }

        /// <summary>
        /// Charset used to interpret RawBody as text for the typed
        /// parse step. Defaults to "utf-8". Preserved here so the
        /// persistence layer can write the JSON form back with the same
        /// encoding the producer used.
        /// </summary>
        public string RawBodyEncoding { get; set; }

        /// <summary>
        /// Content-Type header value from the request, including any
        /// charset / boundary parameters. xAPI defines a few accepted
        /// Content-Types (application/json, multipart/mixed for
        /// statements with attachments). Persistence may want this
        /// alongside the bytes.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// True iff the DTO bound successfully. False means RawBody is
        /// present (we captured the bytes) but JSON parse failed --
        /// useful for debugging "what did the client actually send?"
        /// </summary>
        public bool DtoBound => Dto != null;

        /// <summary>
        /// If <see cref="DtoBound"/> is false, the message from the
        /// parse exception. Otherwise null.
        /// </summary>
        public string ParseError { get; set; }
    }
}
