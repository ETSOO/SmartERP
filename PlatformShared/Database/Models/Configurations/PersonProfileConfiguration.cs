using com.etsoo.CoreFramework.Business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class PersonProfileConfiguration : IEntityTypeConfiguration<PersonProfile>
    {
        public void Configure(EntityTypeBuilder<PersonProfile> entity)
        {
            entity.HasKey(e => e.Id).HasName("person_profile_pkey");

            entity.ToTable("person_profile");

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
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserRole)
                .HasConversion<short>()
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
            entity.Property(e => e.Importance)
                .HasConversion<byte>()
                .HasColumnName("importance");
            entity.Property(e => e.AssigneeId).HasColumnName("assignee_id");

            entity.HasOne(d => d.Assignee).WithMany(p => p.AssignedProfiles)
                .HasForeignKey(d => d.AssigneeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("person_profile_assignee_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.CreatedProfiles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("person_profile_user_id_fkey");

            entity.HasOne(d => d.Order).WithMany(o => o.Profiles)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("person_profile_order_id_fkey");

            entity.HasOne(d => d.Person).WithMany(p => p.Profiles)
                .HasForeignKey(d => d.PersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("person_profile_person_id_fkey");
        }
    }
}
