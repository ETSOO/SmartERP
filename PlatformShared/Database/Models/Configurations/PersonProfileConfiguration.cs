using com.etsoo.CoreFramework.Business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class PersonProfileConfiguration : IEntityTypeConfiguration<PersonProfile>
    {
        public void Configure(EntityTypeBuilder<PersonProfile> entity)
        {
            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.PersonId).HasColumnName("person_id");
            entity.Property(e => e.Persons).HasColumnName("persons");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Kind)
                .HasConversion<byte>()
                .HasColumnName("kind");
            entity.Property(e => e.Title)
                .HasMaxLength(256)
                .IsRequired()
                .HasColumnName("title");
            entity.Property(e => e.Comment)
                .IsRequired()
                .HasColumnName("comment");
            entity.Property(e => e.Location)
                .HasMaxLength(256)
                .HasColumnName("location");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.HappenDate).HasColumnName("happen_date");
            entity.Property(e => e.HappenDateEnd).HasColumnName("happen_date_end");
            entity.Property(e => e.CoreUserId).HasColumnName("core_user_id");
            entity.Property(e => e.UserRole)
                .HasConversion<byte>()
                .HasColumnName("user_role");
            entity.Property(e => e.Creation)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation");
            entity.Property(e => e.Status)
                .HasConversion<byte>()
                .HasDefaultValue(EntityStatus.Normal)
                .HasColumnName("status");
            entity.Property(e => e.Data)
                .HasColumnType("jsonb")
                .HasColumnName("data");
            entity.Property(e => e.IndexKey)
                .HasMaxLength(30)
                .HasColumnName("index_key");
        }
    }
}
