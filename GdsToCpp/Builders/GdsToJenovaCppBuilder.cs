using System.Text;
using System.Text.RegularExpressions;

namespace GdsToJenovaCpp.Builders
{
    public class GdsToJenovaCppBuilder
    {
        private StringBuilder _code = new StringBuilder();

        public GdsToJenovaCppBuilder(string content)
        {
            _code = new StringBuilder(content);
        }

        public GdsToJenovaCppBuilder ReplaceComments()
        {
            _code.Replace("#", "// ");
            return this;
        }

        public string Build()
        {
            return _code.ToString();
        }

        public GdsToJenovaCppBuilder TranslateMethods()
        {
            _code.Replace("func ", "void ");
            _code.Replace(":\r\n", "\r\n");
            _code.Replace("\r\n\t", "\r\n{\r\n\t");
            _code.Replace("\r\n\r\n", "\r\n}\r\n");
            _code = new StringBuilder(Regex.Replace(_code.ToString(), @"(\w+)\.(\w+)", "$1->$2"));

            return this;
        }
    }
}
