using System;
using System.Collections.Generic;
using com.ethnicthv.chemlab.engine.api;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.api.ui.element
{
    public static class ElementUtil
    {
        public static List<string> GetFullElectronConfiguration(string electronConfiguration)
        {
            var stack = new Stack<string>();
            
            PushElectronConfiguration(stack, electronConfiguration);
            
            while (stack.Peek().Contains('['))
            {
                var subshell = stack.Pop();
                var subshellElement = subshell.Remove(0,1);
                subshellElement = subshellElement.Remove(subshellElement.Length - 1, 1);
                PushElectronConfiguration(stack, ElementProperty.GetElementProperty(subshellElement).ElectronConfiguration);
            }

            var results = new List<string>();
            
            while (stack.TryPop(out var electron))
            {
                results.Add(electron);
            }
            return results;
        }

        public static (int n, int e) AnalyzePart(string part)
        {
            var n = part[0] - '0';
            var e = part[^1] - '0';

            return (n, e);
        }
        
        public static Dictionary<int, int> AnalyzeElectronConfiguration(string electronConfiguration)
        {
            var fullElectronConfiguration = GetFullElectronConfiguration(electronConfiguration);
            var result = new Dictionary<int, int>();
            
            foreach (var part in fullElectronConfiguration)
            {
                var (n, e) = AnalyzePart(part);
                if (result.ContainsKey(n))
                {
                    result[n] += e;
                }
                else
                {
                    result.Add(n, e);
                }
            }

            return result;
        }
        
        private static void PushElectronConfiguration(Stack<string> stack, string electronConfiguration)
        {
            var electronConfigurationArray = electronConfiguration.Split(" ");
            
            if (electronConfigurationArray.Length < 1)
                throw new Exception("Invalid electron configuration: " + electronConfiguration);
            
            for(var i = electronConfigurationArray.Length - 1; i >= 0; i--)
            {
                stack.Push(electronConfigurationArray[i]);
            }
        }
    }
}