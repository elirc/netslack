using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetSlack.Domain.Messages;

namespace NetSlack.Infrastructure.Data.Configurations;

public sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages");
        builder.HasKey(message => message.Id);

        builder.Property(message => message.Body).HasMaxLength(4_000).IsRequired();
        builder.Ignore(message => message.DomainEvents);

        builder.HasIndex(message => new { message.ChannelId, message.PostedAt });
        builder.HasOne<NetSlack.Domain.Channels.Channel>()
            .WithMany()
            .HasForeignKey(message => message.ChannelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<NetSlack.Domain.Users.UserProfile>()
            .WithMany()
            .HasForeignKey(message => message.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
