using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;

namespace Gandalf
{
    public static class CsharpClassParser
    {
        public static CsharpClass Parse(string content)
        {
            var cls = new CsharpClass();
            var tree = CSharpSyntaxTree.ParseText(content);
            var members = tree.GetRoot().DescendantNodes().OfType<MemberDeclarationSyntax>();

            foreach (var member in members)
            {
                if (member is PropertyDeclarationSyntax property)
                {
                    cls.Properties.Add(new CsharpClass.CsharpProperty(
                         property.Identifier.ValueText,
                         property.Type.ToString())

                     );
                    var full = member.GetText().ToString().Trim();

                    Console.WriteLine("prop: " + property.Identifier.ValueText + "   " + full);
                    cls.Members.Add(full.Trim());
                }
                if (member is ConstructorDeclarationSyntax ctr)
                {

                    var full = member.GetText().ToString().Trim();

                    Console.WriteLine("ctr: " + ctr.Identifier.ValueText + "   " + full);
                    cls.Members.Add(full.Substring(0, full.IndexOf('{')));
                }

                if (member is NamespaceDeclarationSyntax namespaceDeclaration)
                {
                    cls.Namespace = namespaceDeclaration.Name.ToString();
                }
                if (member is MethodDeclarationSyntax methodDeclaration)
                {
                    var line = tree.GetText().Lines.GetLinePositionSpan(member.Span);
                    //var startLn = member.GetText().Lines[0].LineNumber;
                    var full = member.GetText().ToString();
                    cls.Methods.Add(new CsSharpMethod()
                    {
                        Span = line,
                        Name = methodDeclaration.Identifier.ValueText,
                        Body = full
                    });
                    Console.WriteLine("Method: " + methodDeclaration.Identifier.ValueText);
                    Console.WriteLine("signature: " + full.Substring(0, full.IndexOf('{')));
                    cls.Members.Add(full.Substring(0, full.IndexOf('{')));
                }

                if (member is ClassDeclarationSyntax classDeclaration)
                {
                    cls.Name = classDeclaration.Identifier.ValueText;

                    cls.PrimaryKeyType = FindPrimaryKeyType(classDeclaration);
                }


            }


            return cls;
        }

        private static string FindPrimaryKeyType(ClassDeclarationSyntax classDeclaration)
        {
            if (classDeclaration == null)
            {
                return null;
            }

            if (classDeclaration.BaseList == null)
            {
                return null;
            }

            foreach (var baseClass in classDeclaration.BaseList.Types)
            {
                var match = Regex.Match(baseClass.Type.ToString(), @"<(.*?)>");
                if (match.Success)
                {
                    var primaryKey = match.Groups[1].Value;

                    /*if (AppConsts.PrimaryKeyTypes.Any(x => x.Value == primaryKey))
                    {
                        return primaryKey;
                    }*/
                }
            }

            return null;
        }
    }
}
