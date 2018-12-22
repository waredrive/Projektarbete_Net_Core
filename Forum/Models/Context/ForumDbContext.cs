using System;
using Forum.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Forum.Models.Context
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

        public virtual DbSet<AspNetRoleClaims> AspNetRoleClaims { get; set; }
        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaims> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogins> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUserRoles> AspNetUserRoles { get; set; }
        public virtual DbSet<AspNetUserTokens> AspNetUserTokens { get; set; }
        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }
        public virtual DbSet<Member> Member { get; set; }
        public virtual DbSet<Post> Post { get; set; }
        public virtual DbSet<Thread> Thread { get; set; }
        public virtual DbSet<Topic> Topic { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.0-rtm-35687");

            modelBuilder.Entity<AspNetRoleClaims>(entity =>
            {
                entity.HasIndex(e => e.RoleId);

                entity.Property(e => e.RoleId).IsRequired();

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AspNetRoleClaims)
                    .HasForeignKey(d => d.RoleId);
            });

            modelBuilder.Entity<AspNetRoles>(entity =>
            {
                entity.HasIndex(e => e.NormalizedName)
                    .HasName("RoleNameIndex")
                    .IsUnique()
                    .HasFilter("([NormalizedName] IS NOT NULL)");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name).HasMaxLength(256);

                entity.Property(e => e.NormalizedName).HasMaxLength(256);
            });

            modelBuilder.Entity<AspNetUserClaims>(entity =>
            {
                entity.HasIndex(e => e.UserId);

                entity.Property(e => e.UserId).IsRequired();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserClaims)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserLogins>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

                entity.HasIndex(e => e.UserId);

                entity.Property(e => e.UserId).IsRequired();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserLogins)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserRoles>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });

                entity.HasIndex(e => e.RoleId);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AspNetUserRoles)
                    .HasForeignKey(d => d.RoleId);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserRoles)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserTokens>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserTokens)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUsers>(entity =>
            {
                entity.HasIndex(e => e.NormalizedEmail)
                    .HasName("EmailIndex");

                entity.HasIndex(e => e.NormalizedUserName)
                    .HasName("UserNameIndex")
                    .IsUnique()
                    .HasFilter("([NormalizedUserName] IS NOT NULL)");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.NormalizedEmail).HasMaxLength(256);

                entity.Property(e => e.NormalizedUserName).HasMaxLength(256);

                entity.Property(e => e.UserName).HasMaxLength(256);
            });

            modelBuilder.Entity<Member>(entity =>
            {
                entity.HasIndex(e => e.BlockedBy);

                entity.Property(e => e.Id).ValueGeneratedNever();

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

                entity.HasOne(d => d.IdNavigation)
                    .WithOne(p => p.Member)
                    .HasForeignKey<Member>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Member__Id__7C4F7684");
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasIndex(e => e.CreatedBy);

                entity.HasIndex(e => e.EditedBy);

                entity.HasIndex(e => e.LockedBy);

                entity.HasIndex(e => e.Thread);

                entity.Property(e => e.CreatedBy).IsRequired();

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.PostCreatedByNavigation)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Post__CreatedBy__3B40CD36");

                entity.HasOne(d => d.EditedByNavigation)
                    .WithMany(p => p.PostEditedByNavigation)
                    .HasForeignKey(d => d.EditedBy)
                    .HasConstraintName("FK__Post__EditedBy__3C34F16F");

                entity.HasOne(d => d.LockedByNavigation)
                    .WithMany(p => p.PostLockedByNavigation)
                    .HasForeignKey(d => d.LockedBy)
                    .HasConstraintName("FK__Post__LockedBy__3D2915A8");

                entity.HasOne(d => d.ThreadNavigation)
                    .WithMany(p => p.Post)
                    .HasForeignKey(d => d.Thread)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Post__Thread__73BA3083");
            });

            modelBuilder.Entity<Thread>(entity =>
            {
                entity.HasIndex(e => e.CreatedBy);

                entity.HasIndex(e => e.EditedBy);

                entity.HasIndex(e => e.LockedBy);

                entity.HasIndex(e => e.Topic);

                entity.Property(e => e.ContentText).HasMaxLength(80);

                entity.Property(e => e.CreatedBy).IsRequired();

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.ThreadCreatedByNavigation)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Thread__CreatedB__3F115E1A");

                entity.HasOne(d => d.EditedByNavigation)
                    .WithMany(p => p.ThreadEditedByNavigation)
                    .HasForeignKey(d => d.EditedBy)
                    .HasConstraintName("FK__Thread__EditedBy__40058253");

                entity.HasOne(d => d.LockedByNavigation)
                    .WithMany(p => p.ThreadLockedByNavigation)
                    .HasForeignKey(d => d.LockedBy)
                    .HasConstraintName("FK__Thread__LockedBy__40F9A68C");

                entity.HasOne(d => d.TopicNavigation)
                    .WithMany(p => p.Thread)
                    .HasForeignKey(d => d.Topic)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Thread__Topic__66603565");
            });

            modelBuilder.Entity<Topic>(entity =>
            {
                entity.HasIndex(e => e.CreatedBy);

                entity.HasIndex(e => e.EditedBy);

                entity.HasIndex(e => e.LockedBy);

                entity.Property(e => e.ContentText).HasMaxLength(50);

                entity.Property(e => e.CreatedBy).IsRequired();

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.TopicCreatedByNavigation)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Topic__CreatedBy__42E1EEFE");

                entity.HasOne(d => d.EditedByNavigation)
                    .WithMany(p => p.TopicEditedByNavigation)
                    .HasForeignKey(d => d.EditedBy)
                    .HasConstraintName("FK__Topic__EditedBy__43D61337");

                entity.HasOne(d => d.LockedByNavigation)
                    .WithMany(p => p.TopicLockedByNavigation)
                    .HasForeignKey(d => d.LockedBy)
                    .HasConstraintName("FK__Topic__LockedBy__44CA3770");
            });
        }
    }
}
