using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetSlack.Domain.Workspaces;

namespace NetSlack.Infrastructure.Data.Configurations;

public sealed class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.ToTable("workspaces");
        builder.HasKey(workspace => workspace.Id);

        builder.Property(workspace => workspace.Name).HasMaxLength(120).IsRequired();
        builder.Property(workspace => workspace.Slug).HasMaxLength(64).IsRequired();
        builder.Ignore(workspace => workspace.DomainEvents);
        builder.HasIndex(workspace => workspace.Slug).IsUnique();

        builder.Navigation(workspace => workspace.Members).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
