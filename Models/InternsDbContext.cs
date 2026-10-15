using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GameLauncher.Models;

public partial class InternsDbContext : DbContext
{
    public InternsDbContext()
    {
    }

    public InternsDbContext(DbContextOptions<InternsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CommentLikesJdr> CommentLikesJdrs { get; set; }

    public virtual DbSet<GamesJdr> GamesJdrs { get; set; }

    public virtual DbSet<PlayerCommentsJdr> PlayerCommentsJdrs { get; set; }

    public virtual DbSet<UsersJdr> UsersJdrs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CommentLikesJdr>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CommentL__3214EC072CE7A698");

            entity.ToTable("CommentLikes_JDR");

            entity.HasIndex(e => new { e.CommentId, e.UserId }, "UQ_CommentLike").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Comment).WithMany(p => p.CommentLikesJdrs)
                .HasForeignKey(d => d.CommentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CommentLi__Comme__4EE01394");

            entity.HasOne(d => d.User).WithMany(p => p.CommentLikesJdrs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CommentLi__UserI__4FD437CD");
        });

        modelBuilder.Entity<GamesJdr>(entity =>
        {
            entity.HasKey(e => e.GameId).HasName("PK__Games_JD__2AB897FD16F6AF01");

            entity.ToTable("Games_JDR");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.GameName).HasMaxLength(100);
            entity.Property(e => e.GameNameEs).HasMaxLength(200);
            entity.Property(e => e.IconUrl).HasMaxLength(200);
            entity.Property(e => e.ImageUrl).HasMaxLength(200);
            entity.Property(e => e.Status).HasDefaultValue(true);
        });

        modelBuilder.Entity<PlayerCommentsJdr>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PlayerCo__3214EC0755D07D26");

            entity.ToTable("PlayerComments_JDR");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK__PlayerCom__Paren__4A1B5E77");

            entity.HasOne(d => d.User).WithMany(p => p.PlayerCommentsJdrs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PlayerCom__UserI__49273A3E");
        });

        modelBuilder.Entity<UsersJdr>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users_JD__1788CCAC8904CDDA");

            entity.ToTable("Users_JDR");

            entity.HasIndex(e => e.Username, "UQ__Users_JD__536C85E44B64401E").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Adp)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ADP");
            entity.Property(e => e.DomainId).HasColumnName("DomainID");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
