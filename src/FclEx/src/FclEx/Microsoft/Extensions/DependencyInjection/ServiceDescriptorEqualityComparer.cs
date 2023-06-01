using System.Collections.Generic;
using FclEx;

namespace Microsoft.Extensions.DependencyInjection;

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
        unchecked
        {
            var hashCode = (int)obj.Lifetime;
            hashCode = (hashCode * 397) ^ obj.ServiceType.GetHashCodeSafely();
            hashCode = (hashCode * 397) ^ obj.ImplementationType.GetHashCodeSafely();
            hashCode = (hashCode * 397) ^ obj.ImplementationInstance.GetHashCodeSafely();
            hashCode = (hashCode * 397) ^ obj.ImplementationFactory.GetHashCodeSafely();
            return hashCode;
        }
    }

    public static ServiceDescriptorEqualityComparer Instance { get; } = new();
}