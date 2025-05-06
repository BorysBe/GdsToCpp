using System.Text;

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
    }
}
