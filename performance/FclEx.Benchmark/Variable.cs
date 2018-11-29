namespace FclEx.Benchmark
{
    internal static class Variable
    {
        public const ShortEnum ShortEnum = Benchmark.ShortEnum.Yes;
        public const LongEnum LongEnum = Benchmark.LongEnum.Yes;
        public const IntEnum IntEnum = Benchmark.IntEnum.Yes;

        public const int IntNumber = 100;
        public const short ShortNumber = 100;
        public const long LongNumber = 100;

        public static readonly object IntObj = IntNumber;
        public static readonly object ShortObj = ShortNumber;
        public static readonly object LongObj = LongNumber;
    }
}