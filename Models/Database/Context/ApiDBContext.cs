using System;
using System.Collections.Generic;
using API.Models.Database.Identities;
using Microsoft.EntityFrameworkCore;

namespace API.Models.Database.Context;

public partial class ApiDBContext : DbContext
{
    public ApiDBContext()
    {
    }

    public ApiDBContext(DbContextOptions<ApiDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Gallery> Galleries { get; set; }

    public virtual DbSet<LearningProgress> LearningProgresses { get; set; }

    public virtual DbSet<MediaItem> MediaItems { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<Topic> Topics { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vocabulary> Vocabularies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A0BCFA7E73D");

            entity.HasIndex(e => e.TreePath, "IX_Categories_TreePath");

            entity.HasIndex(e => e.Slug, "UQ__Categori__BC7B5FB620D6BB5C").IsUnique();

            entity.Property(e => e.IsVisible).HasDefaultValueSql("((1))");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Slug)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SortOrder).HasDefaultValueSql("((0))");
            entity.Property(e => e.TreePath)
                .HasMaxLength(500)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__Comments__C3B4DFCA626BAFE3");

            entity.Property(e => e.Content).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsApproved).HasDefaultValueSql("((1))");
        });

        modelBuilder.Entity<Gallery>(entity =>
        {
            entity.HasKey(e => e.GalleryId).HasName("PK__Gallerie__CF4F7BB53DFFA19B");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(255);
        });

        modelBuilder.Entity<LearningProgress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Learning__3214EC07B18D2BD6");

            entity.ToTable("LearningProgress");

            entity.HasIndex(e => new { e.UserId, e.NextReviewDate }, "IX_User_ReviewDate");

            entity.Property(e => e.Box).HasDefaultValueSql("((0))");
            entity.Property(e => e.EaseFactor).HasDefaultValueSql("((2.5))");
            entity.Property(e => e.IntervalDays).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsMastered).HasDefaultValueSql("((0))");
            entity.Property(e => e.LastReviewDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Repetitions).HasDefaultValueSql("((0))");
        });

        modelBuilder.Entity<MediaItem>(entity =>
        {
            entity.HasKey(e => e.MediaId).HasName("PK__MediaIte__B2C2B5CFE30FE7E9");

            entity.Property(e => e.Caption).HasMaxLength(255);
            entity.Property(e => e.SortOrder).HasDefaultValueSql("((0))");
            entity.Property(e => e.Url).HasMaxLength(500);
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__Posts__AA12601823F86ABD");

            entity.HasIndex(e => e.Slug, "UQ__Posts__BC7B5FB6D23A6A6E").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsPublished).HasDefaultValueSql("((0))");
            entity.Property(e => e.PublishedAt).HasColumnType("datetime");
            entity.Property(e => e.Slug)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Summary).HasMaxLength(500);
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ViewCount).HasDefaultValueSql("((0))");
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Topics__3214EC07AB1A14B6");

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C1B42A3B6");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E450CB8350").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105342C11C4D9").IsUnique();

            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValueSql("('User')");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Vocabulary>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Vocabula__3214EC0702B116C2");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Meaning).HasMaxLength(255);
            entity.Property(e => e.Phonetic).HasMaxLength(100);
            entity.Property(e => e.Word).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
