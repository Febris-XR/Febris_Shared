// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.Models.MarketingModels
{
    public class LeadRating : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime TimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        [Range(0,5)]
        public int Rating { get; set; }

        // CRM Phase 2 Tier 2.2 (task #91): lead scoring extension.
        // AutoComputed=true marks the rating as system-generated via
        // LeadScoringLogic.ComputeScore + ScoreToStars; admins can
        // override (which clears the flag). ComputedAt timestamps the
        // last automatic recompute for staleness checks.
        public bool AutoComputed { get; set; }
        public DateTime? ComputedAt { get; set; }
    }
}
