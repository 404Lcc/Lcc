using B83.ExpressionParser;

namespace LccModel
{
    public static class ExpressionHelper
    {
        public static ExpressionParser ExpressionParser { get; set; } = new ExpressionParser();


        public static Expression TryEvaluate(string expressionStr)
        {
            Expression expression = null;
            try
            {
                expression = ExpressionParser.EvaluateExpression(expressionStr);
            }
            catch
            {
            }
            return expression;
        }
    }
}