// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Models;
using Febris.ModelLibrary.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.LookupModels
{
    class TestUserLinks
    {
    }
    public class TestUserLinkedUser:BaseModel
    {      

        public TestUser TestUser { get; set; }
        public Guid TestUserUUID { get; set; }
        public Guid UserId { get; set; }
    }
    public class TestUserLinkedFebris : BaseModel
    {      

        public TestUser TestUser { get; set; }
        public Guid TestUserUUID { get; set; }
        public Guid UserId { get; set; }
    }
    public class TestUserLinkedCurriculum : BaseModel
    {
      

        public TestUser TestUser { get; set; }
        public Guid TestUserUUID { get; set; }
        public Guid CurriculumUUID { get; set; }
        public Curriculum Curriculum { get; set; }
        public LocalPurchase LocalPurchase { get; set; }
        public Guid LocalPurchaseUUID { get; set; }
    }
    public class TestUserLinkedContentDeveloper : BaseModel
    {
      

        public TestUser TestUser { get; set; }
        public Guid TestUserUUID { get; set; }
        public ContentDeveloper ContentDeveloper { get; set; }
        public Guid ContentDeveloperUUID { get; set; }

        public List<TestUser> Select(Func<object, object> p)
        {
            throw new NotImplementedException();
        }
    }
    public class TestUserLinkedActor : BaseModel
    {
     

        public long ActorId { get; set; }
        public Guid ActorUUID { get; set; }
        public TestUser TestUser { get; set; }
        public Guid TestUserUUID { get; set; }
    }
    public class TestUserLinkedAccreditationBody : BaseModel
    {      
        public TestUser TestUser { get; set; }
        public Guid TestUserUUID { get; set; }
        public AccreditationBody AccreditationBody { get; set; }
        public Guid AccreditationBodyUUID { get; set; }
    }
}