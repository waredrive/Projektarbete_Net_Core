using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Forum.Persistence.Entities.ForumData
{
    public partial class ForumDbContext : DbContext
    {
        public ForumDbContext()
        {
        }

        public ForumDbContext(DbContextOptions<ForumDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Account { get; set; }
        public virtual DbSet<Member> Member { get; set; }
        public virtual DbSet<Post> Post { get; set; }
        public virtual DbSet<Thread> Thread { get; set; }
        public virtual DbSet<Topic> Topic { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.0-rtm-35687");

            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasOne(d => d.BlockedByNavigation)
                    .WithMany(p => p.InverseBlockedByNavigation)
                    .HasForeignKey(d => d.BlockedBy)
                    .HasConstraintName("FK__Account__Blocked__01142BA1");
            });

            modelBuilder.Entity<Member>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.AccountNavigation)
                    .WithMany(p => p.Member)
                    .HasForeignKey(d => d.Account)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Member__Account__160F4887");
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.PostCreatedByNavigation)
                    .HasForeignKey(d => d.CreatedBy)
                    .HasConstraintName("FK__Post__CreatedBy__6FE99F9F");

                entity.HasOne(d => d.EditedByNavigation)
                    .WithMany(p => p.PostEditedByNavigation)
                    .HasForeignKey(d => d.EditedBy)
                    .HasConstraintName("FK__Post__EditedBy__70DDC3D8");

                entity.HasOne(d => d.LockedByNavigation)
                    .WithMany(p => p.PostLockedByNavigation)
                    .HasForeignKey(d => d.LockedBy)
                    .HasConstraintName("FK__Post__LockedBy__71D1E811");

                entity.HasOne(d => d.RemovedByNavigation)
                    .WithMany(p => p.PostRemovedByNavigation)
                    .HasForeignKey(d => d.RemovedBy)
                    .HasConstraintName("FK__Post__RemovedBy__72C60C4A");

                entity.HasOne(d => d.ThreadNavigation)
                    .WithMany(p => p.Post)
                    .HasForeignKey(d => d.Thread)
                    .HasConstraintName("FK__Post__Thread__73BA3083");
            });

            modelBuilder.Entity<Thread>(entity =>
            {
                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.ThreadCreatedByNavigation)
                    .HasForeignKey(d => d.CreatedBy)
                    .HasConstraintName("FK__Thread__CreatedB__5812160E");

                entity.HasOne(d => d.EditedByNavigation)
                    .WithMany(p => p.ThreadEditedByNavigation)
                    .HasForeignKey(d => d.EditedBy)
                    .HasConstraintName("FK__Thread__EditedBy__59063A47");

                entity.HasOne(d => d.LockedByNavigation)
                    .WithMany(p => p.ThreadLockedByNavigation)
                    .HasForeignKey(d => d.LockedBy)
                    .HasConstraintName("FK__Thread__LockedBy__59FA5E80");

                entity.HasOne(d => d.RemovedByNavigation)
                    .WithMany(p => p.ThreadRemovedByNavigation)
                    .HasForeignKey(d => d.RemovedBy)
                    .HasConstraintName("FK__Thread__RemovedB__5AEE82B9");

                entity.HasOne(d => d.TopicNavigation)
                    .WithMany(p => p.Thread)
                    .HasForeignKey(d => d.Topic)
                    .HasConstraintName("FK__Thread__Topic__66603565");
            });

            modelBuilder.Entity<Topic>(entity =>
            {
                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.TopicCreatedByNavigation)
                    .HasForeignKey(d => d.CreatedBy)
                    .HasConstraintName("FK__Topic__CreatedBy__5CD6CB2B");

                entity.HasOne(d => d.EditedByNavigation)
                    .WithMany(p => p.TopicEditedByNavigation)
                    .HasForeignKey(d => d.EditedBy)
                    .HasConstraintName("FK__Topic__EditedBy__5DCAEF64");

                entity.HasOne(d => d.LockedByNavigation)
                    .WithMany(p => p.TopicLockedByNavigation)
                    .HasForeignKey(d => d.LockedBy)
                    .HasConstraintName("FK__Topic__LockedBy__5EBF139D");

                entity.HasOne(d => d.RemovedByNavigation)
                    .WithMany(p => p.TopicRemovedByNavigation)
                    .HasForeignKey(d => d.RemovedBy)
                    .HasConstraintName("FK__Topic__RemovedBy__5FB337D6");
            });
        }
    }
}
