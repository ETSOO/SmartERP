using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class PersonCategoryConfiguration : IEntityTypeConfiguration<PersonCategory>
    {
        public void Configure(EntityTypeBuilder<PersonCategory> entity)
        {
            entity.HasKey(e => e.Id).HasName("person_category_pkey");

            entity.ToTable("person_category");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasIdentityOptions(1001L, null, null, null, null, null)
                .HasColumnName("id");
            entity.Property(e => e.CoreOrganizationId).HasColumnName("core_organization_id");
            entity.Property(e => e.IdentityType)
                .HasConversion<byte>()
                .HasColumnName("identity_type");
            entity.Property(e => e.Names)
                .HasColumnType("character varying(128)[]")
                .HasColumnName("names");
            entity.Property(e => e.Creation)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation");
            entity.Property(e => e.Data)
                .HasColumnType("jsonb")
                .HasColumnName("data");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.OrderIndex)
                .HasDefaultValue((short)0)
                .HasColumnName("order_index");
        }
    }
}
