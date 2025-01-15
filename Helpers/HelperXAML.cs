using System.Xml;
using System.IO;
using System.Windows.Markup;

namespace TWC.Helpers
{
    public class HelperXAML
    {
        public static T Clone<T>(T source)
        {
            string gridXaml = XamlWriter.Save(source);
            StringReader stringReader = new StringReader(gridXaml);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            return (T)XamlReader.Load(xmlReader);
            
            /*
            var sb = new StringBuilder();
            var writer = XmlWriter.Create(sb, new XmlWriterSettings
            {
                Indent = true,
                ConformanceLevel = ConformanceLevel.Fragment,
                OmitXmlDeclaration = true,
                NamespaceHandling = NamespaceHandling.OmitDuplicates,
            });
            var mgr = new XamlDesignerSerializationManager(writer);
            // HERE BE MAGIC!!!
            mgr.XamlWriterMode = XamlWriterMode.Value;            
            // THERE WERE MAGIC!!!
            System.Windows.Markup.XamlWriter.Save(source, mgr);


            StringReader stringReader = new StringReader(sb.ToString());
            XmlReader xmlReader = XmlReader.Create(stringReader);
            T t = (T)XamlReader.Load(xmlReader);
            return t;*/
        }
    }
}
