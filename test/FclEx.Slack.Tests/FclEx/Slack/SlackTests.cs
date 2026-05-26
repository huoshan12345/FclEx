using Meziantou.Xunit.v3;

namespace FclEx.Slack;

[EnableParallelization]
[CollectionDefinition(nameof(SlackTestsCollection))]
public class SlackTestsCollection : ICollectionFixture<SlackTestsFixture>;

[EnableParallelization]
[Collection(nameof(SlackTestsCollection))]
public class SlackTests;