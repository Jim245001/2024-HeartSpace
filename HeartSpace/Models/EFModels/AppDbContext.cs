using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace HeartSpace.Models.EFModels
{
	public partial class AppDbContext : DbContext
	{
		public AppDbContext()
			: base("name=AppDbContext1")
		{
		}

		public virtual DbSet<Category> Categories { get; set; }
		public virtual DbSet<EventComment> EventComments { get; set; }
		public virtual DbSet<EventMember> EventMembers { get; set; }
		public virtual DbSet<Event> Events { get; set; }
		public virtual DbSet<Member> Members { get; set; }
		public virtual DbSet<PostComment> PostComments { get; set; }
		public virtual DbSet<Post> Posts { get; set; }
		public virtual DbSet<Tag> Tags { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Category>()
				.HasMany(e => e.Events)
				.WithRequired(e => e.Category)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<EventComment>()
				.Property(e => e.Disabled)
				.IsFixedLength();

			modelBuilder.Entity<Event>()
				.Property(e => e.Limit)
				.IsUnicode(false);

			modelBuilder.Entity<Event>()
				.HasMany(e => e.EventComments)
				.WithRequired(e => e.Event)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<Event>()
				.HasMany(e => e.EventMembers)
				.WithRequired(e => e.Event)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<Member>()
				.Property(e => e.Email)
				.IsUnicode(false);

			modelBuilder.Entity<Member>()
				.Property(e => e.ConfirmCode)
				.IsUnicode(false);

			modelBuilder.Entity<Member>()
				.HasMany(e => e.EventComments)
				.WithRequired(e => e.Member)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<Member>()
				.HasMany(e => e.EventMembers)
				.WithRequired(e => e.Member)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<Member>()
				.HasMany(e => e.PostComments)
				.WithRequired(e => e.Member)
				.HasForeignKey(e => e.UserId)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<Member>()
				.HasMany(e => e.Posts)
				.WithRequired(e => e.Member)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<Post>()
				.HasMany(e => e.PostComments)
				.WithRequired(e => e.Post)
				.WillCascadeOnDelete(false);
		}
	}
}
