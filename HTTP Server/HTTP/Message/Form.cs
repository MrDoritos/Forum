using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;

namespace HTTP_Server.HTTP.Message
{
    public class Form
    {
        public enum FormType
        {
            URLENCODEDFORM,
            MULTIPARTFORMDATA,
        }

        public IEnumerable<Forms.FormVariable<string>> formVariables;

        public static Form TryParse(string content, FormType formType = FormType.URLENCODEDFORM)
        {
            Form form = new Form();
            switch (formType)
            {
                case FormType.URLENCODEDFORM:
                    form = GetURLEncodedFormData(content);
                    break;
                case FormType.MULTIPARTFORMDATA:
                    break;
            }
            return form;
        }

        private static Form GetURLEncodedFormData(string content)
        {
            Form form = new Form();
            List<Forms.FormVariable<string>> formVariables = new List<Forms.FormVariable<string>>();
            foreach (var value in content.Split('&'))
            {
                Forms.FormVariable<string> formVariable;
                var keyvalue = value.Split('=');
                if (keyvalue.Length > 1)
                    formVariable = new Forms.FormVariable<string>(HttpUtility.UrlDecode(keyvalue[0]), HttpUtility.UrlDecode(keyvalue[1]));
                else
                    formVariable = new Forms.FormVariable<string>(HttpUtility.UrlDecode(keyvalue[0]), "");
                formVariables.Add(formVariable);
            }
            form.formVariables = formVariables;
            return form;
        }

        private static void GetMultiPartFormData() { }
    }
}
