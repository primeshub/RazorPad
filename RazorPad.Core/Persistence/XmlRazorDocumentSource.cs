﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace RazorPad.Persistence
{
    [Export(typeof(IRazorDocumentSource))]
    public class XmlRazorDocumentSource : IRazorDocumentSource
    {
        private static readonly IList<string> Extensions =
            new List<string> { ".razorpad", ".xml" };

        private readonly ModelProviders _modelProviders;

        public Encoding Encoding { get; set; }

        public bool CanLoad(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
                return false;

            return Extensions.Any(x => uri.EndsWith(x, StringComparison.OrdinalIgnoreCase));
        }

        public bool CanLoad(Stream stream)
        {
            if (stream == null || !stream.CanRead)
                return false;

            var reader = new StreamReader(stream, Encoding);
            var firstLine = reader.ReadLine() ?? string.Empty;
            
            stream.Seek(0, SeekOrigin.Begin);

            return firstLine.Contains("<? xml") || firstLine.Contains("<RazorDocument>");
        }

        public bool CanSave(RazorDocument document, string uri)
        {
            if (document == null)
                return false;

            return CanLoad(uri);
        }

        public bool CanSave(RazorDocument document, Stream stream)
        {
            if (document == null || stream == null || !stream.CanRead)
                return false;

            return CanSave(document, document.Filename);
        }

        [ImportingConstructor]
        public XmlRazorDocumentSource(ModelProviders modelProviders)
        {
            if(modelProviders == null)
                throw new ArgumentNullException("modelProviders");

            _modelProviders = modelProviders;
            Encoding = Encoding.UTF8;
        }


        public RazorDocument Parse(string document)
        {
            var source = XDocument.Parse(document);
            return Load(source);
        }

        public RazorDocument Load(string uri)
        {
            var source = XDocument.Load(uri);
            return Load(source);
        }

        public RazorDocument Load(Stream stream)
        {
            var source = XDocument.Load(stream);
            return Load(source);
        }

        public void Save(RazorDocument document, string uri)
        {
            var destination = uri ?? document.Filename;

            if (string.IsNullOrWhiteSpace(destination))
                throw new ApplicationException("No filename specified!");

            document.DocumentKind = RazorDocument.GetDocumentKind(uri);

            using (var stream = File.Open(destination, FileMode.Create, FileAccess.Write))
            {
                Save(document, stream);

                try { stream.Flush(); }
                catch (Exception ex)
                {
                    Trace.WriteLine("Error flushing file stream: " + ex);
                }
            }
        }

        public RazorDocument Load(XDocument source)
        {
            if(source == null || source.Root == null)
                return null;

            var root = source.Root;
            var metadataEl = root.Element("Metadata") ?? new XElement("Metadata");
            var referencesEl = root.Element("References") ?? new XElement("References");
            var modelEl = root.Element("Model") ?? new XElement("Model");
            var templateEl = root.Element("Template") ?? new XElement("Template");

            var modelProviderEl = modelEl.Attribute("Provider");
            var modelProviderName = (modelProviderEl == null) ? "Json" : modelProviderEl.Value;
            var modelProvider = _modelProviders.Create(modelProviderName);
            modelProvider.Deserialize(modelEl.Value);
            var references = referencesEl.Elements().Select(x => x.Value).Where(x => !string.IsNullOrWhiteSpace(x));

            IDictionary<string, string> metadata =
                metadataEl.Elements()
                    .Select(x => new KeyValuePair<string, string>(x.Name.LocalName, x.Value))
                    .ToDictionary(val => val.Key, val => val.Value);

            return new RazorDocument(templateEl.Value, references, modelProvider, metadata);
        }

        public void Save(RazorDocument document, Stream stream)
        {
            var writer = new XmlTextWriter(stream, Encoding) { Formatting = Formatting.Indented };

            writer.WriteStartElement("RazorDocument");

            writer.WriteStartElement("Metadata");
            foreach (var datum in document.Metadata)
            {
                writer.WriteStartElement(datum.Key);
                writer.WriteString(datum.Value);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteStartElement("References");
            foreach (var reference in document.References ?? Enumerable.Empty<string>())
            {
                writer.WriteElementString("Reference", reference);
            }
            writer.WriteEndElement();

            var providerName = (string)new ModelProviderName(document.ModelProvider);
            var serializedModel = document.ModelProvider.Serialize();
            writer.WriteStartElement("Model");
            if (!string.IsNullOrWhiteSpace(providerName))
                writer.WriteAttributeString("Provider", providerName);
            writer.WriteCData(serializedModel);
            writer.WriteEndElement();

            writer.WriteStartElement("Template");
            if (!string.IsNullOrWhiteSpace(document.TemplateBaseClassName))
                writer.WriteAttributeString("BaseClass", document.TemplateBaseClassName);
            writer.WriteCData(document.Template);
            writer.WriteEndElement();


            writer.WriteEndElement();

            writer.Flush();
        }
    }
}
