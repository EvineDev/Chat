using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Chat.Service;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Chat.Db
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> contextOptions) : base(contextOptions)
        {

        }

        public DbSet<UserDb> Users { get; set; }

        public DbSet<SessionDb> Sessions { get; set; }

        public DbSet<MessageDb> Messages { get; set; }

		public DbSet<BinaryDb> Binary { get; set; }

		public DbSet<LogoutTokenDb> LogoutToken { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
			//modelBuilder.Entity<MessageDb>()
			//	.HasOne(x => x.Session)
			//	.WithMany(x => x.Messages)
			//	.HasForeignKey(x => x.Session)
			//	.OnDelete(DeleteBehavior.SetNull);

            //modelBuilder.Entity<UserDb>()
            //    .HasAlternateKey(x => x.UserId);
        }
    }
}
