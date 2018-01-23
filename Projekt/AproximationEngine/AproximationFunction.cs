using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalculationEngine;

namespace Projekt.AproximationEngine
{
    /// <summary>
    /// Defines function and it's derivations in form of delegates.
    /// </summary>
    public sealed class AproximationFunction
    {
        private const double H = 0.01;

        /// <summary>
        ///     Z to approximate by Hermite splines.
        ///     All required derivations are calculated automatically.
        /// </summary>
        /// <param name="z"></param>
        public AproximationFunction(Func<double, double, double> z)
        {
            Z = z;
            // Partial derivations 
            const double h2 = 2 * H;
            Dx = (x, y) => (z(x + H, y) - z(x - H, y))/(h2);
            Dy = (x, y) => (z(x, y + H) - z(x, y - H))/(h2);
            Dxy = (x, y) => (Dx(x, y + H) - Dx(x, y - H))/(h2);
        }

        /// <summary>
        ///     Z to approximate by Hermite splines.
        ///     All required derivations are exactly specified.
        /// </summary>
        /// <param name="z"></param>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <param name="dxy"></param>
        public AproximationFunction(Func<double, double, double> z, Func<double, double, double> dx,
            Func<double, double, double> dy, Func<double, double, double> dxy)
        {
            Z = z;
            Dx = dx;
            Dy = dy;
            Dxy = dxy;
        }

        public Func<double, double, double> Z { get; private set; }

        public Func<double, double, double> Dx { get; private set; }
        public Func<double, double, double> Dy { get; private set; }

        public Func<double, double, double> Dxy { get; private set; }

        /// <summary>
        ///     Parse aproximation Z from string.
        ///     All required derivations are calculated automatically.
        /// </summary>
        /// <returns>
        ///     Instance of class if expression is correct math Z, othervise
        ///     returns null;
        /// </returns>
        public static AproximationFunction FromExpression(AproximationExpression expression)
        {
            var engine = new CalcEngine();
            engine.Variables[expression.Var1] = 0.0d;
            engine.Variables[expression.Var2] = 0.0d;
            try
            {
                engine.Evaluate(expression.Expression);
            }
            catch (Exception)
            {
                return null;
            }
            Func<double, double, double> function = (x, y) =>
            {
                engine.Variables[expression.Var1] = x;
                engine.Variables[expression.Var2] = y;
                var result = (double)engine.Evaluate(expression.Expression);
                return result;
            };

            return new AproximationFunction(function);
        }
    }
}
