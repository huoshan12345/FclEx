using System;
using System.Collections.Generic;
using System.Text;
using FclEx.Http.Event;
using FclEx.Utils;
using Xunit;

namespace FclEx.Http.Test.Event
{
    public class ActionEventOfTTests
    {
        [Fact]
        public void ToOperateResult_Ok()
        {
            var action = new ActionEvent<string>(ActionEventType.EvtOk, "Ok");
            var result = action.ToOperateResult();
            Assert.True(result.Successful);
            Assert.Equal(action.Result, result.Result);
        }

        [Fact]
        public void ToOperateResult_Error()
        {
            var action = ActionEvent.Error<string>("Error");
            var result = action.ToOperateResult();
            Assert.False(result.Successful);
            Assert.Equal(action.Exception, result.Exception);
        }

        [Fact]
        public void ToOperateResult_Canceled()
        {
            var action = new ActionEvent<string>(ActionEventType.EvtCanceled, "Cancel");
            var result = action.ToOperateResult();
            Assert.False(result.Successful);
            Assert.IsType<SimpleException>(result.Exception);
            Assert.Contains("canceled", result.Msg);
            Assert.NotNull(result.StackTrace);
        }


        [Fact]
        public void ToOperateResult_Repeat()
        {
            var action = new ActionEvent<string>(ActionEventType.EvtRepeat, "Repeat");
            var result = action.ToOperateResult();
            Assert.False(result.Successful);
            Assert.IsType<SimpleException>(result.Exception);
            Assert.Contains("not finished", result.Msg);
            Assert.NotNull(result.StackTrace);
        }

        [Fact]
        public void ToOperateResult_Retry()
        {
            var action = new ActionEvent<string>(ActionEventType.EvtRetry, "Retry");
            var result = action.ToOperateResult();
            Assert.False(result.Successful);
            Assert.IsType<SimpleException>(result.Exception);
            Assert.Contains("not finished", result.Msg);
            Assert.NotNull(result.StackTrace);
        }
    }
}
