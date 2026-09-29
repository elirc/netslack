using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetSlack.Domain.Channels;

namespace NetSlack.Infrastructure.Data.Configurations;

public sealed class ChannelConfiguration : IEntityTypeConfiguration<Channel>
{
    public void Configure(EntityTypeBuilder<Channel> builder)
    {
        builder.ToTable("channels");
        builder.HasKey(channel => channel.Id);

        builder.Property(channel => channel.Name).HasMaxLength(80).IsRequired();
        builder.Property(channel => channel.Topic).HasMaxLength(280).IsRequired();
        builder.Property(channel => channel.Type).HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Ignore(channel => channel.DomainEvents);

        builder.HasIndex(channel => new { channel.WorkspaceId, channel.Name }).IsUnique();
        builder.HasOne<NetSlack.Domain.Workspaces.Workspace>()
            .WithMany()
            .HasForeignKey(channel => channel.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(channel => channel.Members).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
