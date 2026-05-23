namespace FclEx.DependencyInjection;

public class ServiceDescriptorEqualityComparer : IEqualityComparer<ServiceDescriptor>
{
    public bool Equals(ServiceDescriptor? x, ServiceDescriptor? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.Lifetime == y.Lifetime
               && x.ServiceType == y.ServiceType
               && x.ImplementationType == y.ImplementationType
               && x.ImplementationInstance == y.ImplementationInstance
               && x.ImplementationFactory == y.ImplementationFactory;
    }

    public int GetHashCode(ServiceDescriptor obj)
    {
        return HashCode.Combine(
              obj.Lifetime,
              obj.ServiceType,
              obj.ImplementationType,
              obj.ImplementationInstance,
              obj.ImplementationFactory);

    }

    public static ServiceDescriptorEqualityComparer Instance { get; } = new();
}