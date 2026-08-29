// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.EnumLibrary
{
    class CRMEnums
    {
    }

    public enum LifecycleStage
    {
        None = 0,
        Visit = 100,
        Lead = 200,
        MarketingQualifiedLead =300,
        SalesQualifiedLead =400,
        Opportunity =500,
        Customer =600,
        Evangelist =700,
        Other =800,
    }
    public enum LeadType
    {
        Unknown = 0,        
        Educator = 100,
        [Display(Name = "Education Platform")] EducationPlatform = 125,
        [Display(Name = "HR or L&D")] HR = 150,
        Industrial = 200,
        Healthcare = 250,
        Developer = 300,
        [Display(Name = "XR Hardware Manufacturer")] XRHardwareManufacturer = 400
    }
    public enum ContactType
    {
        [Display(Name = "Unknown/None")] Unknown =0,        
        [Display(Name = "In Person")] InPerson = 100,
        [Display(Name = "Linkedin")] LinkedIn = 125,
        [Display(Name = "E-mail")] Email = 150,
        [Display(Name = "Phone Call")] PhoneCall = 200,
        [Display(Name = "Video Conference")] VideoConference = 300,
        [Display(Name = "In Person Demo")] InPersonDemo = 400,
        [Display(Name = "Virtual Demo")] VirtualDemo = 500,
        [Display(Name = "Negotiation")] Negotiation = 600
    }
    public enum CampaignType
    {
        Unknown =0,
        Newsletter = 50,
        Email = 100,
        TraditionalMedia = 200,
        ProductLaunch = 300,
        SeasonalPush = 400,
        BrandAwareness = 500,
        Rebranding = 600,
        BrandLaunch = 700,
        Expansion = 800,
        SearchEngine = 900,
        SocialMedia = 1000,
        PublicRelations = 1100,
        BetaAnnouncement = 1200,
        Referral = 1300        
    }

    public enum LeadFilter
    {
        [Display(Name = "Unclaimed Leads")] UnclaimedLeads = 100,
        [Display(Name = "Unranked")] Unranked =200,
        [Display(Name = "Uncontacted")] Uncontacted =300,
        [Display(Name = "Contact Overdue")] ContactOverdue = 325,
        [Display(Name = "Contact Needed")] ContactNeeded = 350,
        [Display(Name = "Contact Okay")] ContactOkay = 375,
        [Display(Name = "No Notes")] NoNotes = 400,
        [Display(Name = "No Phone Numbers")] NoPhoneNumbers = 500,
        [Display(Name = "No Email Address")] NoEmailAddress = 600,        
        [Display(Name = "Archived")] Archived = 700

    }
        
    public enum LeadInboxOptions
    {
        [Display(Name = "Connect")] Connect = 0,
        [Display(Name = "Ask A Question")] Question = 50,
        [Display(Name = "Periodic Updates")] Updates = 100,
        [Display(Name = "Pricing Information")] Pricing = 150,
        [Display(Name = "Get A Demo")] Demo = 200,        
        [Display(Name = "Febris For Your Company")] CompanyRegistration = 300,
        [Display(Name = "Develop Our Curriculum")] CustomContentDevelopment = 400,
        [Display(Name = "Content Developer Application")] ContentDeveloperApplication = 500,
        [Display(Name = "Accreditation Body Application")] AccreditationBodyApplication = 600//,        
    }

    public enum LeadSources
    {
        [Display(Name = "Search Engine")] SearchEngine = 100,
        [Display(Name = "Publications")] Publication = 200,
        [Display(Name = "Email Or Newsletter")] EmailOrNewsletter = 300,
        [Display(Name = "Social Media")] SocialMedia = 400,
        [Display(Name = "Word Of Mouth")] WordOfMouth = 500,
        [Display(Name = "YouTube or Similar")] YouTube = 600,
        [Display(Name = "Other")] Other= 700
    }

    // ---- LeadTask supporting enums (Phase 1 CRM, 2026-05-20) ----

    /// <summary>
    /// Priority bands surfaced in the rep's "Today / Overdue" view.
    /// Default is Medium so unflagged tasks don't get drowned out by
    /// urgent ones.
    /// </summary>
    public enum LeadTaskPriority
    {
        Low = 100,
        Medium = 200,
        High = 300,
        Urgent = 400
    }

    /// <summary>
    /// Open / Completed / Cancelled tristate. Cancelled is distinct from
    /// Completed -- a task that was abandoned without action shouldn't
    /// show up in the rep's "completed this week" report.
    /// </summary>
    public enum LeadTaskStatus
    {
        Open = 100,
        Completed = 200,
        Cancelled = 300
    }

    // ---- Email-tracking supporting enum (Phase 1 CRM, 2026-05-20) ----

    /// <summary>
    /// Kind of email-engagement event recorded by the pixel + redirect
    /// endpoints. Open and Click are wired today; Bounce and Unsubscribe
    /// are placeholders for the future inbound-webhook integration.
    /// </summary>
    public enum EmailEventType
    {
        Open = 100,
        Click = 200,
        Bounce = 300,
        Unsubscribe = 400
    }

    // ---- Opportunity / pipeline supporting enums (CRM Phase 2 Tier 1, 2026-05-21) ----

    /// <summary>
    /// CRM Phase 2 Tier 1 (2026-05-21): pipeline stage for an Opportunity.
    /// Ordered low-to-high to match deal progression. ClosedWon/Lost
    /// share the "Closed" suffix and the 900-series number so callers
    /// can detect terminal states with a single threshold check
    /// (<c>stage &gt;= ClosedWon</c>).
    /// <para>
    /// Pinned probability defaults (used when no per-Opportunity override
    /// is set) live in <c>OpportunityLogic.DefaultProbability(stage)</c>:
    /// Prospecting 10, Qualification 25, Discovery 40, Proposal 60,
    /// Negotiation 80, ClosedWon 100, ClosedLost 0.
    /// </para>
    /// </summary>
    public enum DealStage
    {
        [Display(Name = "Prospecting")] Prospecting = 100,
        [Display(Name = "Qualification")] Qualification = 200,
        [Display(Name = "Discovery")] Discovery = 300,
        [Display(Name = "Proposal")] Proposal = 400,
        [Display(Name = "Negotiation")] Negotiation = 500,
        [Display(Name = "Closed Won")] ClosedWon = 900,
        [Display(Name = "Closed Lost")] ClosedLost = 950
    }

    /// <summary>
    /// CRM Phase 2 Tier 1 (2026-05-21): why a deal was lost. Only
    /// meaningful when <see cref="DealStage.ClosedLost"/>; the
    /// "Loss-reason breakdown" report in Tier 2 pivots on this field
    /// to surface systemic objections.
    /// </summary>
    public enum OpportunityLossReason
    {
        [Display(Name = "Not Set")] NotSet = 0,
        [Display(Name = "No Budget")] NoBudget = 100,
        [Display(Name = "No Decision Maker")] NoDecisionMaker = 200,
        [Display(Name = "Lost To Competitor")] LostToCompetitor = 300,
        [Display(Name = "Bad Fit")] BadFit = 400,
        [Display(Name = "Timing")] Timing = 500,
        [Display(Name = "No Response")] NoResponse = 600,
        [Display(Name = "Other")] Other = 999
    }

}
