using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Jace.Util
{
    /// <summary>
    /// Utility methods of Jace.NET that can be used throughout the engine.
    /// </summary>
    internal static class EngineUtil
    {
        static internal IDictionary<string, Complex> ConvertVariableNamesToLowerCase(IDictionary<string, Complex> variables)
        {
            Dictionary<string, Complex> temp = new Dictionary<string, Complex>();
            foreach (KeyValuePair<string, Complex> keyValuePair in variables)
            {
                temp.Add(keyValuePair.Key.ToLowerInvariant(), keyValuePair.Value);
            }

            return temp;
        }
    }
}
