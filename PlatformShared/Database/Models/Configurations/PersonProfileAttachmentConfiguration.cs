using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class PersonProfileAttachmentConfiguration : IEntityTypeConfiguration<PersonProfileAttachment>
    {
        public void Configure(EntityTypeBuilder<PersonProfileAttachment> entity)
        {
            entity.HasKey(e => e.Id).HasName("person_profile_attachment_pkey");

            entity.ToTable("person_profile_attachment");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.ProfileId).HasColumnName("profile_id");
            entity.Property(e => e.FileName)
                .HasMaxLength(256)
                .IsRequired()
                .HasColumnName("file_name");
            entity.Property(e => e.FileSize).HasColumnName("file_size");
            entity.Property(e => e.ContentType)
                .HasMaxLength(128)
                .HasColumnName("content_type");
            entity.Property(e => e.CoreUserId).HasColumnName("core_user_id");
            entity.Property(e => e.Creation)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation");
        }
    }
}
