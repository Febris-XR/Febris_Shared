// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Interfaces.XApiModelInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.Models.XApiModels.ModifiedForSharing
{    
    public class LocalStatement
    {
        //################################################################
        //this needs to be uuid (or guid) can be set up automatically with postgres       
        //################################################################
        public long Id { get; set; }
        public Guid UUID { get; set; }

        //################################################################
        //if not provided needs to set by api
        //################################################################
        public DateTime Timestamp { get; set; }

        //################################################################
        //Set this inside Db for when the record is stored
        //################################################################
        public DateTime Stored { get; set; }

        //################################################################
        //xApi required fields
        //################################################################        
        [Required]
        public Actor Actor { get; set; }
        [Required]
        public long VerbId { get; set; }
        [Required]
        public Guid VerbUUID { get; set; }
        [Required]
        public long ObjectId { get; set; }
        [Required]
        public Guid ObjectUUID { get; set; }

        //################################################################
        //Optional Fields
        //attachments needs to be an ordered array of objects
        //################################################################
        public Result Result { get; set; }
        public Context Context { get; set; }
        public Authority Authority { get; set; }
        public Guid VersionUUID { get; set; }
        public long VersionId { get; set; }
        public List<Attachment> Attachments { get; set; }

        //################################################################
        // Voiding (T5)
        //################################################################
        // xAPI 1.0.3: a statement is retracted by issuing a NEW statement whose verb is
        // http://adlnet.gov/expapi/verbs/voided and whose object references the target. The target
        // is NEVER deleted and NEVER altered -- it stays exactly as the producer sent it, and the
        // LRS simply stops counting it.
        //
        // The 2021 implementation (retired SVN, Febris.Portal xAPIController.VoidStatementConfirmed)
        // did the opposite: it OVERWROTE the target's verb with `voided`, destroying the record of
        // what the learner actually did, and wrote the voiding statement to a JSON file rather than
        // the table so nothing could query it. This build keeps the target intact.
        //
        // VoidedAt is a DENORMALISED marker, not the record of the void -- the record is the voiding
        // statement. It exists because voided-ness is otherwise only derivable by joining every read
        // against "does a voiding statement point at me", across 28 read paths. It backs the global
        // query filter that makes the exclusion impossible to forget.

        /// <summary>
        /// When this statement was voided, or null if it stands. Never un-set: voiding is one-way,
        /// per the spec and the owner's ruling. A mistaken void is corrected by issuing a new
        /// statement, not by reversing the old one.
        /// </summary>
        public DateTime? VoidedAt { get; set; }

        /// <summary>
        /// The Identity user who voided it. The voiding STATEMENT carries the xAPI actor; this
        /// carries the operator, so the action stays attributable even when the voiding admin has
        /// no xAPI actor of their own.
        /// </summary>
        public Guid? VoidedByUserId { get; set; }

        //################################################################
        // Submitter attribution (T2)
        //################################################################
        // The node takes a statement's ACTOR from the request body, which is correct: a shared
        // classroom device legitimately submits for many learners in sequence, so the device is not
        // the learner. What was missing was any record of WHO submitted it. Reads are scoped per
        // actor while writes are not, so a forged record was indistinguishable from a real one
        // afterwards and there was nothing to investigate with.
        //
        // Deliberately SEPARATE from the xAPI Authority above, which is now stamped from the same
        // credential. Authority is part of the exported statement and may later be reshaped or
        // filled from a different credential type. This column is the node's own trail, never
        // leaves the database, and answers the incident question directly: show me everything this
        // device ever submitted.
        //
        // ATTRIBUTION, NOT CONSTRAINT. Nothing rejects a statement based on this value. The owner
        // ruling of 2026-08-10 rejected binding writes through HardwareLinkedCohort, which would
        // tie a compliance record to mutable membership state, and that objection stands. Recording
        // who submitted something is a different act from refusing it.

        /// <summary>
        /// The device that submitted this statement, or null when it did not arrive over a
        /// device-authenticated route (Portal-originated, a seed, or an import). Never used to
        /// accept or reject a statement.
        /// </summary>
        public Guid? SubmittedByHardwareUUID { get; set; }
    }
}
