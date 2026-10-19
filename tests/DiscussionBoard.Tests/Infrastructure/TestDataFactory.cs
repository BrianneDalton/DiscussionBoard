namespace DiscussionBoard.Tests.Infrastructure;

public static class TestDataFactory
{
    public const string DefaultForumName = "Programming";
    public const string DefaultThreadTitle = "EF Core Questions";
    public const string DefaultPostTitle = "Re: EF Core Questions";

    // Lesson 1 deliberately begins without compile-time references to Forum,
    // DiscussionThread, or Post because CourseUpdater installs these tests
    // before students create those types. Strongly typed factory methods can
    // be added after the common model contract exists in later lesson updates.
}
