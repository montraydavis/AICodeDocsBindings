using Codelyzer.Analysis;
using Microsoft.Extensions.Logging;

namespace AICodeDocsBindings.Generator
{
    using Codelyzer.Analysis.Common;
    using Codelyzer.Analysis.Model;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Newtonsoft.Json;
    using System;
    using System.Linq;

    public static class AnalyzerExtensions
    {
        public static void WriteOutput<T>(this IEnumerable<T> obj, string outputPath)
        {

            var jsonl = obj.Select(o => JsonConvert.SerializeObject(o, Formatting.None, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));

            File.WriteAllLines(outputPath, jsonl);
        }
    }

    public class Program
    {
        static async Task Main(string[] args)
        {
            /* 1. Create logger object */
            var loggerFactory = LoggerFactory.Create(builder =>
                    builder.SetMinimumLevel(LogLevel.Debug).AddConsole());
            var logger = loggerFactory.CreateLogger("Analyzer")
                ?? throw new Exception("Failed to create logger.");
            var outputPath = $@"{Directory.GetCurrentDirectory()}";

            /* 2. Create Configuration settings */
            var configuration = new AnalyzerConfiguration(LanguageOptions.CSharp);
            configuration.ExportSettings.GenerateJsonOutput = true;
            configuration.ExportSettings.OutputPath = outputPath;

            configuration.MetaDataSettings.DeclarationNodes = true;
            configuration.MetaDataSettings.MemberAccess = true;
            configuration.MetaDataSettings.MethodInvocations = true;
            configuration.MetaDataSettings.LiteralExpressions = true;
            configuration.MetaDataSettings.LambdaMethods = true;
            configuration.MetaDataSettings.DeclarationNodes = true;
            configuration.MetaDataSettings.Annotations = true;
            configuration.MetaDataSettings.LocationData = true;
            configuration.MetaDataSettings.ReferenceData = true;
            configuration.MetaDataSettings.LoadBuildData = true;
            configuration.MetaDataSettings.InterfaceDeclarations = true;
            configuration.MetaDataSettings.EnumDeclarations = true;
            configuration.MetaDataSettings.StructDeclarations = true;
            configuration.MetaDataSettings.ReturnStatements = true;
            configuration.MetaDataSettings.InvocationArguments = true;
            configuration.MetaDataSettings.GenerateBinFiles = true;
            configuration.MetaDataSettings.ElementAccess = true;
            configuration.MetaDataSettings.MemberAccess = true;

            /* 3. Get Analyzer instance based on language */
            var analyzer = CodeAnalyzerFactory.GetAnalyzer(configuration, logger)
                ?? throw new Exception("Failed to analyze");

            /* 4. Analyze the project or solution */
            var projectPath = "C:\\Users\\montr\\Downloads\\httplib-2.0.16\\JumpKick.HttpLib\\JumpKick.HttpLib";
            var projectFilePath = $@"{projectPath}\JumpKick.HttpLib.csproj";
            var analyzerResult = await analyzer.AnalyzeProject(projectFilePath);



            Console.WriteLine("The results are exported to file : " + analyzerResult.OutputJsonFilePath);

            /* 5. Consume the results as model objects */
            var sourcefile = analyzerResult.ProjectResult.SourceFileResults.First();
            foreach (var invocation in sourcefile.AllInvocationExpressions())
            {
                Console.WriteLine(invocation.MethodName + ":" + invocation.SemanticMethodSignature);
            }

            //var sfr = analyzerResult.ProjectBuildResult.SourceFileBuildResults.First();
            //var pmodel = sfr.PrePortSemanticModel;
            //var model = sfr.SemanticModel;

            //CodeContext codeContext = new CodeContext(pmodel,
            //    model,
            //analyzerResult.ProjectBuildResult.SourceFileBuildResults.First().SyntaxTree,
            //    projectPath,
            //    analyzerResult.ProjectBuildResult.SourceFileBuildResults.First().SourceFilePath,
            //    configuration,
            //    logger);

            var allClasses = analyzerResult
                .ProjectResult
                .SourceFileResults
                .Select(s => (fullFilePath: s.FileFullPath, sourceFIles: File.ReadAllLines(s.FileFullPath), classes: s.AllClasses()))
                .SelectMany(s => s.classes.Select(sfd => new SourceFileDefinition(analyzerResult, s.fullFilePath, s.sourceFIles, sfd)))
                .ToList();

            allClasses
            .WriteOutput(".\\dump.json");


        }
    }
}
