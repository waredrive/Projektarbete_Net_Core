﻿using System;
using System.Collections.Generic;

namespace Forum.Models.Entities {
  public class Thread {
    public Thread() {
      Post = new HashSet<Post>();
    }

    public int Id { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? EditedOn { get; set; }
    public string EditedBy { get; set; }
    public DateTime? LockedOn { get; set; }
    public string LockedBy { get; set; }
    public string ContentText { get; set; }
    public int Topic { get; set; }

    public virtual Member CreatedByNavigation { get; set; }
    public virtual Member EditedByNavigation { get; set; }
    public virtual Member LockedByNavigation { get; set; }
    public virtual Topic TopicNavigation { get; set; }
    public virtual ICollection<Post> Post { get; set; }
  }
}