using System;
using System.Diagnostics.CodeAnalysis;
using FclEx.Web.Models;

namespace FclEx.Web.Core
{
    public interface IHasAccount
    {
        AccountStatus AccountStatus { get; set; }
        event Action<AccountStatus> OnAccountStatusChanged;
    }

    public interface IHasAccount<TAccount> : IHasAccount
    {
        [DisallowNull, MaybeNull]
        TAccount Account { get; set; }
    }
}
