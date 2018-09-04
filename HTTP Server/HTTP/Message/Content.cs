using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTTP_Server.HTTP.Message
{
    public class Content
    {
        public long ContentLength
        {
            get
            {
                return Buffer.Length;
            }
        }

        public byte[] Buffer = new byte[0];
    }
}
