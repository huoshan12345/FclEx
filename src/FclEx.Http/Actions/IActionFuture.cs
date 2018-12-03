using System;

namespace FclEx.Http.Actions
{    /// <summary>
     /// 用于按顺序执行一些action，前一个action成功则继续执行，否则则退出
     /// </summary>
    public interface IActionFuture : IActor
    {
        int Count { get; }

        /// <summary>
        /// 放入一个根据所有action执行结果生成action的委托到执行队列末尾
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        IActionFuture PushAction(Func<object[], IActor> func);
    }
}
