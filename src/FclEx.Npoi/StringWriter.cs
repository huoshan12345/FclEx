using System.IO;
using System.Text;

namespace FclEx.Npoi
{
    internal class StringWriterWrapper : IStringWriter
    {
        public StringWriterWrapper(StringWriter stringWriter)
        {
            this.StringWriter = stringWriter;
        }

        public StringWriter StringWriter { get; set; }

        public void WriteLine()
        {
            this.StringWriter.WriteLine();
        }

        public void Write(string str)
        {
            this.StringWriter.Write(str);
        }

        public void Write(char c)
        {
            this.StringWriter.Write(c);
        }
    }

    internal class StringBuilderWrapper : IStringWriter
    {
        public StringBuilderWrapper(StringBuilder stringBuilder)
        {
            this.StringBuilder = stringBuilder;
        }

        public StringBuilder StringBuilder { get; set; }

        public void WriteLine()
        {
            this.StringBuilder.AppendLine();
        }

        public void Write(string str)
        {
            this.StringBuilder.Append(str);
        }

        public void Write(char c)
        {
            this.StringBuilder.Append(c);
        }
    }

    internal class StreamWriterWrapper : IStringWriter
    {
        public StreamWriterWrapper(StreamWriter streamWriter)
        {
            this.StreamWriter = streamWriter;
        }

        public StreamWriter StreamWriter { get; set; }
        public void WriteLine()
        {
            this.StreamWriter.WriteLine();
        }

        public void Write(string str)
        {
            this.StreamWriter.Write(str);
        }

        public void Write(char c)
        {
            this.StreamWriter.Write(c);
        }
    }

}
