using System.Text;
using System.Text.RegularExpressions;

namespace GdsToJenovaCpp.Main.Builders
{
    public partial class GdsToJenovaCppBuilder
    {
        private StringBuilder _code = new StringBuilder();

        public GdsToJenovaCppBuilder(string content)
        {
            _code = new StringBuilder(content);
        }

        public string Build()
        {
            return _code.ToString();
        }

        public GdsToJenovaCppBuilder ReplaceComments()
        {
            _code.Replace(Expressions.GdScript.Comment, Expressions.Jenova.Comment + Expressions.SpaceChar);
            return this;
        }

        public GdsToJenovaCppBuilder ReplaceMethods()
        {
            AddFloatTypeToAllFuncParams();
            _code.Replace(Expressions.Jenova.LeftBracket, Expressions.ParserInterLang.LeftCppBracketEscaped);
            _code.Replace(Expressions.Jenova.RightBracket, Expressions.ParserInterLang.RightCppBracketEscaped);
            PutBracketsForIfStatements();
            _code.Replace("func ", "void ");
            _code.Replace(":=", "=");
            ReplaceMethodHeaderForVoid();
            ForVariablesStoringMemoryAddressOfAnObjectUseArrowOperator();
            ReverseParametersOrderInFunctionDeclarationBrackets();
            _code.Replace(Expressions.ParserInterLang.LeftCppBracketEscaped, Expressions.Jenova.LeftBracket);
            _code.Replace(Expressions.ParserInterLang.RightCppBracketEscaped, Expressions.Jenova.RightBracket);
            return this;
        }

        private void PutBracketsForIfStatements()
        {
            var lines = LoadAsCodeLineList();
            lines.Append("");
            var previousIndentCount = 0;
            bool previouslySpottedFunc = false;

            for (int idx = 0; idx < lines.Count; idx++)
            {
                var input = lines[idx];

                string pattern = @"^(\s*)(if|elif)\s+(.*?):";
                Regex regex = new Regex(pattern, RegexOptions.Multiline);
                var leadingIntend = "";
                if (previousIndentCount > 0 && input.Length == 0)
                {
                    for (int i = 0; i < previousIndentCount - 1; i++)
                    {
                        leadingIntend += "\t";
                    }

                    if (previouslySpottedFunc)
                    {
                        lines[idx] = leadingIntend + "}\r\n";
                        previousIndentCount = 0;
                    }
                    continue;
                }
                var trim = lines[idx].Trim().Length;
                var lgth = lines[idx].Length;
                previousIndentCount = lgth - trim;

                string output = regex.Replace(input, match =>
                {
                    string leadingIntend = match.Groups[1].Value;
                    if (match.Groups[2].Value == "if")
                    {
                         return $"{leadingIntend}if ({match.Groups[3].Value})\n{leadingIntend}{{{leadingIntend}";
                    }
                    else if (match.Groups[2].Value == "elif")
                    {
                        return $"{leadingIntend}}}\r\n{leadingIntend}else\r\n{leadingIntend}if ({match.Groups[3].Value})\n{leadingIntend}{{{leadingIntend}";
                    }

                    return input;
                });
                if (input.Trim().StartsWith("func"))
                {
                    if (previouslySpottedFunc)
                    {
                        lines[idx] =  $"{leadingIntend}\r\n}}\r\n\r\n" + FixSemicolon(lines[idx]);
                        previouslySpottedFunc = false;
                    }
                    lines[idx] = FixSemicolon(lines[idx]) + $"\r\n{leadingIntend}{{";
                    previouslySpottedFunc = true;

                }
                else
                    lines[idx] = FixSemicolon(output);
            }

            var str = string.Join(Expressions.NewLine, lines).Trim();
            _code = new StringBuilder(str);
        }

        public GdsToJenovaCppBuilder AddGodotFunctionsUtilitiesHeader()
        {
            var addUtilities = false;
            var code = _code.ToString();
            foreach (var godotFunc in Expressions.GodotUtilities)
            {
                addUtilities = code.Contains(godotFunc);
                if (addUtilities)
                {
                    _code.Insert(0, "#include <Godot/variant/utility_functions.hpp>" + Expressions.NewLine);
                    _code.Replace(godotFunc, $"UtilityFunctions::{godotFunc}");
                    break;
                }
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
            string pattern = @"^\s*func\s+(\w+)\s*\(([^)]*)\)";
            Match match = Regex.Match(input.Trim(), pattern);

            if (!match.Success)
                return input;

            string funcName = match.Groups[1].Value;
            string paramsSection = match.Groups[2].Value;
            if (paramsSection == "")
                return input;

            string[] paramList = paramsSection.Split(',');
            for (int i = 0; i < paramList.Length; i++)
            {
                string param = paramList[i].Trim();
                if (param.Contains(":")) 
                    continue;

                paramList[i] = $"float {param}";
            }

            string updatedParams = string.Join(", ", paramList);
            if (!updatedParams.HasValue())
                return input;

            return $"func {funcName}({updatedParams}):";
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
            paramList.Add(paramItem);

            for (int i = 0; i < paramList.Count; i++)
            {
                string param = paramList[i];

                if (param.Contains(":"))
                {
                    string[] parts = param.Split(':');
                    paramList[i] = $"{parts[1].Trim()} {parts[0].Trim()}";
                }
                else if (!param.Contains("("))
                {
                    paramList[i] = param.Replace(",", ", ");
                }
            }

            return string.Join(", ", paramList);
        }

        private static string FixSemicolon(string line)
        {
            string trimmedLine = line.Trim();
            if (trimmedLine.Length == 0) 
                return line;

            bool needsSemicolon = (!trimmedLine.EndsWith("}") &&
                                  !trimmedLine.EndsWith("{") &&
                                  !trimmedLine.EndsWith("]") &&
                                  !trimmedLine.EndsWith("[") &&
                                  !trimmedLine.EndsWith("else") &&
                                  !trimmedLine.EndsWith(",") &&
                                  !string.IsNullOrWhiteSpace(trimmedLine) &&
                                  !trimmedLine.EndsWith(Expressions.Jenova.LeftBracket) && !trimmedLine.EndsWith(Expressions.ParserInterLang.LeftCppBracketEscaped)) && !trimmedLine.EndsWith("}") &&
                                  !trimmedLine.EndsWith(";") &&
                                  !Regex.IsMatch(trimmedLine, @"^\s*(if|while|for|func|#include)\b") &&
                                  !Regex.IsMatch(trimmedLine, @"^}\s*(if|while|for|func|#include)\b");

            return needsSemicolon ? line + ";" : line;
        }

        private static string MakeCodeAgainFrom(List<string> lines)
        {
            var codeAgain = string.Empty;
            for (var idx = 0; idx < lines.Count; idx++)
            {
                codeAgain += lines[idx] + Expressions.NewLine;
            }

            return codeAgain;
        }

        private List<string> LoadAsCodeLineList()
        {
            var code = _code.ToString();
            List<string> lines = code.Split(Expressions.NewLine, StringSplitOptions.None).ToList();
            return lines;
        }

        private void ForVariablesStoringMemoryAddressOfAnObjectUseArrowOperator()
        {
            _code.Replace("().", "()->");
        }

        private void ReplaceMethodHeaderForVoid()
        {
            _code.Replace($" -> void:", "");
            _code.Replace($"-> void:", "");
            _code.Replace($" ->void:", "");
            _code.Replace($"):", $")");
        }

        [GeneratedRegex(@"(\w+)\s*\(\s*\)")]
        private static partial Regex ApplyArrowOperator();
    }
}
