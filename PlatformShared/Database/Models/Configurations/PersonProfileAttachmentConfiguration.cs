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
                .IsRequired()
                .HasColumnName("content_type");
            entity.Property(e => e.Description)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnName("description");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Creation)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation");

            entity.HasOne(d => d.Profile).WithMany(p => p.Attachments)
                .HasForeignKey(d => d.ProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("person_profile_attachment_profile_id_fkey");

            entity.HasOne(d => d.User).WithMany(u => u.ProfileAttachments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("person_profile_attachment_user_id_fkey");
        }
    }
}
