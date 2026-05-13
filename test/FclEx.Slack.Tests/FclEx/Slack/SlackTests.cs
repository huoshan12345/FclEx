using Meziantou.Xunit.v3;

namespace FclEx.Slack;

[CollectionDefinition(nameof(SlackTestsCollection))]
public class SlackTestsCollection : ICollectionFixture<SlackFixture>;

[EnableParallelization]
[Collection(nameof(SlackTestsCollection))]
public class SlackTests;