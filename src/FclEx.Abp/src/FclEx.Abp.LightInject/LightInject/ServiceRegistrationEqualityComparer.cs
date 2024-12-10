namespace LightInject;

public class ServiceRegistrationEqualityComparer : IEqualityComparer<ServiceRegistration>
{
    public bool Equals(ServiceRegistration? x, ServiceRegistration? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x == null || y == null) return false;

        return x.ServiceType == y.ServiceType
               && x.Lifetime?.GetType() == y.Lifetime?.GetType()
               && x.ImplementingType == y.ImplementingType
               && x.Value == y.Value
               && x.FactoryExpression == y.FactoryExpression;
    }

    public int GetHashCode(ServiceRegistration obj)
    {
        var hashCode = 2001076147;
        hashCode = hashCode * -1521134295 + obj.ServiceType.GetHashCodeSafely();
        hashCode = hashCode * -1521134295 + obj.Lifetime.GetType().GetHashCodeSafely();
        hashCode = hashCode * -1521134295 + obj.ImplementingType.GetHashCodeSafely();
        hashCode = hashCode * -1521134295 + obj.Value.GetHashCodeSafely();
        hashCode = hashCode * -1521134295 + obj.FactoryExpression.GetHashCodeSafely();
        return hashCode;
    }

    public static ServiceRegistrationEqualityComparer Instance { get; } = new();
}