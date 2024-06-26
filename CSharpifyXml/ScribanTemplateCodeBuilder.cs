﻿using CSharpifyXml.Core;
using Scriban;
using Scriban.Runtime;

namespace CSharpifyXml;

public class ScribanTemplateCodeBuilder : ITemplateCodeBuilder
{
    /// <summary>
    /// Translates the XmlClassDescriptors to Scriban script objects,
    /// which can be injected into the scriban template.
    /// </summary>
    private sealed class InjectionModelBuilder
    {
        private readonly ScriptObject _model = new();

        public InjectionModelBuilder WithTargetNamespace(string targetNamespace)
        {
            _model["TargetNamespace"] = targetNamespace;
            return this;
        }

        public InjectionModelBuilder FromClassDescriptor(XmlClassDescriptor descriptor)
        {
            _model["ClassName"] = descriptor.ElementName;
            _model["IsRoot"] = descriptor.IsRoot;
            _model.Add("FromAttributes", FromPropertyDescriptors(descriptor.FromAttributes));
            _model.Add("FromElements", FromPropertyDescriptors(descriptor.FromElements));
            return this;
        }

        public ScriptObject Build()
        {
            return _model;
        }

        /// <summary>
        /// Translates a collection of <see cref="XmlPropertyDescriptor"/> into an <see cref="ScriptArray"/>.
        /// This allows to iterate through the properties using loops within the scriban template.
        /// </summary>
        /// <param name="descriptors">Descriptors of the class properties.</param>
        /// <returns>The <see cref="ScriptArray"/> containing the property descriptions.</returns>
        private static ScriptArray FromPropertyDescriptors(List<XmlPropertyDescriptor> descriptors)
        {
            var scriptArray = new ScriptArray();
            foreach (var descriptor in descriptors)
            {
                var scriptObject = new ScriptObject();
                scriptObject["Name"] = descriptor.Name;
                scriptObject["TypeName"] = descriptor.TypeName;
                scriptObject["IsClass"] = descriptor.IsClass;
                scriptArray.Add(scriptObject);
            }

            return scriptArray;
        }
    }

    public IEnumerable<ClassFileContent> RenderClasses(ScribanGenerationRequest request)
    {
        var template = Template.Parse(request.TemplateContent);

        if (template.HasErrors)
        {
            throw new InvalidOperationException("Template parsing error: " + string.Join(", ", template.Messages));
        }

        foreach (var descriptionOfFutureClass in request.XmlDescriptions)
        {
            var model = new InjectionModelBuilder()
                .WithTargetNamespace(request.TargetNamespace)
                .FromClassDescriptor(descriptionOfFutureClass)
                .Build();

            var context = new TemplateContext();
            context.PushGlobal(model);

            var renderedClassFileContent = template.Render(context);
            yield return new ClassFileContent($"{descriptionOfFutureClass.ElementName}.cs", renderedClassFileContent);
        }
    }
}