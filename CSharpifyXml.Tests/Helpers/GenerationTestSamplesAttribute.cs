using System.Reflection;
using CSharpifyXml.Core;
using CSharpifyXml.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Sdk;

namespace CSharpifyXml.Tests.Helpers;

/// <summary>
/// Loads test cases from a local folder.
/// The test cases are expected to be in the form of XML and json files
/// </summary>
public class GenerationTestSamplesAttribute(string testCaseRootFolderPath, string templateFilepath) : DataAttribute
{
    private const string TestCaseFileExtension = ".xml";

    private IXmlClassIdentifier? _classIdentifier;

    private IXmlClassIdentifier GetIdentifierInstance()
    {
        if (_classIdentifier != null) return _classIdentifier;
        var services = new ServiceCollection();
        services.AddSingleton(MappingConfiguration.Default());
        services.ConfigureCSharpifyServices();
        var serviceProvider = services.BuildServiceProvider();
        _classIdentifier = serviceProvider.GetRequiredService<IXmlClassIdentifier>();
        return _classIdentifier;
    }

    /// <summary>
    /// Returns the test cases for the test method, by finding all test file xml files in
    /// the test cases folder and their corresponding expected json files. 
    /// </summary>
    /// <returns>The test cases as <see cref="GenerationSample"/></returns>
    private IEnumerable<GenerationSample> GetTestCases()
    {
        var files = Directory.GetFiles(testCaseRootFolderPath, $"*{TestCaseFileExtension}");
        foreach (var testCaseFilepath in files)
        {
            var testCaseName = Path.GetFileNameWithoutExtension(testCaseFilepath) ?? throw new InvalidOperationException("Invalid test case folder.");
            yield return new GenerationSample 
            (
                templateContent: File.ReadAllText(templateFilepath),
                classDescriptions: GetXmlDescriptors(testCaseFilepath).ToList(),
                testCaseName: testCaseName 
            );
        }
    }

    private IEnumerable<XmlClassDescriptor> GetXmlDescriptors(string xmlFilepath)
    {
        using var xmlStream = new StreamReader(xmlFilepath);
        var identifier = GetIdentifierInstance();
        identifier.Identify(xmlStream);
        var map = identifier.GetDescriptors(); 
        return map;
    }

    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        return GetTestCases().Select(couple => new object[] { couple });
    }
}