using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTTP_Server.HTTP.Message
{
    public class Header
    {
        public string HttpVersion = "HTTP/2.0";
        public string RequestURI = "";
        public StatusCodes StatusCode = StatusCodes.OK;
        public Methods Method = Methods.GET;
        public string ContentType { get; private set; } = "text/html";
        public string CharSet { get; private set; } = "utf-8";
        public long ContentLength;
        public bool IsRequest = false;
        public string[] RequestHeaders;
        public ContentTypes SetContentType
        {
            set
            {
                switch (value)
                {
                    case ContentTypes.CSS:
                        ContentType = "text/css";
                        break;
                    case ContentTypes.HTML:
                        ContentType = "text/html";
                        break;
                    case ContentTypes.JAVASCRIPT:
                        ContentType = "text/html";
                        break;
                    case ContentTypes.XML:
                        ContentType = "text/xml";
                        break;
                    case ContentTypes.IMG:
                        ContentType = "image/png";
                        break;
                }
            }
        }
        public Charset SetCharSet
        {
            set
            {
                switch (value)
                {
                    case Charset.UNICODE:
                    case Charset.ASCII:
                        CharSet = value.ToString().ToLower();
                        break;
                    case Charset.UTF32:
                        CharSet = "utf-32";
                        break;
                    case Charset.UTF7:
                        CharSet = "utf-7";
                        break;
                    case Charset.UTF8:
                        CharSet = "utf-8";
                        break;
                }
            }
        }

        public Header() { }

        public enum StatusCodes
        {
            OK = 200,
            NOT_FOUND = 404,
            FORBIDDEN = 403,
            I_AM_A_TEAPOT = 418,
        }

        public enum ContentTypes
        {
            HTML,
            CSS,
            XML,
            JAVASCRIPT,
            IMG,
        }
        public enum Charset
        {
            UTF8,
            ASCII,
            UTF7,
            UNICODE,
            UTF32
        }
        public enum Methods
        {
            GET,
            DELETE,
            PUT,
            POST,
        }
    }
}
