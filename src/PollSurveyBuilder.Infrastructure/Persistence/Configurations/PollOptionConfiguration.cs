using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PollSurveyBuilder.Domain.Entities;

namespace PollSurveyBuilder.Infrastructure.Persistence.Configurations
{
    public class PollOptionConfiguration : IEntityTypeConfiguration<PollOption>
    {
        public void Configure(EntityTypeBuilder<PollOption> builder)
        {
            builder.ToTable("PollOptions");
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Text)
                .IsRequired()
                .HasMaxLength(120);
        }
    }
}
