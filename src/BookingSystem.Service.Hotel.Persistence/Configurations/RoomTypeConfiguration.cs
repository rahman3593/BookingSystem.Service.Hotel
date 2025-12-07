using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingSystem.Service.Hotel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingSystem.Service.Hotel.Persistence.Configurations
{
    public class RoomTypeConfiguration : IEntityTypeConfiguration<Domain.Entities.RoomType>
    {
        public void Configure(EntityTypeBuilder<RoomType> builder)
        {
            builder.ToTable("RoomTypes");

            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(rt => rt.Description)
                .HasMaxLength(500);

            builder.Property(rt => rt.MaxOccupancy)
                .IsRequired();

            builder.Property(rt => rt.BasePrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(rt => rt.Size)
                .IsRequired()
                .HasColumnType("decimal(10,2)");

            builder.Property(rt => rt.BedType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(rt => rt.ViewType)
                .HasMaxLength(50);

            builder.Property(rt => rt.HasBalcony)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(rt => rt.HasKitchen)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(rt => rt.IsSmokingAllowed)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(rt => rt.HotelId)
                .IsRequired();

            builder.Property(rt => rt.CreatedAt)
                .IsRequired();

            builder.Property(rt => rt.UpdatedAt);

            builder.Property(rt => rt.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasIndex(rt => rt.HotelId);
            builder.HasIndex(rt => rt.Name);
            builder.HasIndex(rt => rt.IsDeleted);

            builder.HasQueryFilter(rt => !rt.IsDeleted);

            builder.HasOne<Domain.Entities.Hotel>()
                   .WithMany()
                   .HasForeignKey(rt => rt.HotelId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
