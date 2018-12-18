using System;
using System.Collections.Generic;

namespace Forum.Models.Entities
{
    public partial class Topic
    {
        public Topic()
        {
            Thread = new HashSet<Thread>();
        }

        public int Id { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? EditedOn { get; set; }
        public int? EditedBy { get; set; }
        public DateTime? LockedOn { get; set; }
        public int? LockedBy { get; set; }
        public DateTime? RemovedOn { get; set; }
        public int? RemovedBy { get; set; }
        public string ContentText { get; set; }

        public virtual Member CreatedByNavigation { get; set; }
        public virtual Member EditedByNavigation { get; set; }
        public virtual Member LockedByNavigation { get; set; }
        public virtual Member RemovedByNavigation { get; set; }
        public virtual ICollection<Thread> Thread { get; set; }
    }
}
