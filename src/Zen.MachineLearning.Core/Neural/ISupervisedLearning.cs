using System.Numerics;

namespace Zen.MachineLearning.Core.Neural;

/// <summary>
/// Supervised learning interface.
/// </summary>
/// 
/// <remarks><para>The interface describes methods, which should be implemented
/// by all supervised learning algorithms. Supervised learning is such
/// type of learning algorithms, where system's desired output is known on
/// the learning stage. So, given sample input values and desired outputs,
/// system should adopt its internals to produce correct (or close to correct)
/// result after the learning step is complete.</para></remarks>
/// 
public interface ISupervisedLearning<T>
    where T : IFloatingPoint<T>
{
    /// <summary>
    /// Runs learning iteration.
    /// </summary>
    /// 
    /// <param name="input">Input vector.</param>
    /// <param name="output">Desired output vector.</param>
    /// 
    /// <returns>Returns learning error.</returns>
    /// 
    T Run(Vector<T> input, Vector<T> output);

    /// <summary>
    /// Runs learning epoch.
    /// </summary>
    /// 
    /// <param name="input">Array of input vectors.</param>
    /// <param name="output">Array of output vectors.</param>
    /// 
    /// <returns>Returns sum of learning errors.</returns>
    /// 
    T RunEpoch(Vector<T>[] input, Vector<T>[] output);
}
