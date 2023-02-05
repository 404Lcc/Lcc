using DynamicExpresso;
using System.Collections.Generic;

namespace LccModel
{
    public static class ExpressionHelper
    {
        public static T Evaluate<T>(string expression, Dictionary<string, string> dict)
        {
            Interpreter interpreter = new Interpreter();
            foreach (var item in dict)
            {
                var value = float.Parse(item.Value);
                interpreter.SetVariable(item.Key, value);
            }
            return interpreter.Eval<T>(expression);
        }
        public static T Evaluate<T>(string expression)
        {
            Interpreter interpreter = new Interpreter();
            return interpreter.Eval<T>(expression);
        }
    }
}