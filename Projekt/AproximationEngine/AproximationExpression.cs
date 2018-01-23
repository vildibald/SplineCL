namespace Projekt.AproximationEngine
{
    public class AproximationExpression
    {
        public string Expression { get; private set; }
        public string Var1 { get; private set; }
        public string Var2 { get; private set; }

        public AproximationExpression(string expression, string var1, string var2)
        {
            Expression = expression;
            Var1 = var1;
            Var2 = var2;
        }
    }
}
