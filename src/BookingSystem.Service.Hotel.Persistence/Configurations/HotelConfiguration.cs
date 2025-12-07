using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingSystem.Service.Hotel.Persistence.Configurations
{
    public class HotelConfiguration : IEntityTypeConfiguration<Domain.Entities.Hotel>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.Hotel> builder)
        {
            builder.ToTable("Hotels");
            builder.HasKey(h => h.Id);
            builder.Property(h => h.Name)
                .IsRequired()
                .HasMaxLength(200);
            builder.Property(h => h.Description)
                .HasMaxLength(1000);
            builder.Property(h => h.StarRating)
                .IsRequired()
                .HasConversion<int>();
            builder.Property(h => h.Status)
                .IsRequired()
                .HasConversion<int>();
            builder.Property(h => h.Street)
                .HasMaxLength(200);
            builder.Property(h => h.City)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(h => h.State)
                .HasMaxLength(100);
            builder.Property(h => h.Country)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(h => h.ZipCode)
                .HasMaxLength(20);
            builder.Property(h => h.Email)
                .HasMaxLength(100);
            builder.Property(h => h.Phone)
                .HasMaxLength(20);
            builder.Property(h => h.Website)
                .HasMaxLength(200);
            builder.Property(h => h.CreatedAt)
                .IsRequired();
            builder.Property(h => h.UpdatedAt);
            builder.Property(h => h.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasIndex(h => h.Name);
            builder.HasIndex(h => h.City);
            builder.HasIndex(h => h.IsDeleted);

            builder.HasQueryFilter(h => !h.IsDeleted);

        }
    }
}
