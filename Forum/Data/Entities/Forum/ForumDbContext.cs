using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Forum.Data.Entities.Forum
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

        public virtual DbSet<Member> Member { get; set; }
        public virtual DbSet<Post> Post { get; set; }
        public virtual DbSet<Thread> Thread { get; set; }
        public virtual DbSet<Topic> Topic { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.0-rtm-35687");

            modelBuilder.Entity<Member>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.BlockedBy).HasMaxLength(450);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.BlockedByNavigation)
                    .WithMany(p => p.InverseBlockedByNavigation)
                    .HasForeignKey(d => d.BlockedBy)
                    .HasConstraintName("FK__Member__BlockedB__46B27FE2");
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.Property(e => e.CreatedBy).HasMaxLength(450);

                entity.Property(e => e.EditedBy).HasMaxLength(450);

                entity.Property(e => e.LockedBy).HasMaxLength(450);

                entity.Property(e => e.RemovedBy).HasMaxLength(450);

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.PostCreatedByNavigation)
                    .HasForeignKey(d => d.CreatedBy)
                    .HasConstraintName("FK__Post__CreatedBy__3B40CD36");

                entity.HasOne(d => d.EditedByNavigation)
                    .WithMany(p => p.PostEditedByNavigation)
                    .HasForeignKey(d => d.EditedBy)
                    .HasConstraintName("FK__Post__EditedBy__3C34F16F");

                entity.HasOne(d => d.LockedByNavigation)
                    .WithMany(p => p.PostLockedByNavigation)
                    .HasForeignKey(d => d.LockedBy)
                    .HasConstraintName("FK__Post__LockedBy__3D2915A8");

                entity.HasOne(d => d.RemovedByNavigation)
                    .WithMany(p => p.PostRemovedByNavigation)
                    .HasForeignKey(d => d.RemovedBy)
                    .HasConstraintName("FK__Post__RemovedBy__3E1D39E1");

                entity.HasOne(d => d.ThreadNavigation)
                    .WithMany(p => p.Post)
                    .HasForeignKey(d => d.Thread)
                    .HasConstraintName("FK__Post__Thread__73BA3083");
            });

            modelBuilder.Entity<Thread>(entity =>
            {
                entity.Property(e => e.CreatedBy).HasMaxLength(450);

                entity.Property(e => e.EditedBy).HasMaxLength(450);

                entity.Property(e => e.LockedBy).HasMaxLength(450);

                entity.Property(e => e.RemovedBy).HasMaxLength(450);

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.ThreadCreatedByNavigation)
                    .HasForeignKey(d => d.CreatedBy)
                    .HasConstraintName("FK__Thread__CreatedB__3F115E1A");

                entity.HasOne(d => d.EditedByNavigation)
                    .WithMany(p => p.ThreadEditedByNavigation)
                    .HasForeignKey(d => d.EditedBy)
                    .HasConstraintName("FK__Thread__EditedBy__40058253");

                entity.HasOne(d => d.LockedByNavigation)
                    .WithMany(p => p.ThreadLockedByNavigation)
                    .HasForeignKey(d => d.LockedBy)
                    .HasConstraintName("FK__Thread__LockedBy__40F9A68C");

                entity.HasOne(d => d.RemovedByNavigation)
                    .WithMany(p => p.ThreadRemovedByNavigation)
                    .HasForeignKey(d => d.RemovedBy)
                    .HasConstraintName("FK__Thread__RemovedB__41EDCAC5");

                entity.HasOne(d => d.TopicNavigation)
                    .WithMany(p => p.Thread)
                    .HasForeignKey(d => d.Topic)
                    .HasConstraintName("FK__Thread__Topic__66603565");
            });

            modelBuilder.Entity<Topic>(entity =>
            {
                entity.Property(e => e.CreatedBy).HasMaxLength(450);

                entity.Property(e => e.EditedBy).HasMaxLength(450);

                entity.Property(e => e.LockedBy).HasMaxLength(450);

                entity.Property(e => e.RemovedBy).HasMaxLength(450);

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.TopicCreatedByNavigation)
                    .HasForeignKey(d => d.CreatedBy)
                    .HasConstraintName("FK__Topic__CreatedBy__42E1EEFE");

                entity.HasOne(d => d.EditedByNavigation)
                    .WithMany(p => p.TopicEditedByNavigation)
                    .HasForeignKey(d => d.EditedBy)
                    .HasConstraintName("FK__Topic__EditedBy__43D61337");

                entity.HasOne(d => d.LockedByNavigation)
                    .WithMany(p => p.TopicLockedByNavigation)
                    .HasForeignKey(d => d.LockedBy)
                    .HasConstraintName("FK__Topic__LockedBy__44CA3770");

                entity.HasOne(d => d.RemovedByNavigation)
                    .WithMany(p => p.TopicRemovedByNavigation)
                    .HasForeignKey(d => d.RemovedBy)
                    .HasConstraintName("FK__Topic__RemovedBy__45BE5BA9");
            });
        }
    }
}
