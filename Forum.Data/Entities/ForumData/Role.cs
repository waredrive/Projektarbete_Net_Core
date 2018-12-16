using System;
using System.Collections.Generic;

namespace Forum.Persistence.Entities.ForumData
{
    public partial class Role
    {
        public Role()
        {
            Account = new HashSet<Account>();
        }

        public int Id { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Account> Account { get; set; }
    }
}
