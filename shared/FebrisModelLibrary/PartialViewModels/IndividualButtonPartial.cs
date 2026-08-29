// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.PartialViewModels
{
    public class IndividualButtonPartial
    {
        public string Action { get; set; }
        public string Glyph { get; set; }
        public string ButtonType { get; set; }
        public string Tooltip { get; set; }
        //public string ButtonAction { get; set; }
        //public long? LinkBase { get; set; }//may not be needed
        //public long? LinkBase2 { get; set; }//may not be needed
        //public Guid UserId { get; set; }



        public long? Id { get; set; }
        public string ActionParameters
        {
            get
            {
                if (Id != 0 && Id != null)
                {
                    return Id.ToString();
                }
                return null;
            }
        }
    }
    public class IndividualUUIDButtonPartial
    {
        public string Action { get; set; }
        public string Glyph { get; set; }
        public string ButtonType { get; set; }
        public string Tooltip { get; set; }        
        //public Guid UserId { get; set; }
        public Guid? Id { get; set; }
        public string ActionParameters
        {
            get
            {
                if (Id != Guid.Empty && Id != null)
                {
                    return Id.ToString();
                }
                return null;
            }
        }
    }
    public class IndividualSubsetButtonPartial
    {
        public string Action { get; set; }
        public string Glyph { get; set; }
        public string ButtonType { get; set; }
        public string Tooltip { get; set; }        
        public Guid Variable { get; set; }
        
        public long? Id { get; set; }
        public string ActionParameters
        {
            get
            {
                if (Id != 0 && Id != null)
                {
                    return Id.ToString();
                }
                return null;
            }
        }
    }

    public class StarButtonPartial
    {
        public string Action { get; set; }
        public string Glyph { get; set; }
        public string ButtonType { get; set; }
        public string Tooltip { get; set; }
        [Range(0,5)]
        public int Rating { get; set; }
        public long? Id { get; set; }
        public string ActionParameters
        {
            get
            {
                if (Id != 0 && Id != null)
                {
                    return Id.ToString();
                }
                return null;
            }
        }
    }
}
