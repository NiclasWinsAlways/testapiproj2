using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestRepo.DTO;

namespace TestRepo.Data
{
    public class Dbcontext : DbContext // EF CORE
    {
        // a class has 2 things (methods and properties)
        public Dbcontext(DbContextOptions<Dbcontext> options) : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Account> Acc { get; set; } // Ensure this matches what you use in your controller

        public DbSet<Volume> Vols { get; set; }
        public DbSet<BookProgress> BooksProgress { get; set; }
        public DbSet<VolProgress> volProgress { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Account>().ToTable("Accounts");
            modelBuilder.Entity<BookProgress>()
            .HasOne(p => p.Book)
            .WithMany()
            .HasForeignKey(p => p.BookId)
            .IsRequired(false);  // Make sure this is set correctly based on your requirements

            modelBuilder.Entity<BookProgress>()
                .HasOne(p => p.Account)
                .WithMany()
                .HasForeignKey(p => p.AccountId)
                .IsRequired(false);  // Adjust based on your model's requirements
 

        // Assuming Volume is the primary entity that, when deleted, should cascade delete VolProgress
        modelBuilder.Entity<VolProgress>()
                .HasOne(vp => vp.Volume)
                .WithMany()
                .HasForeignKey(vp => vp.volId)
                .OnDelete(DeleteBehavior.Cascade);  // Allow cascade delete from Volume to VolProgress

            // Prevent cascade delete from Account and Book to VolProgress to avoid multiple paths
            modelBuilder.Entity<VolProgress>()
                .HasOne(vp => vp.Account)
                .WithMany()
                .HasForeignKey(vp => vp.AccountId)
                .OnDelete(DeleteBehavior.Restrict); // Change to Restrict to clearly disallow cascade

            modelBuilder.Entity<VolProgress>()
                .HasOne(vp => vp.book)
                .WithMany()
                .HasForeignKey(vp => vp.BookId)
                .OnDelete(DeleteBehavior.Restrict); // Change to Restrict here as well
        }




    }
}