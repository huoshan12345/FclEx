namespace FclEx.Slack;

[CollectionDefinition(nameof(SlackTestsCollection))]
public class SlackTestsCollection : ICollectionFixture<SlackTestsFixture>;

[Collection(nameof(SlackTestsCollection))]
public class SlackTests;