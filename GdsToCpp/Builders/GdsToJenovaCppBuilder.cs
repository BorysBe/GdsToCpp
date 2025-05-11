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
            AddFloatTypeToAllFuncParams();
            _code.Replace("func ", "void ");
            _code.Replace(Expressions.Jenova.LeftBracket, Expressions.ParserInterLang.LeftCppBracketEscaped);
            _code.Replace(":=", "=");
            ReplaceMethodHeaderForVoid();
            ReplaceBrackets();
            ReverseParametersOrderInFunctionDeclarationBrackets();
            _code.Replace(Expressions.ParserInterLang.LeftCppBracketEscaped, Expressions.Jenova.LeftBracket);
            return this;
        }

        public GdsToJenovaCppBuilder AddGodotFunctionsUtilitiesHeader()
        {
            var addUtilities = false;
            var code = _code.ToString();
            foreach (var godotFunc in Expressions.GodotUtilities)
            {
                addUtilities = code.Contains(godotFunc);
                if (addUtilities)
                    _code.Insert(0, "#include <Godot/variant/utility_functions.hpp>" + Expressions.NewLine);
                    _code.Replace(godotFunc, $"UtilityFunctions::{godotFunc}");
                    break;
            }
            return this;
        }

        private void AddFloatTypeToAllFuncParams()
        {
            var lines = LoadAsCodeLineList();
            for (int idx = 0; idx < lines.Count; idx++)
            {
                lines[idx] = AddFloatTypeToFuncParams(lines[idx]);
            }
            string codeAgain = MakeCodeAgainFrom(lines);
            _code = new StringBuilder(codeAgain);
        }

        static string AddFloatTypeToFuncParams(string input)
        {
            string pattern = @"^\s*func\s+(\w+)\s*\(([^)]*)\)"; // Dopasowanie definicji funkcji
            Match match = Regex.Match(input.Trim(), pattern);

            if (!match.Success) return input; // Jeśli nie pasuje do funkcji, zwraca oryginalny string

            string funcName = match.Groups[1].Value;
            string paramsSection = match.Groups[2].Value;

            // Podziel parametry na poszczególne elementy
            string[] paramList = paramsSection.Split(',');

            for (int i = 0; i < paramList.Length; i++)
            {
                string param = paramList[i].Trim();

                // Jeśli parametr już zawiera ":" (np. "area: Area2D"), zostawiamy go nietkniętym
                if (param.Contains(":")) continue;

                // W przeciwnym razie dodajemy "float" przed nazwą zmiennej
                paramList[i] = $"float {param}";
            }

            // Połącz poprawione parametry w nowy string
            string updatedParams = string.Join(", ", paramList);

            if (!updatedParams.HasValue())
                return input;

            return $"func {funcName}({updatedParams})";
        }

        /// <summary>
        /// Replacing GDScript format
        /// "(area: Area2D, area2: Area2D)" into "(Area2D area, Area2D area2)"
        /// </summary>
        private void ReverseParametersOrderInFunctionDeclarationBrackets()
        {
            string input = _code.ToString();
            string pattern = @"\(([^)]*)\)"; // Dopasowanie zawartości nawiasów

            string replaced = Regex.Replace(input, pattern, match =>
            {
                string innerText = match.Groups[1].Value;
                return $"({ProcessParametersRecursively(innerText)})"; // Rekurencyjna analiza parametrów
            });

            _code = new StringBuilder(replaced);
        }

        static string ProcessParametersRecursively(string parameters)
        {
            var paramList = new List<string>();
            var buffer = new StringBuilder();
            int openBrackets = 0;

            foreach (char c in parameters)
            {
                if (c == '(') openBrackets++;
                if (c == ')') openBrackets--;

                if (c == ',' && openBrackets == 0)
                {
                    paramList.Add(buffer.ToString().Trim());
                    buffer.Clear();
                }
                else
                {
                    buffer.Append(c);
                }
            }
            var paramItem = buffer.ToString().Trim();
            paramItem = paramItem.Replace(",", ", ");
            paramList.Add(paramItem); // Dodaj ostatni parametr

            for (int i = 0; i < paramList.Count; i++)
            {
                string param = paramList[i];

                if (param.Contains(":")) // Jeśli parametr ma typ, odwróć jego kolejność
                {
                    string[] parts = param.Split(':');
                    paramList[i] = $"{parts[1].Trim()} {parts[0].Trim()}";
                }
                else if (!param.Contains("(")) // Jeśli parametr nie jest funkcją, rozdziel elementy poprawnie
                {
                    paramList[i] = param.Replace(",", ", ");
                }
            }

            return string.Join(", ", paramList);
        }

        private void ReplaceBrackets()
        {
            string rebuilt = RebuiltStringWithOpenBracketsGeneratedOnTheFly();
            ForVariablesStoringMemoryAddressOfAnObjectUseArrowOperator();
            List<string> lines = LoadAsCodeLineList();
            string codeAgain = PutClosingBrackets(lines);
            codeAgain = CleanupBracketErrors(codeAgain);
            _code = new StringBuilder(codeAgain.Trim());
        }

        private static string CleanupBracketErrors(string codeAgain)
        {
            var codeBuilder = new StringBuilder(codeAgain + Expressions.ParserInterLang.EndOfParsedRegion);
            codeBuilder.Replace($"{Expressions.Jenova.LeftBracket}{Expressions.Jenova.EndOfCommand}", $"{Expressions.Jenova.LeftBracket}");
            codeBuilder.Replace($"{Expressions.GdScript.Indent}{Expressions.Jenova.EndOfCommand}", $"{Expressions.Jenova.RightBracket}{Expressions.NewLine}");
            codeBuilder.Replace($"{Expressions.Jenova.RightBracket}{Expressions.NewLine}{Expressions.NewLine}{Expressions.ParserInterLang.EndOfParsedRegion}", "");
            codeBuilder.Replace($"{Expressions.Jenova.RightBracket}{Expressions.NewLine}{Expressions.ParserInterLang.EndOfParsedRegion}", "");
            codeBuilder.Replace($"{Expressions.NewLine}{Expressions.ParserInterLang.EndOfParsedRegion}", "");
            codeBuilder.Replace($"{Expressions.ParserInterLang.EndOfParsedRegion}", "");
            codeBuilder.Replace($"{Expressions.NewLine}{Expressions.NewLine}{Expressions.Jenova.EndOfCommand}", "");
            codeBuilder.Replace($"{Expressions.NewLine}{Expressions.Jenova.EndOfCommand}", "");

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
            string codeAgain = MakeCodeAgainFrom(lines);
            return codeAgain;
        }

        private static string MakeCodeAgainFrom(List<string> lines)
        {
            var codeAgain = string.Empty;
            for (var idx = 0; idx < lines.Count - 1; idx++)
            {
                codeAgain += lines[idx] + Expressions.NewLine;
            }

            return codeAgain;
        }

        private List<string> LoadAsCodeLineList()
        {
            List<string> lines = _code.ToString().Split(Expressions.NewLine, StringSplitOptions.None).ToList();
            lines.Add(Expressions.NewLine);
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
            var tst = _code.ToString();
            _code.Replace($" -> void:{Expressions.NewLine}{Expressions.GdScript.Indent}", $"{Expressions.NewLine}{Expressions.Jenova.LeftBracket}{Expressions.NewLine}{Expressions.GdScript.Indent}");
            _code.Replace($"-> void:{Expressions.NewLine}{Expressions.GdScript.Indent}", $"{Expressions.NewLine}{Expressions.Jenova.LeftBracket}{Expressions.NewLine}{Expressions.GdScript.Indent}");
            _code.Replace($" ->void:{Expressions.NewLine}{Expressions.GdScript.Indent}", $"{Expressions.NewLine}{Expressions.Jenova.LeftBracket}{Expressions.NewLine}{Expressions.GdScript.Indent}");
            _code.Replace($"){Expressions.NewLine}{Expressions.GdScript.Indent}", $"){Expressions.NewLine}{Expressions.Jenova.LeftBracket}{Expressions.NewLine}{Expressions.GdScript.Indent}");
            _code.Replace($":{Expressions.NewLine}{Expressions.GdScript.Indent}", $"{Expressions.NewLine}{Expressions.Jenova.LeftBracket}{Expressions.NewLine}{Expressions.GdScript.Indent}");
        }

        [GeneratedRegex(@"(\w+)\s*\(\s*\)")]
        private static partial Regex ApplyArrowOperator();
    }
}
