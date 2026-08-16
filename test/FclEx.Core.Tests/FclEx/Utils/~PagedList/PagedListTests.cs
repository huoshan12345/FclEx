namespace FclEx.Utils;

public class PagedListTests
{
    [Fact]
    public void EmptyArray_FirstPage_Test()
    {
        var arr = Enumerable.Empty<int>().ToList();
        var pagedList = new PagedList<int>(arr, 0, 10, 0);
        Assert.Equal(0, pagedList.PageCount);
        Assert.Equal(0, pagedList.TotalCount);
        Assert.Equal(0, pagedList.PageIndex);
        Assert.Equal(1, pagedList.PageNumber);
        Assert.Equal(10, pagedList.PageSize);
        Assert.False(pagedList.HasPreviousPage);
        Assert.False(pagedList.HasNextPage);
        Assert.True(pagedList.IsFirstPage);
        Assert.True(pagedList.IsLastPage);
        Assert.Equal(0, pagedList.ItemStart);
        Assert.Equal(0, pagedList.ItemEnd);
    }

    [Fact]
    public void NonEmptyArray_OnlyOnePage_FirstPage_Test()
    {
        var arr = Enumerable.Range(1, 9).ToArray();
        var pagedList = new PagedList<int>(arr, 0, 10, arr.Length);
        Assert.Equal(1, pagedList.PageCount);
        Assert.Equal(9, pagedList.TotalCount);
        Assert.Equal(0, pagedList.PageIndex);
        Assert.Equal(1, pagedList.PageNumber);
        Assert.Equal(10, pagedList.PageSize);
        Assert.False(pagedList.HasPreviousPage);
        Assert.False(pagedList.HasNextPage);
        Assert.True(pagedList.IsFirstPage);
        Assert.True(pagedList.IsLastPage);
        Assert.Equal(1, pagedList.ItemStart);
        Assert.Equal(9, pagedList.ItemEnd);
    }


    [Fact]
    public void NonEmptyArray_MoreThanOnePage_FirstPage_Test()
    {
        var arr = Enumerable.Range(1, 10).ToArray();
        var pagedList = new PagedList<int>(arr, 0, 10, 55);
        Assert.Equal(6, pagedList.PageCount);
        Assert.Equal(55, pagedList.TotalCount);
        Assert.Equal(0, pagedList.PageIndex);
        Assert.Equal(1, pagedList.PageNumber);
        Assert.Equal(10, pagedList.PageSize);
        Assert.False(pagedList.HasPreviousPage);
        Assert.True(pagedList.HasNextPage);
        Assert.True(pagedList.IsFirstPage);
        Assert.False(pagedList.IsLastPage);
        Assert.Equal(1, pagedList.ItemStart);
        Assert.Equal(10, pagedList.ItemEnd);
    }

    [Fact]
    public void NonEmptyArray_MoreThanOnePage_LastPage_Test()
    {
        var arr = Enumerable.Range(51, 5).ToArray();
        var pagedList = new PagedList<int>(arr, 5, 10, 55);
        Assert.Equal(6, pagedList.PageCount);
        Assert.Equal(55, pagedList.TotalCount);
        Assert.Equal(5, pagedList.PageIndex);
        Assert.Equal(6, pagedList.PageNumber);
        Assert.Equal(10, pagedList.PageSize);
        Assert.True(pagedList.HasPreviousPage);
        Assert.False(pagedList.HasNextPage);
        Assert.False(pagedList.IsFirstPage);
        Assert.True(pagedList.IsLastPage);
        Assert.Equal(51, pagedList.ItemStart);
        Assert.Equal(55, pagedList.ItemEnd);
    }


    [Fact]
    public void NonEmptyArray_MoreThanOnePage_SecondPage_Test()
    {
        var arr = Enumerable.Range(11, 10).ToArray();
        var pagedList = new PagedList<int>(arr, 1, 10, 55);
        Assert.Equal(6, pagedList.PageCount);
        Assert.Equal(55, pagedList.TotalCount);
        Assert.Equal(1, pagedList.PageIndex);
        Assert.Equal(2, pagedList.PageNumber);
        Assert.Equal(10, pagedList.PageSize);
        Assert.True(pagedList.HasPreviousPage);
        Assert.True(pagedList.HasNextPage);
        Assert.False(pagedList.IsFirstPage);
        Assert.False(pagedList.IsLastPage);
        Assert.Equal(11, pagedList.ItemStart);
        Assert.Equal(20, pagedList.ItemEnd);
    }

    [Fact]
    public void Constructor_ShouldRejectZeroPageSize_WhenItemsAreEmpty()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PagedList<int>([], pageIndex: 0, pageSize: 0, totalCount: 0));

        Assert.Equal("pageSize", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectPageOffsetThatCannotBeRepresented()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PagedList<int>([], pageIndex: int.MaxValue, pageSize: 2, totalCount: 0));

        Assert.Equal("pageIndex", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectPageIndexOutsideKnownPages()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PagedList<int>([], pageIndex: 2, pageSize: 10, totalCount: 15));

        Assert.Equal("pageIndex", exception.ParamName);
    }

    [Theory]
    [InlineData(0, 10, 0, 1)]
    [InlineData(0, 2, 10, 3)]
    [InlineData(1, 3, 5, 3)]
    public void Constructor_ShouldRejectItemsThatCannotBelongToThePage(
        int pageIndex,
        int pageSize,
        int totalCount,
        int itemCount)
    {
        var items = Enumerable.Range(0, itemCount).ToArray();

        var exception = Assert.Throws<ArgumentException>(() =>
            new PagedList<int>(items, pageIndex, pageSize, totalCount));

        Assert.Equal("items", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldUseActualItemsForItemRange()
    {
        var pagedList = new PagedList<int>([11, 12], pageIndex: 1, pageSize: 10, totalCount: 55);

        Assert.Equal(11, pagedList.ItemStart);
        Assert.Equal(12, pagedList.ItemEnd);
    }

    [Fact]
    public void Constructor_ShouldDefensivelyCopyItems()
    {
        var items = new List<int> { 1, 2 };
        var pagedList = new PagedList<int>(items, pageIndex: 0, pageSize: 10, totalCount: 2);

        items[0] = 99;
        items.Add(3);

        Assert.Equal([1, 2], pagedList);
        Assert.Equal(2, pagedList.Count);
        Assert.Equal(1, pagedList.ItemStart);
        Assert.Equal(2, pagedList.ItemEnd);
    }
}
