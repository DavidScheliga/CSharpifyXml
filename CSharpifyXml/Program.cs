// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using CommandLine;
using CSharpifyXml.Core;
using CSharpifyXml.Core.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpifyXml;

public class CSharpifyXmlOptions
{
    [Value(0, MetaName = "template", Required = true, HelpText = "Template file to be processed.")]
    public string TemplateFile { get; set; } = null!;

    [Value(1, MetaName = "inputXml", Required = true, HelpText = "Input XML file to be processed.")]
    public string InputXmlFile { get; set; } = null!;

    [Value(2, MetaName = "TargetNamespace", Required = true, HelpText = "The namespace for the output class files.")]
    public string TargetNamespace { get; set; } = null!;

    [Option('o', "output", Required = false, HelpText = "Output folder for the processed files.", Default = ".")]
    public string OutputPath { get; set; } = null!;

    [Option("SingleFile", Required = false, HelpText = "Generate a single file for all classes.", Default = false)]
    public bool SingleFile { get; set; }

    [Option("SequenceTemplate", Required = false,
        HelpText =
            "Set the template for representing a sequence in the output. The case-insensitive replacement token is '{{typeName}}'.",
        Default = "{{typeName}}[]")]
    public string SequenceTemplate { get; set; } = "{{typeName}}[]";
}

public class Program
{
    private const string ConfigFilename = "appsettings.json";
    private static ServiceProvider? _serviceProvider;

    [RequiresUnreferencedCode("Calls CSharpifyXml.Program.RunApplication(CSharpifyXmlOptions)")]
    static void Main(string[] args)
    {
        // Parse command line arguments
        Parser.Default.ParseArguments<CSharpifyXmlOptions>(args)
            .WithParsed(RunApplication)
            .WithNotParsed(HandleParseError);
    }

    private sealed record RequestCreationResult(bool Success, object? Request, string ErrorMessage)
    {
        public static RequestCreationResult ForFailed(string errorMessage) => new(false, null, errorMessage);

        public static RequestCreationResult ForSucceed(object request) => new(true, request, string.Empty);
    };

    [RequiresUnreferencedCode("Using section of configuration to bind into concrete instance.")]
    private static ServiceProvider GetServiceProvider(CSharpifyXmlOptions opts)
    {
        if (_serviceProvider != null)
        {
            return _serviceProvider;
        }

        var pathOfTheProgram = Path.Join(Path.GetDirectoryName(Environment.ProcessPath), ConfigFilename);
        var config = new ConfigurationBuilder()
            // look within the installation path
            .AddJsonFile(pathOfTheProgram)
            // local configuration overrides installation path
            .AddJsonFile(ConfigFilename)
            .Build();

        var mappingConfig = config.GetSection(nameof(MappingConfiguration)).Get<MappingConfiguration>()
                            ?? new MappingConfiguration { SequenceTemplate = opts.SequenceTemplate };

        var services = new ServiceCollection();
        services.AddLogging(builder => { builder.AddConfiguration(config.GetSection("Logging")); });
        services.AddSingleton<IMappingConfiguration>(mappingConfig);
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        return _serviceProvider;
    }

    /// <summary>
    /// Returns an instance of the concrete implemented service from the IoC container.
    /// </summary>
    /// <remarks>IoC Inversion of Control</remarks>
    /// <typeparam name="T">Interface, which should be provided.</typeparam>
    /// <returns>Instance of concrete implementation of T</returns>
    [RequiresUnreferencedCode("Using section of configuration to bind into concrete instance.")]
    private static T GetRequiredService<T>(CSharpifyXmlOptions opts) where T : notnull
        => GetServiceProvider(opts).GetRequiredService<T>();

    private static void ConfigureServices(IServiceCollection services)
    {
        services.ConfigureCSharpifyServices();
        services.AddSingleton<ITemplateCodeBuilder, ScribanTemplateCodeBuilder>();
        services.AddLogging(configure =>
            {
                configure.AddConsole();
                configure.SetMinimumLevel(LogLevel.Error);
            }
        );
    }

    [RequiresUnreferencedCode("Calls CSharpifyXml.Program.GetRequiredService<T>(CSharpifyXmlOptions)")]
    private static RequestCreationResult CreateScribanRequest(CSharpifyXmlOptions opts)
    {
        string templateContent;

        try
        {
            templateContent = File.ReadAllText(opts.TemplateFile);
        }
        catch (FileNotFoundException)
        {
            return RequestCreationResult.ForFailed($"Template file '{opts.TemplateFile}' not found");
        }
        catch (Exception ex)
        {
            return RequestCreationResult.ForFailed(ex.Message);
        }

        var identifier = GetRequiredService<IXmlClassIdentifier>(opts);
        var inputFilePaths = GetInputFilePaths(opts.InputXmlFile);
        var noFilesWhereFoundUsingTheInputFilepath = inputFilePaths.Length == 0;
        if (noFilesWhereFoundUsingTheInputFilepath)
        {
            return RequestCreationResult.ForFailed($"Xml input file '{opts.TemplateFile}' not found");
        }

        foreach (var inputFilepath in inputFilePaths)
        {
            using var xmlContentReader = new StreamReader(inputFilepath);
            identifier.Identify(xmlContentReader);
        }

        var foundClassDescriptions = identifier.GetDescriptors().ToList();

        var request = new ScribanGenerationRequest(opts.TargetNamespace, templateContent,
            xmlDescriptions: foundClassDescriptions, opts.OutputPath);
        return RequestCreationResult.ForSucceed(request);
    }

    private static string[] GetInputFilePaths(string inputXmlFilepath)
    {
        var fullPath = Path.GetFullPath(inputXmlFilepath);
        var sourceFolder = Path.GetDirectoryName(fullPath);
        if (sourceFolder == null)
        {
            return [];
        }

        var filenameOrPattern = Path.GetFileName(inputXmlFilepath);
        return Directory.GetFiles(sourceFolder, filenameOrPattern);
    }

    [RequiresUnreferencedCode("Calls CSharpifyXml.Program.GetRequiredService<T>(CSharpifyXmlOptions)")]
    private static void RunApplication(CSharpifyXmlOptions opts)
    {
        var buildsTheCode = GetRequiredService<ITemplateCodeBuilder>(opts);

        var requestCreationResult = CreateScribanRequest(opts);
        if (!requestCreationResult.Success)
        {
            Console.WriteLine(requestCreationResult.ErrorMessage);
            return;
        }

        Debug.Assert(requestCreationResult != null);
        var request = (ScribanGenerationRequest)requestCreationResult.Request!;

        var renderedCode = buildsTheCode.RenderClasses(request);
        if (opts.SingleFile)
        {
            CreateSingleClassCodeFile(renderedCode, request.OutputPath);
        }
        else
        {
            CreateMultipleClassCodeFiles(renderedCode, request.OutputPath);
        }
    }

    private static void CreateSingleClassCodeFile(
        IEnumerable<ClassFileContent> codeToBeWritten,
        string outputFolderPath)
    {
        Debug.Assert(codeToBeWritten != null, "There must be a result to write.");
        Debug.Assert(!string.IsNullOrEmpty(outputFolderPath), "The output folder path must be defined.");

        var fullPathGiven = Path.GetFullPath(outputFolderPath);
        var filenameFromUser = Path.GetFileName(fullPathGiven);
        var finalFilename = !string.IsNullOrEmpty(filenameFromUser) ? filenameFromUser : "GeneratedClasses.cs";
        var folderPath = Path.GetDirectoryName(fullPathGiven) ?? ".";
        var directory = Directory.CreateDirectory(folderPath);

        var filepath = Path.Join(directory.FullName, finalFilename);
        var ifIsThereDoNotOverride = Path.Exists(filepath);
        if (ifIsThereDoNotOverride)
        {
            return;
        }

        var sb = new StringBuilder();
        foreach (var code in codeToBeWritten)
        {
            sb.Append(code.Content);
            sb.AppendLine("");
            sb.AppendLine("");
        }

        File.WriteAllText(filepath, sb.ToString());
    }

    private static void CreateMultipleClassCodeFiles(IEnumerable<ClassFileContent> codeToBeWritten,
        string outputFolderPath)
    {
        foreach (var code in codeToBeWritten)
        {
            CreateClassCodeFile(code, outputFolderPath);
        }
    }

    private static void CreateClassCodeFile(ClassFileContent codeToBeWritten, string outputFolderPath)
    {
        Debug.Assert(codeToBeWritten != null, "There must be a result to write.");
        Debug.Assert(!string.IsNullOrEmpty(outputFolderPath), "The output folder path must be defined.");

        var directory = Directory.CreateDirectory(outputFolderPath);

        var filepath = Path.Join(directory.FullName, codeToBeWritten.ProposedFilename);
        if (Path.Exists(filepath))
        {
            return;
        }

        File.WriteAllText(filepath, codeToBeWritten.Content);
    }

    private static void HandleParseError(IEnumerable<Error> errs)
    {
        // Handle errors here
        Console.WriteLine("Error parsing command line arguments:");
        foreach (var err in errs)
        {
            Console.WriteLine(err.ToString());
        }
    }
}