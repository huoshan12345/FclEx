using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FclEx.Http.Core;
using Xunit;

namespace FclEx.Http.Test.HttpReqTests
{
    public class SetHostTests
    {
        public static string[] Hosts { get; } =
        {
            "localhost",
            "www.baidu.com",
            "127.0.0.1",
            "220.181.112.244"
        };

        public static int[] Ports { get; } = { 80, 8080, 1234, };

        public static IEnumerable<object[]> HostPortsPair { get; } = Hosts
            .SelectMany(m => Ports, (i, j) => (h: i, p: j)).SelectMany((i, j) => new object[]
            {
                i.h,
                i.p,
                i.p == 80 ? i.h : $"{i.h}:{i.p}",
                j.h,
                j.p,
                j.p == 80 ? j.h : $"{j.h}:{j.p}",
            })
            .ToArray();


        [Theory]
        [MemberData(nameof(HostPortsPair))]
        public void TestSetHost(string host, int port, string hp, string newHost, int newPort, string newHp)
        {
            var req = HttpReq.Get("http://" + host);
            Assert.Equal(host, req.Host);
            Assert.Equal(80, req.Port);

            req.Host(hp);
            Assert.Equal(host, req.Host);
            Assert.Equal(port, req.Port);

            req.Host(newHp);
            Assert.Equal(newHost, req.Host);
            Assert.Equal(newPort, req.Port);
        }

        [Fact]
        public void SetHostWithSchemeTest()
        {
            var req = HttpReq.Get("/teacher/app/clean-redis-cache")
                .Host("https://betassapinew.knowbox.cn:9002");

            Assert.Equal("https", req.Scheme);
            Assert.Equal("betassapinew.knowbox.cn", req.Host);
            Assert.Equal(9002, req.Port);
        }
    }
}
