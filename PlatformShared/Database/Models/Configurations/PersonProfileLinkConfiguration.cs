using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    public class PersonProfileLinkConfiguration : IEntityTypeConfiguration<PersonProfileLink>
    {
        public void Configure(EntityTypeBuilder<PersonProfileLink> entity)
        {
            entity.HasKey(e => e.Id).HasName("person_profile_link_pkey");

            entity.ToTable("person_profile_link");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasIdentityOptions(null, null, null, 2147483647L, null, null)
                .HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Creation)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation");
            entity.Property(e => e.Kind)
                .HasConversion<byte>()
                .HasColumnName("kind");
            entity.Property(e => e.ProfileId).HasColumnName("profile_id");
            entity.Property(e => e.TargetProfileId).HasColumnName("target_profile_id");

            entity.HasOne(d => d.Profile).WithMany(p => p.Links)
                .HasForeignKey(d => d.ProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("person_profile_link_profile_id_fkey");

            entity.HasOne(d => d.TargetProfile).WithMany(p => p.TargetLinks)
                .HasForeignKey(d => d.TargetProfileId)
                .HasConstraintName("person_profile_link_target_profile_id_fkey");

            entity.HasOne(d => d.User).WithMany(u => u.ProfileLinks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("person_profile_link_user_id_fkey");
        }
    }
}
