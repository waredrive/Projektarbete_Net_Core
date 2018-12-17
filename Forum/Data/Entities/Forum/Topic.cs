using System;
using System.Collections.Generic;

namespace Forum.Data.Entities.Forum
{
    public partial class Topic
    {
        public Topic()
        {
            Thread = new HashSet<Thread>();
        }

        public int Id { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? EditedOn { get; set; }
        public string EditedBy { get; set; }
        public DateTime? LockedOn { get; set; }
        public string LockedBy { get; set; }
        public DateTime? RemovedOn { get; set; }
        public string RemovedBy { get; set; }
        public string ContentText { get; set; }

        public virtual Member CreatedByNavigation { get; set; }
        public virtual Member EditedByNavigation { get; set; }
        public virtual Member LockedByNavigation { get; set; }
        public virtual Member RemovedByNavigation { get; set; }
        public virtual ICollection<Thread> Thread { get; set; }
    }
}
