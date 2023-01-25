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
            catch (System.Exception e)
            {
            }
            return expression;
        }
    }
    //public class ExpressionParser
    //{
    //    public Expression EvaluateExpression(string expressionStr)
    //    {
    //        Expression expression = new Expression(expressionStr);
    //        return expression;
    //    }
    //}
    //public class Expression
    //{
    //    public string expression;

    //    public Dictionary<string, Parameter> Parameters = new Dictionary<string, Parameter>();

    //    public double Value => CSharpExpression.Evaluate<double>(expression);

    //    public Expression(string expression)
    //    {
    //        this.expression = expression;
    //    }
    //}
    //public class Parameter
    //{
    //    public string key;
    //    public double Value;
    //}

    //public static class ExpressionHelper
    //{
    //    public static ExpressionParser ExpressionParser { get; set; } = new ExpressionParser();


    //    public static Expression TryEvaluate(string expressionStr)
    //    {
    //        Expression expression = null;
    //        try
    //        {
    //            expression = ExpressionParser.EvaluateExpression(expressionStr);
    //        }
    //        catch (System.Exception e)
    //        {
    //        }
    //        return expression;
    //    }
    //}
}