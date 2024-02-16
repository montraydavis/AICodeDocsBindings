namespace AICodeDocsBindings.Generator
{
    using Codelyzer.Analysis.Model;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a source file definition.
    /// </summary>
    public class SourceFileDefinition
    {
        /// <summary>
        /// Gets the class information.
        /// </summary>
        public ClassDeclaration ClassInfo { get; }

        /// <summary>
        /// Gets the array of method definitions.
        /// </summary>
        public MethodDefinition[] Methods { get; }

        /// <summary>
        /// Gets the array of class property definitions.
        /// </summary>
        public ClassPropertyDefinition[] Properties { get; }

        private void GetProperties(ClassDeclarationSyntax classDeclaration, List<PropertyDeclarationSyntax> properties)
        {
            foreach (var member in classDeclaration.Members)
            {
                if (member is PropertyDeclarationSyntax property)
                {
                    properties.Add(property);
                }
                else if (member is ClassDeclarationSyntax nestedClass)
                {
                    GetProperties(nestedClass, properties);
                }
            }
        }

        private void GetMethods(ClassDeclarationSyntax classDeclaration, List<MethodDeclarationSyntax> methods)
        {
            foreach (var member in classDeclaration.Members)
            {
                if (member is MethodDeclarationSyntax method)
                {
                    methods.Add(method);
                }
                else if (member is ClassDeclarationSyntax nestedClass)
                {
                    GetMethods(nestedClass, methods);
                }
            }
        }

        /// <summary>
        /// Gets the list of property declaration syntax nodes within a class.
        /// </summary>
        /// <param name="classDeclaration">The class declaration syntax node.</param>
        /// <returns>The list of property declaration syntax nodes.</returns>
        public List<PropertyDeclarationSyntax> GetPropertiesWithinClass(ClassDeclarationSyntax classDeclaration)
        {
            var properties = new List<PropertyDeclarationSyntax>();
            GetProperties(classDeclaration, properties);
            return properties;
        }

        /// <summary>
        /// Gets the list of method declaration syntax nodes within a class.
        /// </summary>
        /// <param name="classDeclaration">The class declaration syntax node.</param>
        /// <returns>The list of method declaration syntax nodes.</returns>
        public List<MethodDeclarationSyntax> GetMethodsWithinClass(ClassDeclarationSyntax classDeclaration)
        {
            var properties = new List<MethodDeclarationSyntax>();
            GetMethods(classDeclaration, properties);
            return properties;
        }

        private ClassDeclarationSyntax GetClassDeclaration(SyntaxTree syntaxTree)
        {
            var root = syntaxTree.GetRoot();
            var classDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();

            return classDeclaration
                ?? throw new Exception("Could not find class declaration in source file.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceFileDefinition"/> class.
        /// </summary>
        /// <param name="analyzerResult">The analyzer result.</param>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="sourceFIleLines">The source file lines.</param>
        /// <param name="classDeclaration">The class declaration.</param>
        public SourceFileDefinition(Codelyzer.Analysis.Model.AnalyzerResult analyzerResult, string sourceFilePath, string[] sourceFIleLines, ClassDeclaration classDeclaration)
        {
            this.ClassInfo = classDeclaration;
            var rootSyntaxTree = analyzerResult
                .ProjectBuildResult
                .SourceFileBuildResults
                .First(r => r.SourceFileFullPath == sourceFilePath)
                .SyntaxTree;

            var root = rootSyntaxTree
                .GetCompilationUnitRoot();

            var jsonSerializerSettings = new Newtonsoft.Json.JsonSerializerSettings() { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore };

            this.Methods = GetMethodsWithinClass(GetClassDeclaration(rootSyntaxTree))
                .Select(m =>
                {

                    var methodParameters = m.ParameterList.Parameters.Select(p => new PropertyDefinition(p.Type?.GetText().ToString() ?? "Unknown", p.Identifier.Text, false, false, false, false, false, false, false)).ToArray();
                    return new MethodDefinition(m.Identifier.Text, methodParameters);
                })
                .ToArray();

            Properties = GetPropertiesWithinClass(GetClassDeclaration(rootSyntaxTree))
                .Select(property => new ClassPropertyDefinition(
                    property.Type?.GetText().ToString() ?? "Unknown",
                    property.Identifier.Text,
                    property.Modifiers.Any(SyntaxKind.PublicKeyword),
                    property.Modifiers.Any(SyntaxKind.StaticKeyword),
                    property.Modifiers.Any(SyntaxKind.AbstractKeyword),
                    property.Modifiers.Any(SyntaxKind.FieldKeyword),
                    property.Modifiers.Any(SyntaxKind.VirtualKeyword),
                    property.Modifiers.Any(SyntaxKind.OverrideKeyword),
                    property.Modifiers.Any(SyntaxKind.ReturnKeyword)))
                .ToArray();

        }
    }
}
