namespace SCAuditStudio
{
    public class CodeSnippet
    {
        public string language;
        public string code;

        public CodeSnippet()
        {
            language = "undefined";
            code = "";
        }

        public CodeSnippet(string code, string language = "undefined")
        {
            this.code = code;
            this.language = language;
        }

        public static CodeSnippet Empty
        {
            get { return new CodeSnippet(); }
        }
    }
}
