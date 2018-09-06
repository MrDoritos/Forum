using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HTTP_Server.HTTP.Message;

namespace HTTP_Server.HTTP
{
    public class HttpRequest
    {
        private static string[] split = new string[] { "\r\n\r\n" };
        private static string[] splitHeader = new string[] { "\r\n" };

        public Header Header { get; private set; }
        public Content Content { get; private set; }
        public Form Form { get; private set; }
        public HttpRequest(Header Header, Content Content, Form Form) { this.Header = Header; this.Content = Content; this.Form = Form; }
        public HttpRequest(Header Header, Content Content) { this.Header = Header; this.Content = Content; }

        public static HttpRequest Parse(byte[] buffer)
        {
            if (buffer == null || buffer.Length < 1) { return null; }
            string body = Encoding.UTF8.GetString(buffer);
            string header = body.Split(split, StringSplitOptions.None).First();
            byte[] content = Encoding.UTF8.GetBytes(body.Split(split, StringSplitOptions.None).Last());
            Header headerTemplate = new Header();
            Content contentTemplate = new Content() { Buffer = content };
            Form form = new Form();
            string[] splitHead = header.Split(splitHeader, StringSplitOptions.None);
            Enum.TryParse(splitHead[0].Split(' ')[0].ToUpper(), out Header.Methods method);
            headerTemplate.RequestURI = splitHead[0].Split(' ')[1];
            headerTemplate.Method = method;
            headerTemplate.RequestHeaders = splitHead;
            headerTemplate.IsRequest = true;

            if (method == Header.Methods.POST) { form = Form.TryParse(Encoding.UTF8.GetString(content)); return new HttpRequest(headerTemplate, contentTemplate, form); } else
            return new HttpRequest(headerTemplate, contentTemplate);
        }
    }
}
