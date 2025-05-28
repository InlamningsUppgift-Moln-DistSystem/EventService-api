using Microsoft.EntityFrameworkCore;
using EventMicroService.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace EventMicroService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Event> Events { get; set; }
        public DbSet<Attendee> Attendees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>()
                .HasMany(e => e.Attendees)
                .WithOne(a => a.Event)
                .HasForeignKey(a => a.EventId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
