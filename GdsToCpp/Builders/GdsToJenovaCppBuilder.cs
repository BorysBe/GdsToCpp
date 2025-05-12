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

        private void ReplaceBrackets()
        {
            string rebuilt = _code.ToString();
            ForVariablesStoringMemoryAddressOfAnObjectUseArrowOperator();
            List<string> lines = LoadAsCodeLineList();
            string codeAgain = PutClosingBrackets(lines);
            codeAgain = CleanupBracketErrors(codeAgain);
            _code = new StringBuilder(codeAgain.Trim());
        }

        private static string CleanupBracketErrors(string codeAgain)
        {
            codeAgain = codeAgain.Replace($"{Expressions.NewLine}{Expressions.GdScript.Indent}{Expressions.NewLine}", $"{Expressions.NewLine}");
            return codeAgain;
        }

        private static string PutClosingBrackets(List<string> lines)
        {
            Stack<int> openBracketsIndices = new Stack<int>();
            List<string> updatedLines = new List<string>();
            bool insideBlock = false;

            for (int idx = 0; idx < lines.Count; idx++)
            {
                string currentLine = lines[idx].Trim();
                bool isFunctionStart = Regex.IsMatch(currentLine, @"^\s*\w+\s+\w+\s*\(.*\)$");

                if (isFunctionStart)
                {
                    insideBlock = true;
                    updatedLines.Add(currentLine);

                    if (idx + 1 < lines.Count && !lines[idx + 1].Trim().StartsWith(Expressions.Jenova.LeftBracket))
                    {
                        updatedLines.Add(Expressions.Jenova.LeftBracket);
                    }
                    openBracketsIndices.Push(updatedLines.Count - 1);
                }
                else if (insideBlock)
                {
                    if (lines[idx] != Expressions.Jenova.LeftBracket)
                    {
                        if (!lines[idx].StartsWith("\t"))
                            updatedLines.Add("\t" + FixSemicolon(lines[idx]));
                        else
                            updatedLines.Add(FixSemicolon(lines[idx]));
                    }
                    else
                        updatedLines.Add(lines[idx]);
                }
                else
                {
                    updatedLines.Add(lines[idx]);
                }

                bool isNewFunctionComing = idx + 1 < lines.Count && Regex.IsMatch(lines[idx + 1].Trim(), @"^\s*\w+\s+\w+\s*\(.*\)$");

                if (openBracketsIndices.Count > 0 && isNewFunctionComing)
                {
                    if (!updatedLines.Last().Trim().EndsWith(Expressions.Jenova.RightBracket))
                    {
                        updatedLines.Add(Expressions.Jenova.RightBracket);
                    }
                    updatedLines.Add("");
                    insideBlock = false;
                    openBracketsIndices.Pop();
                }
            }

            while (openBracketsIndices.Count > 0)
            {
                updatedLines.Add(Expressions.Jenova.RightBracket);
                openBracketsIndices.Pop();
            }

            return string.Join(Expressions.NewLine, updatedLines);
        }

        private static string FixSemicolon(string line)
        {
            string trimmedLine = line.Trim();
            if (trimmedLine.Length == 0) 
                return line;

            bool needsSemicolon = trimmedLine.Contains("return ") ||
                                  !string.IsNullOrWhiteSpace(trimmedLine) &&
                                  !trimmedLine.EndsWith("{") && !trimmedLine.EndsWith("}") &&
                                  !trimmedLine.EndsWith(";") &&
                                  !Regex.IsMatch(trimmedLine, @"^\s*(if|while|for|return)\b");

            return needsSemicolon ? "\t" + trimmedLine + ";" : "\t" + trimmedLine;
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
            var test2 = _code.ToString();
            var newMethodSyntaxBracketSection = $"{Expressions.NewLine}{Expressions.Jenova.LeftBracket}{Expressions.NewLine}{Expressions.GdScript.Indent}";
            _code.Replace($" -> void:{Expressions.NewLine}{Expressions.GdScript.Indent}", $"{newMethodSyntaxBracketSection}");
            _code.Replace($"-> void:{Expressions.NewLine}{Expressions.GdScript.Indent}", $"{newMethodSyntaxBracketSection}");
            _code.Replace($" ->void:{Expressions.NewLine}{Expressions.GdScript.Indent}", $"{newMethodSyntaxBracketSection}");
            _code.Replace($"):{Expressions.NewLine}{Expressions.GdScript.Indent}", $"){newMethodSyntaxBracketSection}");
            var test = _code.ToString();
        }

        [GeneratedRegex(@"(\w+)\s*\(\s*\)")]
        private static partial Regex ApplyArrowOperator();
    }
}
