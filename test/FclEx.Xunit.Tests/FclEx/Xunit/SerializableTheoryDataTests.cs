#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable xUnit1045 // Avoid using TheoryData type arguments that might not be serializable

namespace FclEx.Xunit;

public class SerializableTheoryDataTests
{
    public record Person(string Name, int Age)
    {
        public override string ToString() => Name;
    }

    // because TheoryData<T> in v3 does not have method Add(T item), so here we have to specify the type Person
    // ReSharper disable once ArrangeObjectCreationWhenTypeEvident

    public static readonly Person[] People = [new("Tom", 10), new("Jim", 20), new("Tim", 30)];
    public static readonly TheoryData<Person> TestCases = People.ToTheoryData();
    public static readonly SerializableTheoryData<Person> SerializableTestCases = People.ToSerializableTheoryData();
    public static readonly JsonSerializableTheoryData<Person> JsonSerializableTestCases = People.ToJsonSerializableTheoryData();

    [LocalOnlyTheory]
    [MemberData(nameof(TestCases))]
    public async Task NonSerializable_Test(Person person) // cases won't be executed parallelly using ParallelTestFramework
    {
        await Task.Delay(TimeSpan.FromMilliseconds(1000));
    }

    [LocalOnlyTheory]
    [MemberData(nameof(SerializableTestCases))]
    public async Task Serializable_Test(XunitSerializable<Person> person)
    {
        // NOTE: cases will be executed parallelly using ParallelTestFramework in xunit.v2, but sequentially in xunit.v3
        // IXunitSerializable in xunit.v3 requires every member should be serializable
        await Task.Delay(TimeSpan.FromMilliseconds(1000));
    }

    [LocalOnlyTheory]
    [MemberData(nameof(JsonSerializableTestCases))]
    public async Task JsonSerializable_Test(XunitJsonSerializable<Person> person) // cases will be executed parallelly using ParallelTestFramework
    {
        await Task.Delay(TimeSpan.FromMilliseconds(1000));
    }
}
