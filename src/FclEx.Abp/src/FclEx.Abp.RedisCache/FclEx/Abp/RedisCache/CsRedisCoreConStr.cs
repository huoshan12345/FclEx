using System.Text;

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
            using var sb = new ValueStringBuilder();
            sb.Append(Host);
            sb.Append(':');
            sb.Append(Port.ToString());
            if (Password.IsValid())
            {
                sb.Append(",password=");
                sb.Append(Password);
            }
            sb.Append(",defaultDatabase=");
            sb.Append(DefaultDatabase.ToString());
            sb.Append(",connectTimeout=");
            sb.Append(ConnectTimeout.ToString());
            sb.Append(",syncTimeout=");
            sb.Append(SyncTimeout.ToString());
            sb.Append(",testcluster=");
            sb.Append(false.ToLower());
            sb.Append(",poolsize=");
            sb.Append(Poolsize.ToString());
            sb.Append(",ssl=");
            sb.Append(Ssl.ToLower());
            sb.Append(",writeBuffer=");
            sb.Append(WriteBuffer.ToString());
            sb.Append(",prefix=");
            sb.Append(Prefix);
            sb.Append(",tryit=");
            sb.Append(TryIt.ToString());
            sb.Append(",name=");
            sb.Append(Name);
            sb.Append(",preheat=");
            sb.Append(PreHeat.ToString());

            return sb.ToString();
        }
    }
}
