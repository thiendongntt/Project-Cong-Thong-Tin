using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace CongThongTin_UTC2.Models
{
    public partial class DBCongThongTin : DbContext
    {
        public DBCongThongTin()
            : base("name=DBCongThongTin")
        {
        }

        public virtual DbSet<C__MigrationHistory> C__MigrationHistory { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<WebInfo> WebInfo { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<Series> Series { get; set; }
        public virtual DbSet<StickyPost> StickyPosts { get; set; }
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Post>()
                .HasMany(e => e.Tags)
                .WithMany(e => e.Posts)
                .Map(m => m.ToTable("Tbl_PostTags").MapLeftKey("PostID").MapRightKey("TagID"));

            modelBuilder.Entity<Post>()
                .HasMany(e => e.Series)
                .WithMany(e => e.Posts)
                .Map(m => m.ToTable("Tbl_SeriesPost").MapLeftKey("PostID").MapRightKey("seriesID"));

            modelBuilder.Entity<User>()
                .Property(e => e.username)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.password)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.userrole)
                .IsUnicode(false);
        }
    }
}
