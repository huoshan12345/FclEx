using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FclEx.Utils;
using Xunit;

namespace FclEx.Test.Utils
{
    public class PagedListTests
    {
        [Fact]
        public void EmptyArray_FirstPage_Test()
        {
            var arr = Enumerable.Empty<int>();
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
            var arr = Enumerable.Range(1, 55).ToArray();
            var pagedList = new PagedList<int>(arr, 0, 10, arr.Length);
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
            var arr = Enumerable.Range(1, 55).ToArray();
            var pagedList = new PagedList<int>(arr, 5, 10, arr.Length);
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
            var arr = Enumerable.Range(1, 55).ToArray();
            var pagedList = new PagedList<int>(arr, 1, 10, arr.Length);
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
    }
}
