// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Collections.Generic.OrderedList;

public class OrderedListTests_String : OrderedListTests<string>
{
    protected override string CreateT(int seed)
    {
        var stringLength = seed % 10 + 5;
        var rand = new Random(seed);
        var bytes = new byte[stringLength];
        rand.NextBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}

public class OrderedListTests_Int : OrderedListTests<int>
{
    protected override int CreateT(int seed)
    {
        var rand = new Random(seed);
        return rand.Next();
    }
}

public class OrderedListTests_String_ReadOnly : OrderedListTests<string>
{
    protected override string CreateT(int seed)
    {
        var stringLength = seed % 10 + 5;
        var rand = new Random(seed);
        var bytes = new byte[stringLength];
        rand.NextBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    protected override bool IsReadOnly => true;

    protected override IList<string> GenericIListFactory(int setLength)
    {
        return GenericListFactory(setLength).AsReadOnly();
    }

    protected override IList<string> GenericIListFactory()
    {
        return GenericListFactory().AsReadOnly();
    }

    protected override IEnumerable<ModifyEnumerable> GetModifyEnumerables(ModifyOperation operations) => new List<ModifyEnumerable>();
        
    protected override bool Enumerator_Empty_Current_UndefinedOperation_Throws => true;
}

public class OrderedListTests_Int_ReadOnly : OrderedListTests<int>
{
    protected override int CreateT(int seed)
    {
        var rand = new Random(seed);
        return rand.Next();
    }

    protected override bool IsReadOnly => true;

    protected override IList<int> GenericIListFactory(int setLength)
    {
        return GenericListFactory(setLength).AsReadOnly();
    }

    protected override IList<int> GenericIListFactory()
    {
        return GenericListFactory().AsReadOnly();
    }

    protected override IEnumerable<ModifyEnumerable> GetModifyEnumerables(ModifyOperation operations) => new List<ModifyEnumerable>();

    protected override bool Enumerator_Empty_Current_UndefinedOperation_Throws => true;
}