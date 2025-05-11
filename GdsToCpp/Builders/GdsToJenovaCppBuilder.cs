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
            _code.Replace(Expressions.GdScript.Comment, Expressions.Jenova.Comment + Expressions.SpaceChar);
            return this;
        }

        public string Build()
        {
            return _code.ToString();
        }

        public GdsToJenovaCppBuilder ReplaceMethods()
        {
            _code.Replace("func ", "void ");
            _code.Replace(Expressions.Jenova.LeftBracket, Expressions.ParserInterLang.LeftCppBracketEscaped);
            _code.Replace(":=", "=");
            ReplaceMethodHeaderForVoid();
            ReplaceBrackets();
            _code.Replace(Expressions.ParserInterLang.LeftCppBracketEscaped, Expressions.Jenova.LeftBracket);
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
            var codeBuilder = new StringBuilder(codeAgain + Expressions.ParserInterLang.EndOfParsedRegion);
            codeBuilder.Replace($"{Expressions.Jenova.LeftBracket}{Expressions.Jenova.EndOfCommand}", $"{Expressions.Jenova.LeftBracket}");
            codeBuilder.Replace($"{Expressions.GdScript.Indent}{Expressions.ParserInterLang.EndOfParsedRegion}", $"{Expressions.Jenova.RightBracket}{Expressions.NewLine}");
            codeBuilder.Replace($"{Expressions.Jenova.RightBracket}{Expressions.NewLine}{Expressions.NewLine}{Expressions.ParserInterLang.EndOfParsedRegion}", "");
            codeBuilder.Replace($"{Expressions.Jenova.RightBracket}{Expressions.NewLine}{Expressions.ParserInterLang.EndOfParsedRegion}", "");

            return codeBuilder.ToString();
        }

        private static string PutClosingBrackets(List<string> lines)
        {
            var linesCount = lines.Count();
            bool openBracketSpotted = false;
            bool closedBracketSpotted = false;
            for (var idx = 0; idx < linesCount; idx++)
            {
                if (lines[idx].Contains($"{Expressions.Jenova.LeftBracket}"))
                    openBracketSpotted = true;
                if (lines[idx].Contains($"{Expressions.Jenova.RightBracket}"))
                {
                    openBracketSpotted = false;
                    closedBracketSpotted = true;
                }

                if (openBracketSpotted && lines[idx] == "")
                {
                    lines[idx] = $"{Expressions.Jenova.RightBracket}{Expressions.NewLine}";
                }
                else
                    if (openBracketSpotted && lines[idx] != "" && !lines[idx].Contains("void"))
                {
                    lines[idx] = lines[idx] + Expressions.Jenova.EndOfCommand;
                }
            }
            var codeAgain = string.Empty;
            for (var idx = 0; idx < linesCount - 1; idx++)
            {
                codeAgain += lines[idx] + Expressions.NewLine;
            }

            return codeAgain;
        }

        private List<string> ReformatToLinesList()
        {
            List<string> lines = _code.ToString().Split(Expressions.NewLine, StringSplitOptions.None).ToList();
            lines.Add(Expressions.NewLine);
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
