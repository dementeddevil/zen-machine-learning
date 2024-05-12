namespace Zen.MachineLearning.Core
{
    using System;

    public static class MathEx
    {
        public const double DoubleApproxZero = double.Epsilon * 3;
        public const float FloatApproxZero = float.Epsilon * 3;
        public const double BoltzmannFactor = 1.38066e-23;

        public static bool IsApproximatelyEqual(this double lhs, double rhs)
        {
            return Math.Abs(lhs - rhs) <= DoubleApproxZero;
        }

        public static bool IsApproximatelyEqual(this float lhs, float rhs)
        {
            return Math.Abs(lhs - rhs) <= FloatApproxZero;
        }

        public static bool IsApproximatelyZero(this double value)
        {
            return Math.Abs(value) <= DoubleApproxZero;
        }

        public static bool IsApproximatelyZero(this float value)
        {
            return Math.Abs(value) <= FloatApproxZero;
        }
    }
}
