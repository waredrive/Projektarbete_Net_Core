using System;
using System.Collections.Generic;

namespace Forum.Persistence.Entities.ForumData
{
    public partial class Member
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public int Account { get; set; }

        public virtual Account AccountNavigation { get; set; }
    }
}
