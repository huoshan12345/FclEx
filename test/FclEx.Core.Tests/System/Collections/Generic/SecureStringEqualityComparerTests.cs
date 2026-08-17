namespace System.Collections.Generic;

using System.Security;

public class SecureStringEqualityComparerTests
{
    [Fact]
    public void EqualContentsAreEqualAndHaveSameHashCode()
    {
        using var left = Create("secret");
        using var right = Create("secret");

        Assert.True(SecureStringEqualityComparer.Instance.Equals(left, right));
        Assert.Equal(
            SecureStringEqualityComparer.Instance.GetHashCode(left),
            SecureStringEqualityComparer.Instance.GetHashCode(right));
    }

    [Fact]
    public void DifferentContentsAreNotEqual()
    {
        using var left = Create("secret-1");
        using var right = Create("secret-2");

        Assert.False(SecureStringEqualityComparer.Instance.Equals(left, right));
    }

    private static SecureString Create(string value)
    {
        var result = new SecureString();
        foreach (var character in value)
        {
            result.AppendChar(character);
        }
        result.MakeReadOnly();
        return result;
    }
}
