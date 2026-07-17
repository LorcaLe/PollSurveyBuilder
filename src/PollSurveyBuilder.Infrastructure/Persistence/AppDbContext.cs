using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PollSurveyBuilder.Domain.Entities;
using PollSurveyBuilder.Domain.Entities.Identity;
using PollSurveyBuilder.Infrastructure.Persistence.Configurations;

namespace PollSurveyBuilder.Infrastructure.Persistence
{
    /// <summary>
    /// Single DbContext for the whole service: ASP.NET Identity tables (AspNetUsers, etc.)
    /// plus the Poll/PollOption/Vote domain tables. Kept in one context (rather than the
    /// two-context split some Clean Architecture samples use) because every write here
    /// is small and transactional - one DbContext keeps SaveChangesAsync atomic.
    /// </summary>
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Poll> Polls { get; set; } = default!;
        public DbSet<PollOption> PollOptions { get; set; } = default!;
        public DbSet<Vote> Votes { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new PollConfiguration());
            modelBuilder.ApplyConfiguration(new PollOptionConfiguration());
            modelBuilder.ApplyConfiguration(new VoteConfiguration());
        }
    }
}
