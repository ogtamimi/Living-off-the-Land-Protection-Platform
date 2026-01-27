using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using OGT.WatchTower.Core.Rules;

namespace OGT.WatchTower.Core.Detection
{
    public class DetectionEngine
    {
        private List<SigmaRule> _rules = new List<SigmaRule>();
        private IDeserializer _deserializer;

        public DetectionEngine(string rulesPath)
        {
            _deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            LoadRules(rulesPath);
        }

        private void LoadRules(string rulesPath)
        {
            if (!Directory.Exists(rulesPath))
                return;

            foreach (var file in Directory.GetFiles(rulesPath, "*.yml", SearchOption.AllDirectories))
            {
                try
                {
                    var yaml = File.ReadAllText(file);
                    var rule = _deserializer.Deserialize<SigmaRule>(yaml);
                    rule.FilePath = file;
                    if (rule.IsValid)
                    {
                        _rules.Add(rule);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load rule {file}: {ex.Message}");
                }
            }
        }

        public List<SigmaRule> Evaluate(Dictionary<string, object> eventData)
        {
            var matches = new List<SigmaRule>();
            foreach (var rule in _rules)
            {
                if (MatchRule(rule, eventData))
                {
                    matches.Add(rule);
                }
            }
            return matches;
        }

        private bool MatchRule(SigmaRule rule, Dictionary<string, object> eventData)
        {
            if (!rule.IsValid) return false;

            var condition = rule.Detection["condition"].ToString();
            var results = new Dictionary<string, bool>();

            foreach (var kvp in rule.Detection)
            {
                if (kvp.Key == "condition") continue;
                results[kvp.Key] = CheckSelection(kvp.Value, eventData);
            }

            return EvaluateCondition(condition, results);
        }

        private bool CheckSelection(object selection, Dictionary<string, object> eventData)
        {
            // List means OR (ANY of the items in the list must match)
            if (selection is List<object> list)
            {
                return list.Any(item => CheckSelection(item, eventData));
            }

            // Dict means AND (ALL keys in the dict must match)
            if (selection is Dictionary<object, object> dict)
            {
                foreach (var kvp in dict)
                {
                    string fullKey = kvp.Key.ToString();
                    object expectedValue = kvp.Value;

                    // Parse Field and Modifiers
                    // e.g. "CommandLine|contains" -> Field: "CommandLine", Modifier: "contains"
                    string[] parts = fullKey.Split('|');
                    string field = parts[0];
                    string modifier = parts.Length > 1 ? parts[1].ToLower() : "";
                    
                    // Check if field exists
                    if (!eventData.ContainsKey(field)) return false;
                    string eventValue = eventData[field]?.ToString() ?? "";

                    // Handle List of Expected Values (OR logic by default for the field)
                    // e.g. CommandLine|contains: ["-e", "-en"] means contains "-e" OR contains "-en"
                    List<string> expectedValues = new List<string>();
                    if (expectedValue is List<object> valList)
                    {
                        expectedValues.AddRange(valList.Select(v => v.ToString()));
                    }
                    else
                    {
                        expectedValues.Add(expectedValue.ToString());
                    }

                    bool fieldMatch = false;
                    foreach (var val in expectedValues)
                    {
                        if (CheckValue(eventValue, val, modifier))
                        {
                            fieldMatch = true;
                            break;
                        }
                    }

                    if (!fieldMatch) return false;
                }
                return true;
            }

            return false;
        }

        private bool CheckValue(string eventValue, string expectedValue, string modifier)
        {
            if (string.IsNullOrEmpty(expectedValue)) return true;

            switch (modifier)
            {
                case "contains":
                    return eventValue.IndexOf(expectedValue, StringComparison.OrdinalIgnoreCase) >= 0;
                
                case "endswith":
                    return eventValue.EndsWith(expectedValue, StringComparison.OrdinalIgnoreCase);
                
                case "startswith":
                    return eventValue.StartsWith(expectedValue, StringComparison.OrdinalIgnoreCase);
                
                default:
                    // Default behavior: Exact match or simple wildcard
                    if (expectedValue.StartsWith("*") && expectedValue.EndsWith("*"))
                    {
                        return eventValue.IndexOf(expectedValue.Trim('*'), StringComparison.OrdinalIgnoreCase) >= 0;
                    }
                    else if (expectedValue.EndsWith("*"))
                    {
                        return eventValue.StartsWith(expectedValue.TrimEnd('*'), StringComparison.OrdinalIgnoreCase);
                    }
                    else if (expectedValue.StartsWith("*"))
                    {
                        return eventValue.EndsWith(expectedValue.TrimStart('*'), StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        return eventValue.Equals(expectedValue, StringComparison.OrdinalIgnoreCase);
                    }
            }
        }

        private bool EvaluateCondition(string condition, Dictionary<string, bool> results)
        {
            // Simplified condition evaluator similar to the Python one
            // Supports: selection, selection1 or selection2, 1 of selection*
            
            // 1. Handle "1 of ..."
            if (condition.Contains("1 of"))
            {
                // Simplified: if any result is true, return true
                // In a real engine, we'd filter by pattern (e.g. 1 of selection*)
                return results.Values.Any(b => b);
            }
            
            if (condition.Contains("all of"))
            {
                 return results.Values.All(b => b);
            }

            // 2. Replace tokens with values
            // Sort by length desc to avoid partial matches
            var sortedKeys = results.Keys.OrderByDescending(k => k.Length);
            var expression = condition;

            foreach (var key in sortedKeys)
            {
                expression = expression.Replace(key, results[key].ToString().ToLower());
            }

            // 3. Evaluate boolean expression
            // Since we can't easily eval() in C#, we'll do a very basic check for now
            // Or use a library like NCalc, but we are limited on packages.
            // Let's implement a very basic recursive evaluator or just support simple AND/OR
            
            // Hacky boolean eval for "true or false", "true and false"
            // Remove 'not' logic for simplicity or handle it
            expression = expression.Replace("not ", "!");
            expression = expression.Replace(" and ", " && ");
            expression = expression.Replace(" or ", " || ");

            // Since we can't eval, we will rely on the "1 of" logic mostly for now
            // or try to manually parse simple expressions if needed.
            // But looking at the python code:
            // condition = condition.replace(selection_name, str(results[selection_name]))
            // return eval(condition)
            
            // Most rules use "1 of selection*" or simple logic.
            // Let's just try to process simple OR/AND if no "1 of".
            
            try 
            {
                // Very naive evaluation for simple cases
                if (expression.Contains("||") && !expression.Contains("&&"))
                    return expression.Split("||").Any(p => bool.Parse(p.Trim()));
                
                if (expression.Contains("&&") && !expression.Contains("||"))
                    return expression.Split("&&").All(p => bool.Parse(p.Trim()));
                    
                 return bool.Parse(expression.Trim());
            }
            catch
            {
                // Fallback: if any is true, return true (loose matching)
                return results.Values.Any(b => b);
            }
        }
    }
}
