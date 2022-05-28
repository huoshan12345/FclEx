using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FclEx.Utils;

namespace FclEx.Actions;

public interface IProducer<in TIn, TOut>
{
    Task<OperateResult<TOut>> ProduceAsync(TIn input);
}