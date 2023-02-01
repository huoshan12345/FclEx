namespace FclEx.Abp.RedisCache
{
    public class CsRedisCoreConStr
    {
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 6379;
        public int ConnectTimeout { get; set; } = 5000;
        public int SyncTimeout { get; set; } = 5000;
        public string Password { get; set; } = "";
        public int DefaultDatabase { get; set; } = 0;
        public int Poolsize { get; set; } = 50;
        public int PreHeat { get; set; } = 0;
        public bool Ssl { get; set; } = false;
        public int WriteBuffer { get; set; } = 10240;
        public int TryIt { get; set; } = 0;
        public string Name { get; set; } = "";
        public string Prefix { get; set; } = "";

        public override string ToString()
        {
            return $"{Host}:{Port},password={Password},defaultDatabase={DefaultDatabase}," +
                   $"connectTimeout={ConnectTimeout},syncTimeout={SyncTimeout},testcluster=false" +
                   $"poolsize={Poolsize},ssl={Ssl.ToString().ToLower()},writeBuffer={WriteBuffer}," +
                   $"prefix={Prefix},tryit={TryIt},name={Name},preheat={PreHeat.ToString().ToLower()}";
        }
    }
}
