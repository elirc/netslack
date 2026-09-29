using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetSlack.Domain.Channels;

namespace NetSlack.Infrastructure.Data.Configurations;

public sealed class ChannelMemberConfiguration : IEntityTypeConfiguration<ChannelMember>
{
    public void Configure(EntityTypeBuilder<ChannelMember> builder)
    {
        builder.ToTable("channel_members");
        builder.HasKey(member => new { member.ChannelId, member.UserId });

        builder.HasOne(member => member.Channel)
            .WithMany(channel => channel.Members)
            .HasForeignKey(member => member.ChannelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(member => member.User)
            .WithMany()
            .HasForeignKey(member => member.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

