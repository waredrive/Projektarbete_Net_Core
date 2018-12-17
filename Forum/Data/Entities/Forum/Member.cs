using System;
using System.Collections.Generic;

namespace Forum.Data.Entities.Forum
{
    public partial class Member
    {
        public Member()
        {
            InverseBlockedByNavigation = new HashSet<Member>();
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
        }

        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? BlockedOn { get; set; }
        public string BlockedBy { get; set; }
        public DateTime? BlockedEnd { get; set; }

        public virtual Member BlockedByNavigation { get; set; }
        public virtual ICollection<Member> InverseBlockedByNavigation { get; set; }
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
    }
}
