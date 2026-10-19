using DiscussionBoard.Tests.Infrastructure;

namespace DiscussionBoard.Tests;

[CollectionDefinition(Name)]
public sealed class DiscussionBoardCollection : ICollectionFixture<ApplicationFixture>
{
    public const string Name = "DiscussionBoard";
}
