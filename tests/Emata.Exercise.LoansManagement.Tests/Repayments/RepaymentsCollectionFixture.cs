using System;
using Emata.Exercise.LoansManagement.Tests.Setup;

namespace Emata.Exercise.LoansManagement.Tests.Repayments;

[CollectionDefinition(CollectionName)]
public class RepaymentsCollectionFixture : ICollectionFixture<ApiFactory>
{
    public const string CollectionName = "RepaymentsCollection";
}