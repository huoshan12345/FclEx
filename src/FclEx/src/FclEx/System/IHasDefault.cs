using System.Collections.Generic;

namespace System;

public interface IHasDefault<out TSelf> where TSelf : new()
{
    static TSelf Default { get; } = new();
}