using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using FclEx;

namespace FclEx.Image.Test
{
    public class Base64Tests
    {
        [Fact]
        public void Base64ToPic()
        {
            var txt = File.ReadAllText("base64.txt");
            var str = txt.Split(',')[0];
            var raw = str.Base64StringToBytes();

        }
    }
}
