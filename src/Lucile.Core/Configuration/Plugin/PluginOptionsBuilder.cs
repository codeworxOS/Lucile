using System.Collections.Generic;
using System.IO;
using Lucile.Builder;

namespace Lucile.Configuration.Plugin
{
    public class PluginOptionsBuilder : BaseBuilder<PluginOptions>
    {
        public PluginOptionsBuilder()
        {
            Assemblies = new List<string>();
        }

        public List<string> Assemblies { get; }

        public PluginOptionsBuilder FromFolder(string folderPath, string pattern)
        {
            foreach (var item in Directory.GetFiles(folderPath, pattern))
            {
                var fi = new FileInfo(item);
                this.Assemblies.Add(fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length));
            }

            return this;
        }

        public override void LoadFrom(PluginOptions value)
        {
            Assemblies.AddRange(value.Assemblies);
        }

        protected override PluginOptions BuildTarget()
        {
            var result = new PluginOptions();

            foreach (var item in this.Assemblies)
            {
                result.Assemblies.Add(item);
            }

            return result;
        }
    }
}