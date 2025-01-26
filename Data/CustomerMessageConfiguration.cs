using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ContactSvc.Dtos;

namespace ContactSvc.Data
{
    public class CustomerMessageConfiguration : IEntityTypeConfiguration<CustomerMessage>
    {
        public void Configure(EntityTypeBuilder<CustomerMessage> builder)
        {
            builder.HasKey(cm => cm.Id);
            builder.Property(cm => cm.customerEmail).IsRequired().HasMaxLength(255);
            builder.Property(cm => cm.customerName).IsRequired().HasMaxLength(255);
            builder.Property(cm => cm.messageText).IsRequired();
        }
    }
}