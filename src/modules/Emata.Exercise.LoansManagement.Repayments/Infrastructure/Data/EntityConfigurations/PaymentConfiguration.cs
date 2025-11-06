using Emata.Exercise.LoansManagement.Repayments.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Emata.Exercise.LoansManagement.Repayments.Infrastructure.Data.EntityConfigurations;

internal class PaymentConfiguration : IEntityTypeConfiguration<Repayment>
{
    public void Configure(EntityTypeBuilder<Repayment> builder)
    {
        //set the default value for CreatedOn
        builder.Property(e => e.CreatedOn)
            .HasDefaultValueSql("now()");
    }
}
