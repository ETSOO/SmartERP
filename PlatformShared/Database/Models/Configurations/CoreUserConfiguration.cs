using com.etsoo.CoreFramework.Business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class CoreUserConfiguration : IEntityTypeConfiguration<CoreUser>
    {
        public void Configure(EntityTypeBuilder<CoreUser> entity)
        {
            entity.HasKey(e => e.Id).HasName("core_user_pkey");

            entity.ToTable("core_user");

            entity.Property(e => e.Id)
                //.UseIdentityAlwaysColumn()
                .HasIdentityOptions(1001L)
                .HasColumnName("id");
            entity.Property(e => e.Password)
                .HasMaxLength(128)
                .HasColumnName("password");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnName("name");
            entity.Property(e => e.GivenName)
                .HasMaxLength(50)
                .HasColumnName("given_name");
            entity.Property(e => e.FamilyName)
                .HasMaxLength(50)
                .HasColumnName("family_name");
            entity.Property(e => e.PreferredName)
                .HasMaxLength(128)
                .HasColumnName("preferred_name");
            entity.Property(e => e.LatinGivenName)
                .HasMaxLength(50)
                .HasColumnName("latin_given_name");
            entity.Property(e => e.LatinFamilyName)
                .HasMaxLength(50)
                .HasColumnName("latin_family_name");
            entity.Property(e => e.Avatar)
                .HasMaxLength(256)
                .HasColumnName("avatar");
            entity.Property(e => e.FrozenTime)
                .HasColumnName("frozen_time");
            entity.Property(e => e.Step)
                .HasColumnName("step");
            entity.Property(e => e.Region)
                .HasMaxLength(2)
                .HasColumnName("region");
            entity.Property(e => e.Pin)
                .HasMaxLength(20)
                .HasColumnName("pin");
            entity.Property(e => e.Creation)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation");
            entity.Property(e => e.Status)
                .HasConversion<byte>()
                .HasDefaultValue(EntityStatus.Normal)
                .HasColumnName("status");
            entity.Property(e => e.QueryKeyword)
                .HasMaxLength(30)
                .HasColumnName("query_keyword");
            entity.Property(e => e.LatestOrganizationIds)
                .HasColumnName("latest_organization_ids");
            entity.Property(e => e.LatestAppIds)
                .HasColumnName("latest_app_ids");
        }
    }
}
