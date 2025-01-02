using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace HeartSpace.Models.EFModel
{
	public partial class AppDbContext : DbContext
	{
		public AppDbContext()
			: base("name=AppDbContext")
		{
		}

		public virtual DbSet<Admin> Admins { get; set; }
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
			modelBuilder.Entity<Event>()
				.Property(e => e.Limit)
				.IsUnicode(false);

			modelBuilder.Entity<Member>()
				.Property(e => e.Email)
				.IsUnicode(false);

			modelBuilder.Entity<Member>()
				.Property(e => e.ConfirmCode)
				.IsUnicode(false);
		}
	}
}
