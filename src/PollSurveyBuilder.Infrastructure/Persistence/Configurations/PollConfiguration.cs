using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PollSurveyBuilder.Domain.Entities;

namespace PollSurveyBuilder.Infrastructure.Persistence.Configurations
{
    public class PollConfiguration : IEntityTypeConfiguration<Poll>
    {
        public void Configure(EntityTypeBuilder<Poll> builder)
        {
            builder.ToTable("Polls");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Code)
                .IsRequired()
                .HasMaxLength(12);
            builder.HasIndex(p => p.Code).IsUnique();

            builder.Property(p => p.Question)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(p => p.Type).HasConversion<string>().HasMaxLength(20);
            builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);

            builder.HasMany(p => p.Options)
                .WithOne(o => o.Poll)
                .HasForeignKey(o => o.PollId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Votes)
                .WithOne(v => v.Poll)
                .HasForeignKey(v => v.PollId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
