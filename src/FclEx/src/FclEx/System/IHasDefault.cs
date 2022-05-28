using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System;

public interface IHasDefault<out TSelf> where TSelf : new()
{
    static TSelf Default { get; } = new();
}