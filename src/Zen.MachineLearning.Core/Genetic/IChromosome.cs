namespace Zen.MachineLearning.Core.Genetic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public interface IChromosome : ICloneable, ICollection
    {
        /// <summary>
        /// Gets the type of the gene.
        /// </summary>
        /// <value>The type of the gene.</value>
        Type GeneType
        {
            get;
        }

        /// <summary>
        /// Gets or sets the <see cref="object"/> at the specified index.
        /// </summary>
        /// <value></value>
        object this[int index]
        {
            get;
            set;
        }

        /// <summary>
        /// Seeds this instance.
        /// </summary>
        void Seed();

        /// <summary>
        /// Seeds the specified probability.
        /// </summary>
        /// <param name="probability">The probability.</param>
        void Seed(float probability);

        /// <summary>
        /// Performs drift mutation on the specified gene.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="mode">The mode.</param>
        void MutateDrift(int index, MutateDriftMode mode);

        /// <summary>
        /// Performs random mutation on the specified gene.
        /// </summary>
        /// <param name="index">The index.</param>
        void MutateRandom(int index);
    }

    public interface IChromosome<TGeneType> : IChromosome, ICollection<TGeneType>
    {
        new TGeneType this[int index]
        {
            get;
            set;
        }
    }
}
