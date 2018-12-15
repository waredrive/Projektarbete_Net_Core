using System;
using System.Collections.Generic;

namespace Forum.Persistence.Entities
{
    public partial class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime BirthDate { get; set; }
        public int Account { get; set; }

        public virtual Account AccountNavigation { get; set; }
    }
}
