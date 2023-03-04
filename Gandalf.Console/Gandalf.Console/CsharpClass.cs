using Microsoft.CodeAnalysis.Text;

namespace Gandalf
{
    public class CsharpClass
    {
        public List<CsSharpMethod> Methods = new List<CsSharpMethod>();
        public List<string> Members = new List<string>();
        public string Name { get; set; }

        public string Namespace { get; set; }

        public List<CsharpProperty> Properties { get; set; }

        public string PrimaryKeyType { get; set; }

        public class CsharpProperty
        {
            public string Name { get; set; }
            public string Type { get; set; }

            public CsharpProperty(string name, string type)
            {
                Name = name;
                Type = type;
            }
        }

        public CsharpClass()
        {
            Properties = new List<CsharpProperty>();
        }
    }
    public class CsSharpMethod
    {
        public LinePositionSpan Span;
        public string Name;
        public string Body;
    }
}
