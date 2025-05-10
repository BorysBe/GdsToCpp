using System.Text;
using System.Text.RegularExpressions;

namespace GdsToJenovaCpp.Builders
{
    public partial class GdsToJenovaCppBuilder
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
            _code.Replace("{", "ESCLBracket");
            _code.Replace(":=", "=");
            ReplaceMethodHeaderForVoid();
            ReplaceBrackets();
            _code.Replace("ESCLBracket", "{");
            return this;
        }

        private void ReplaceBrackets()
        {
            string rebuilt = RebuiltStringWithOpenBracketsGeneratedOnTheFly();
            ForVariablesStoringMemoryAddressOfAnObjectUseArrowOperator();
            List<string> lines = ReformatToLinesList();
            string codeAgain = PutClosingBrackets(lines);
            codeAgain = CleanupBracketErrors(codeAgain);
            _code = new StringBuilder(codeAgain.Trim());
        }

        private static string CleanupBracketErrors(string codeAgain)
        {
            var codeBuilder = new StringBuilder(codeAgain + "CLOSURE");
            codeBuilder.Replace("{;", "{");
            codeBuilder.Replace("\t;", "}\r\n");
            codeBuilder.Replace("}\r\n\r\nCLOSURE", "");
            codeBuilder.Replace("}\r\nCLOSURE", "");

            return codeBuilder.ToString();
        }

        private static string PutClosingBrackets(List<string> lines)
        {
            var linesCount = lines.Count();
            bool openBracketSpotted = false;
            bool closedBracketSpotted = false;
            for (var idx = 0; idx < linesCount; idx++)
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
                else
                    if (openBracketSpotted && lines[idx] != "" && !lines[idx].Contains("void"))
                {
                    lines[idx] = lines[idx] + ";";
                }
            }
            var codeAgain = string.Empty;
            for (var idx = 0; idx < linesCount - 1; idx++)
            {
                codeAgain += lines[idx] + "\r\n";
            }

            return codeAgain;
        }

        private List<string> ReformatToLinesList()
        {
            List<string> lines = _code.ToString().Split("\r\n", StringSplitOptions.None).ToList();
            lines.Add("\r\n");
            return lines;
        }

        private string RebuiltStringWithOpenBracketsGeneratedOnTheFly()
        {
            return _code.ToString();
        }

        private void ForVariablesStoringMemoryAddressOfAnObjectUseArrowOperator()
        {
            _code.Replace("().", "()->");
        }

        private void ReplaceMethodHeaderForVoid()
        {
            _code.Replace(" -> void:\r\n\t", "\r\n{\r\n\t");
            _code.Replace("-> void:\r\n\t", "\r\n{\r\n\t");
            _code.Replace(" ->void:\r\n\t", "\r\n{\r\n\t");
            _code.Replace(":\r\n\t", "\r\n{\r\n\t");
        }

        [GeneratedRegex(@"(\w+)\s*\(\s*\)")]
        private static partial Regex ApplyArrowOperator();
    }
}
