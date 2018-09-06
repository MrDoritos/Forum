using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTTP_Server.HTTP.Message.Forms
{
    public class FormVariable<T>
    {
        public string name;
        public T value;
        public FormVariable(string name, T value) { this.name = name; this.value = value; }
    }
}
