using System;
using FclEx.Utils;

namespace FclEx.Actions
{
    public interface IActionFuture : IActor
    {
        int Count { get; }

        /// <summary>
        /// 放入一个根据所有action执行结果生成action的委托到执行队列末尾
        /// </summary>
        /// <param name="actorSelector"></param>
        /// <param name="terminationCondition"></param>
        /// <returns></returns>
        IActionFuture PushAction(Func<IOperateResult[], IActor> actorSelector, Func<IOperateResult, bool> terminationCondition = null);
    }
}
