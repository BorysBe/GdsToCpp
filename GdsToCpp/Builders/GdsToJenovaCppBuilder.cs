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

        public GdsToJenovaCppBuilder ReplaceMethods()
        {
            _code.Replace("func ", "void ");
            ReplaceMethodHeaderForVoid();
            ReplaceBrackets();
            return this;
        }

        private void ReplaceBrackets()
        {
            _code = new StringBuilder(Regex.Replace(_code.ToString(), @"(\w+)\.(\w+)", "$1->$2"));
            string[] lines = _code.ToString().Split("\r\n");
            var linesCount = lines.Count();
            bool openBracketSpotted = false;
            bool closedBracketSpotted = false;
            for (var idx = 0; idx < linesCount - 1; idx++)
            {
                if (lines[idx].Contains("{"))
                    openBracketSpotted = true;
                if (lines[idx].Contains("}"))
                {
                    openBracketSpotted = false;
                    closedBracketSpotted = true;
                }

                if (openBracketSpotted && lines[idx] == "")
                {
                    lines[idx] = "}\r\n";
                }
            }
            var codeAgain = string.Empty;
            for (var idx = 1; idx < linesCount - 1; idx++)
            {
                codeAgain += lines[idx] + "\r\n";
            }
            _code = new StringBuilder(codeAgain);
        }

        private void ReplaceMethodHeaderForVoid()
        {
            _code.Replace(" -> void:\r\n\t", "\r\n{\r\n\t");
            _code.Replace("-> void:\r\n\t", "\r\n{\r\n\t");
            _code.Replace(" ->void:\r\n\t", "\r\n{\r\n\t");
            _code.Replace(":\r\n\t", "\r\n{\r\n\t");
        }
    }
}
