using com.etsoo.CoreFramework.Business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class PersonConfiguration : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> entity)
        {
            entity.HasKey(e => e.Id).HasName("person_pkey");

            entity.ToTable("person");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasIdentityOptions(1001L, null, null, null, null, null)
                .HasColumnName("id");
            entity.Property(e => e.Uid)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("uid");
            entity.Property(e => e.OrgId).HasColumnName("org_id");
            entity.Property(e => e.CoreOrganizationId).HasColumnName("core_organization_id");
            entity.Property(e => e.CoreUserId).HasColumnName("core_user_id");
            entity.Property(e => e.UserRole)
                .HasConversion<short>()
                .HasColumnName("user_role");
            entity.Property(e => e.IdentityType)
                .HasConversion<byte>()
                .HasColumnName("identity_type");
            entity.Property(e => e.IsLegalPerson)
                .HasDefaultValue(false)
                .HasColumnName("is_legal_person");
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
            entity.Property(e => e.LatinGivenName)
                .HasMaxLength(50)
                .HasColumnName("latin_given_name");
            entity.Property(e => e.LatinFamilyName)
                .HasMaxLength(50)
                .HasColumnName("latin_family_name");
            entity.Property(e => e.PreferredName)
                .HasMaxLength(128)
                .HasColumnName("preferred_name");
            entity.Property(e => e.Title)
                .HasConversion<byte>()
                .HasColumnName("title");
            entity.Property(e => e.JobTitle)
                .HasMaxLength(50)
                .HasColumnName("job_title");
            entity.Property(e => e.Description)
                .HasMaxLength(1280)
                .HasColumnName("description");
            entity.Property(e => e.Avatar)
                .HasMaxLength(256)
                .HasColumnName("avatar");
            entity.Property(e => e.Birthday).HasColumnName("birthday");
            entity.Property(e => e.AssignedId)
                .HasMaxLength(20)
                .HasColumnName("assigned_id");
            entity.Property(e => e.Regions)
                .HasColumnType("character(2)[]")
                .HasColumnName("regions");
            entity.Property(e => e.Currencies)
                .HasColumnType("character(3)[]")
                .HasColumnName("currencies");
            entity.Property(e => e.Cultures)
                .HasColumnType("character varying(10)[]")
                .HasColumnName("cultures");
            entity.Property(e => e.Ethnicity)
                .HasMaxLength(50)
                .HasColumnName("ethnicity");
            entity.Property(e => e.Gender)
                .HasMaxLength(1)
                .HasColumnName("gender");
            entity.Property(e => e.Height).HasColumnName("height");
            entity.Property(e => e.Weight).HasColumnName("weight");
            entity.Property(e => e.MaritalStatus)
                .HasConversion<byte>()
                .HasColumnName("marital_status");
            entity.Property(e => e.Education)
                .HasConversion<byte>()
                .HasColumnName("education");
            entity.Property(e => e.Degree)
                .HasConversion<byte>()
                .HasColumnName("degree");
            entity.Property(e => e.Data)
                .HasColumnType("jsonb")
                .HasColumnName("data");
            entity.Property(e => e.Creation)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation");
            entity.Property(e => e.Expiry).HasColumnName("expiry");
            entity.Property(e => e.RefreshTime)
                .HasDefaultValueSql("now()")
                .HasColumnName("refresh_time");
            entity.Property(e => e.Status)
                .HasConversion<byte>()
                .HasDefaultValue(EntityStatus.Normal)
                .HasColumnName("status");
            entity.Property(e => e.QueryKeyword)
                .HasMaxLength(30)
                .HasColumnName("query_keyword");
            entity.Property(e => e.InviterId).HasColumnName("inviter_id");
            entity.Property(e => e.ReportTo).HasColumnName("report_to");
            entity.Property(e => e.PoliticalStatus)
                .HasMaxLength(50)
                .HasColumnName("political_status");
            entity.Property(e => e.CategoryIds).HasColumnName("category_ids");
            entity.Property(e => e.Keywords).HasColumnName("keywords");
            entity.Property(e => e.Addresses).HasColumnName("addresses");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.PermissionGroups).HasColumnName("permission_groups");
            entity.Property(e => e.PermissionIncluded).HasColumnName("permission_included");
            entity.Property(e => e.PermissionExcluded).HasColumnName("permisson_excluded");

            entity.HasOne(d => d.User).WithMany(p => p.OwnedUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("person_user_id_fkey");

            entity.HasOne(d => d.CoreUser).WithMany(u => u.BoundPersons)
                .HasForeignKey(d => d.CoreUserId)
                .HasConstraintName("person_core_user_id_fkey");

            entity.HasOne(d => d.Inviter).WithMany(u => u.InvitedPersons)
                .HasForeignKey(d => d.InviterId)
                .HasConstraintName("person_inviter_id_fkey");

            entity.HasOne(d => d.ReportToUser).WithMany(p => p.DirectReports)
                .HasForeignKey(d => d.ReportTo)
                .HasConstraintName("person_report_to_fkey");

            entity.HasOne(d => d.CoreOrganization).WithMany(o => o.BoundPersons)
                .HasForeignKey(d => d.CoreOrganizationId)
                .HasConstraintName("person_core_organization_id_fkey");

            entity.HasOne(d => d.Organization).WithMany(o => o.Persons)
                .HasForeignKey(d => d.OrgId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("person_org_id_fkey");

            entity.HasMany(d => d.PermissionItems).WithMany(p => p.Persons)
                .UsingEntity<Dictionary<string, object>>(
                    "PersonPermissionItem",
                    r => r.HasOne<PermissionItem>().WithMany()
                        .HasForeignKey("PermissionItemId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("person_permission_item_permission_item_id_fkey"),
                    l => l.HasOne<Person>().WithMany()
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("person_permission_item_person_id_fkey"),
                    j =>
                    {
                        j.HasKey("PersonId", "PermissionItemId").HasName("person_permission_item_pkey");
                        j.ToTable("person_permission_item");
                        j.IndexerProperty<long>("PersonId").HasColumnName("person_id");
                        j.IndexerProperty<short>("PermissionItemId").HasColumnName("permission_item_id");
                    });
        }
    }
}
