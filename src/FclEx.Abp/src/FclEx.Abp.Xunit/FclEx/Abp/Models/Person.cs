using System;

namespace FclEx.Abp.Models;

public class Person : IEquatable<Person>
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public int? CoinCount { get; set; }

    public bool Equals(Person? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id
               && string.Equals(Name, other.Name)
               && Age == other.Age
               && CoinCount == other.CoinCount;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Person)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Id;
            hashCode = (hashCode * 397) ^ Name.GetHashCodeSafely();
            hashCode = (hashCode * 397) ^ Age;
            hashCode = (hashCode * 397) ^ CoinCount.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(Person left, Person right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Person left, Person right)
    {
        return !Equals(left, right);
    }
}