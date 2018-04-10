using System.Collections.Generic;

namespace Lucile.Configuration.Plugin
{
    public class PluginOptions
    {
        public PluginOptions()
        {
            Assemblies = new HashSet<string>();
        }

        public HashSet<string> Assemblies { get; }
    }
}