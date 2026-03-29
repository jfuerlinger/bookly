using Bookly.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookly.Core.Data.Configuration;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.NormalizedIsbn)
            .IsRequired()
            .HasMaxLength(13);

        builder.HasIndex(b => b.NormalizedIsbn)
            .IsUnique();

        builder.Property(b => b.Isbn10).HasMaxLength(10);
        builder.Property(b => b.Isbn13).HasMaxLength(13);

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(b => b.Title);

        builder.Property(b => b.Subtitle).HasMaxLength(500);
        builder.Property(b => b.Publisher).HasMaxLength(300);
        builder.Property(b => b.Language).HasMaxLength(10);
        builder.Property(b => b.Description).HasMaxLength(4000);

        builder.Property(b => b.CoverSmallUrl).HasMaxLength(2048);
        builder.Property(b => b.CoverMediumUrl).HasMaxLength(2048);
        builder.Property(b => b.CoverLargeUrl).HasMaxLength(2048);

        builder.Property(b => b.MetadataSource)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(b => b.CreatedAtUtc).IsRequired();
        builder.Property(b => b.UpdatedAtUtc).IsRequired();
    }
}
