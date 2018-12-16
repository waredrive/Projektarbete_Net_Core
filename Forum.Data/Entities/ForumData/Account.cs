using System;
using System.Collections.Generic;

namespace Forum.Persistence.Entities.ForumData
{
    public partial class Account
    {
        public Account()
        {
            InverseBlockedByNavigation = new HashSet<Account>();
            PostCreatedByNavigation = new HashSet<Post>();
            PostEditedByNavigation = new HashSet<Post>();
            PostLockedByNavigation = new HashSet<Post>();
            PostRemovedByNavigation = new HashSet<Post>();
            ThreadCreatedByNavigation = new HashSet<Thread>();
            ThreadEditedByNavigation = new HashSet<Thread>();
            ThreadLockedByNavigation = new HashSet<Thread>();
            ThreadRemovedByNavigation = new HashSet<Thread>();
            TopicCreatedByNavigation = new HashSet<Topic>();
            TopicEditedByNavigation = new HashSet<Topic>();
            TopicLockedByNavigation = new HashSet<Topic>();
            TopicRemovedByNavigation = new HashSet<Topic>();
            User = new HashSet<User>();
        }

        public int Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime? BlockedOn { get; set; }
        public int? BlockedBy { get; set; }
        public DateTime? BlockeEnd { get; set; }
        public int? Role { get; set; }

        public virtual Account BlockedByNavigation { get; set; }
        public virtual ICollection<Account> InverseBlockedByNavigation { get; set; }
        public virtual ICollection<Post> PostCreatedByNavigation { get; set; }
        public virtual ICollection<Post> PostEditedByNavigation { get; set; }
        public virtual ICollection<Post> PostLockedByNavigation { get; set; }
        public virtual ICollection<Post> PostRemovedByNavigation { get; set; }
        public virtual ICollection<Thread> ThreadCreatedByNavigation { get; set; }
        public virtual ICollection<Thread> ThreadEditedByNavigation { get; set; }
        public virtual ICollection<Thread> ThreadLockedByNavigation { get; set; }
        public virtual ICollection<Thread> ThreadRemovedByNavigation { get; set; }
        public virtual ICollection<Topic> TopicCreatedByNavigation { get; set; }
        public virtual ICollection<Topic> TopicEditedByNavigation { get; set; }
        public virtual ICollection<Topic> TopicLockedByNavigation { get; set; }
        public virtual ICollection<Topic> TopicRemovedByNavigation { get; set; }
        public virtual ICollection<User> User { get; set; }
    }
}
