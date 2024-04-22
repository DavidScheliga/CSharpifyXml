// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using CommandLine;
using CSharpifyXml.Core;
using CSharpifyXml.Core.Abstractions;
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
    public string OutputFolder { get; set; } = null!;

    [Option("sequenceTemplate", Required = false,
        HelpText =
            "Set the template for representing a sequence in the output. The case-insensitive replacement token is '{{typeName}}'.",
        Default = "{{typeName}}[]")]
    public string SequenceTemplate { get; set; } = "{{typeName}}[]";
}

public class Program
{
    private static ServiceProvider? _serviceProvider;

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

    private static ServiceProvider GetServiceProvider(CSharpifyXmlOptions opts)
    {
        if (_serviceProvider != null)
        {
            return _serviceProvider;
        }

        var config = new MappingConfiguration()
        {
            SequenceTemplate = opts.SequenceTemplate
        };

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IMappingConfiguration>(config);
        ConfigureServices(serviceCollection);
        _serviceProvider = serviceCollection.BuildServiceProvider();
        return _serviceProvider;
    }

    /// <summary>
    /// Returns an instance of the concrete implemented service from the IoC container.
    /// </summary>
    /// <remarks>IoC Inversion of Control</remarks>
    /// <typeparam name="T">Interface, which should be provided.</typeparam>
    /// <returns>Instance of concrete implementation of T</returns>
    private static T GetRequiredService<T>(CSharpifyXmlOptions opts) where T : notnull => GetServiceProvider(opts).GetRequiredService<T>();

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
        if (!Path.Exists(opts.InputXmlFile))
        {
            return RequestCreationResult.ForFailed($"Xml input file '{opts.TemplateFile}' not found");
        }

        using var xmlContentReader = new StreamReader(opts.InputXmlFile);
        var foundClassDescriptions = identifier.Identify(xmlContentReader).ToList();

        var request = new ScribanGenerationRequest(opts.TargetNamespace, templateContent,
            xmlDescriptions: foundClassDescriptions, opts.OutputFolder);
        return RequestCreationResult.ForSucceed(request);
    }

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

        foreach (var codeToBeWritten in buildsTheCode.RenderClasses(request))
        {
            CreateClassCodeFile(codeToBeWritten, request.OutputFolder);
        }
    }

    private static void CreateClassCodeFile(ClassFileContent codeToBeWritten, string outputFolderPath)
    {
        Debug.Assert(codeToBeWritten != null, "There must be a result to write.");
        Debug.Assert(!string.IsNullOrEmpty(outputFolderPath), "The output folder path must be defined.");

        if (!Path.Exists(outputFolderPath))
        {
            throw new DirectoryNotFoundException($"The output path '{outputFolderPath}' could not be found.");
        }

        var filepath = Path.Join(outputFolderPath, codeToBeWritten.ProposedFilename);
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