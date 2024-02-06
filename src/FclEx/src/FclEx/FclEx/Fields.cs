using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FclEx;

public static class Fields
{
    public static readonly FieldInfo Exception_Message = typeof(Exception).GetRequiredField("_message");
    public static readonly FieldInfo Exception_StackTrace = typeof(Exception).GetRequiredField("_stackTraceString");
}