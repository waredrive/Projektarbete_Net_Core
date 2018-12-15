using System;
using System.Collections.Generic;

namespace Forum.Persistence.Entities
{
    public partial class Post
    {
        public int Id { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? EditedOn { get; set; }
        public int? EditedBy { get; set; }
        public DateTime? LockedOn { get; set; }
        public int? LockedBy { get; set; }
        public DateTime? RemovedOn { get; set; }
        public int? RemovedBy { get; set; }
        public int? Thread { get; set; }
        public string ContentText { get; set; }

        public virtual Account CreatedByNavigation { get; set; }
        public virtual Account EditedByNavigation { get; set; }
        public virtual Account LockedByNavigation { get; set; }
        public virtual Account RemovedByNavigation { get; set; }
        public virtual Thread ThreadNavigation { get; set; }
    }
}
