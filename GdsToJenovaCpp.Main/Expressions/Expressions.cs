public static class Expressions
{
    public static List<string> GodotUtilities = new List<string>()
    {
        { "randf_range" }
    };

    public static class Jenova
    {
        public static string Comment = "//";
        public static string LeftBracket = "{";
        public static string RightBracket = "}";
        public static string EndOfCommand = ";";
    }

    public static class GdScript
    {
        public static string Comment = "#";
        public static string Indent = "\t";
    }

    public static class ParserInterLang
    {
        public static string LeftCppBracketEscaped = "ESCLBracket";
        public static string RightCppBracketEscaped = "ESCRBracket";
        public static string EndOfParsedRegion = "CLOSURE";
        public static string RightBracketCleanup = "END__";
        public static string RightBracketCleanup2 = "END2__";
        public static string RightBracketCleanup3 = "END3__";
        public static string RightBracketCleanup4 = "END4__";

    }

    public static string SpaceChar = " ";
    public static string NewLine = "\r\n";
}
