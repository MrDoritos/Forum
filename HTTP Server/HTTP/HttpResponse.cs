using System.Text;
using HTTP_Server.HTTP.Message;

namespace HTTP_Server.HTTP
{
    public class HttpResponse
    {
        private Header _header;
        public Header Header
        {
            get
            {
                _header.ContentLength = Content.ContentLength;
                return _header;
            }
            set { _header = value; }
        }
        public Content Content = new Content();

        public byte[] Serialize()
        {
            if (Header.IsRequest) { return new byte[0]; }
            string header = $"{Header.HttpVersion} {(int)Header.StatusCode} {Header.StatusCode.ToString()}\r\n";
            if (Header.ContentType == "image/png")
            {
                header += $"content-type: {Header.ContentType}\r\ncontent-length: {Header.ContentLength}\r\n\r\n";
            }
            else
            {
                header += $"content-type: {Header.ContentType}; charset={Header.CharSet}\r\ncontent-length: {Header.ContentLength}\r\n\r\n";
            }
            return Encoding.UTF8.GetBytes(header).Append(Content.Buffer);
        }
    }
}
