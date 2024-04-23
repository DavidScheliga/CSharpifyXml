using CSharpifyXml.Core;
using CSharpifyXml.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace CSharpifyXml;

public class ScribanRequestBuilder(IXmlClassIdentifier identifier, ILogger? logger = null)
{
    const string DefaultNamespace = "Generated.By.CSharpifyXml";
    private string? _templateFilepath;
    private string _targetNamespace = DefaultNamespace;
    private string _outputFolder = ".";
    private bool _buildSucceed = true;
    private string? _xmlFilepath;

    public ScribanRequestBuilder WithOptions(CSharpifyXmlOptions options)
    {
        return WithNamespace(options.TargetNamespace)
            .WithTemplate(options.TemplateFile)
            .WithOutputFolder(options.OutputPath);
    }

    private ScribanRequestBuilder WithNamespace(string targetNamespace)
    {
        _targetNamespace = string.IsNullOrEmpty(targetNamespace) 
            ? "Generated.By.CSharpifyXml" 
            : targetNamespace;
        return this;
    }

    private ScribanRequestBuilder WithTemplate(string templateFilepath)
    {
        _templateFilepath = templateFilepath;
        return this;
    }

    private string GetTemplateContent(string templateFilepath)
    {
        try
        {
            return File.ReadAllText(templateFilepath);
        }
        catch (Exception _) when (_ is DirectoryNotFoundException or FileNotFoundException)
        {
            logger?.LogError($"Error reading {templateFilepath} not found.");
            return "";
        }
        catch (IOException)
        {
            logger?.LogError($"Error reading template {templateFilepath}");
            return "";
        }
        catch (Exception e)
        {
            logger?.LogError($"Error reading template {templateFilepath}: {e.Message}");
            return "";
        }
    }

    public ScribanRequestBuilder WithXmlFile(string xmlFilepath)
    {
        _xmlFilepath = xmlFilepath;
        return this;
    }

    private ScribanRequestBuilder WithOutputFolder(string outputFolder)
    {
        _outputFolder = string.IsNullOrEmpty(outputFolder) ? "." : outputFolder;
        return this;
    }

    public ScribanGenerationRequest? Build()
    {
        if (string.IsNullOrEmpty(_templateFilepath))
        {
            logger?.LogError("Template file not provided.");
            _buildSucceed = false;
            return null;
        }
        
        if (string.IsNullOrEmpty(_xmlFilepath))
        {
            logger?.LogError("XML file not provided.");
            _buildSucceed = false;
            return null;
        }
        
        var templateContent = GetTemplateContent(_templateFilepath);
        var classDescriptions = GetDescriptions(_xmlFilepath);

        if (_buildSucceed)
        {
            return null;
        }

        return new ScribanGenerationRequest(
            targetNamespace: _targetNamespace,
            templateContent: templateContent,
            xmlDescriptions: classDescriptions.ToList(),
            outputFolder: _outputFolder
        );
    }

    private IEnumerable<XmlClassDescriptor> GetDescriptions(string xmlFilepath)
    {
        using var fileStream = new StreamReader(xmlFilepath);
        return identifier.Identify(fileStream);
    }
}