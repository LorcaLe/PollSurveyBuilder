using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PollSurveyBuilder.Domain.Entities;

namespace PollSurveyBuilder.Infrastructure.Persistence.Configurations
{
    public class VoteConfiguration : IEntityTypeConfiguration<Vote>
    {
        public void Configure(EntityTypeBuilder<Vote> builder)
        {
            builder.ToTable("Votes");
            builder.HasKey(v => v.Id);

            builder.Property(v => v.VoterToken)
                .IsRequired()
                .HasMaxLength(64);

            builder.Property(v => v.TextAnswer)
                .HasMaxLength(1000);

            // One vote per browser per poll - enforced at the database level, not just in code.
            builder.HasIndex(v => new { v.PollId, v.VoterToken }).IsUnique();

            builder.HasOne(v => v.Option)
                .WithMany(o => o.Votes)
                .HasForeignKey(v => v.OptionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
