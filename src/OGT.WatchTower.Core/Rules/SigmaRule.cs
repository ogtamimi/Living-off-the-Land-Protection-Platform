using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace OGT.WatchTower.Core.Rules
{
    public class SigmaRule
    {
        [YamlMember(Alias = "title")]
        public string Title { get; set; }

        [YamlMember(Alias = "id")]
        public string Id { get; set; }

        [YamlMember(Alias = "description")]
        public string Description { get; set; }

        [YamlMember(Alias = "level")]
        public string Level { get; set; } = "low";

        [YamlMember(Alias = "detection")]
        public Dictionary<string, object> Detection { get; set; }

        public string FilePath { get; set; }

        public bool IsValid => Detection != null && Detection.ContainsKey("condition");
    }
}
