using System.Numerics;

namespace Zen.MachineLearning.Core.Neural;

/// <summary>
/// Unsupervised learning interface.
/// </summary>
/// 
/// <remarks><para>The interface describes methods, which should be implemented
/// by all unsupervised learning algorithms. Unsupervised learning is such
/// type of learning algorithms, where system's desired output is not known on
/// the learning stage. Given sample input values, it is expected, that
/// system will organize itself in the way to find similarities betweed provided
/// samples.</para></remarks>
/// 
public interface IUnsupervisedLearning<T> 
    where T : IFloatingPoint<T>, IRootFunctions<T>, ISignedNumber<T>, IUnaryNegationOperators<T, T>, IAdditionOperators<T, T, T>
{
    /// <summary>
    /// Runs learning iteration.
    /// </summary>
    /// <param name="input">Input vector.</param>
    /// <returns>Returns learning error.</returns>
    T Run(Vector<T> input);

    /// <summary>
    /// Runs learning epoch.
    /// </summary>
    /// <param name="input">Array of input vectors.</param>
    /// <returns>Returns sum of learning errors.</returns>
    T RunEpoch(Vector<T>[] input);
}
