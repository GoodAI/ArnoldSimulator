using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Arnold.Observation
{
    public class InvalidDimensionsException : FormatException
    {
        public InvalidDimensionsException(string message) : base(message) { }
    }

    public class Dimensions
    {
        #region Static 

        private static Dimensions m_emptyInstance;

        public static Dimensions Empty => m_emptyInstance ?? (m_emptyInstance = new Dimensions());

        #endregion

        private readonly IImmutableList<int> m_dims;

        public const int MaxDimensions = 100;  // Ought to be enough for everybody.

        public Dimensions() : this(ImmutableList<int>.Empty)  // This means default dimensions.
        { }

        public Dimensions(IImmutableList<int> immutableDimensions)
        {
            if (immutableDimensions.Count > MaxDimensions)
                throw new InvalidDimensionsException($"Maximum number of dimensions is {MaxDimensions}.");

            m_dims = immutableDimensions;

            // Precompute this since we are immutable.
            ElementCount = IsEmpty ? 0 : Math.Abs(m_dims.Aggregate(1, (acc, item) => acc * item));  // Tolerate -1s.
        }

        public Dimensions(params int[] dimensions) : this(ProcessDimensions(dimensions))
        { }

        public Dimensions(IEnumerable<int> dimensions) : this(ProcessDimensions(dimensions))
        { }

        #region Object overrides

        public bool Equals(Dimensions other)
        {
            if (other.Rank != Rank)
                return false;

            if ((Rank == 1) && (m_dims == null) && (other.m_dims == null))
                return true;

            return (m_dims != null) && (other.m_dims != null) && m_dims.SequenceEqual(other.m_dims);
        }

        public override int GetHashCode()
        {
            if (m_hashCode == -1)
            {
                m_hashCode = IsEmpty ? 0 : m_dims.Aggregate(19, (acc, item) => 31 * acc + item);
            }

            return m_hashCode;
        }
        private int m_hashCode = -1;

        #endregion

        public bool IsEmpty => (m_dims == null) || (m_dims.Count == 0);

        public int Rank => IsEmpty ? 1 : m_dims.Count;

        public int ElementCount { get; }

        public int this[int index]
        {
            get
            {
                if (IsEmpty)
                {
                    if (index == 0)
                        return 0;  // We pretend we have one dimension of size 0.

                    throw GetIndexOutOfRangeException(index, 0);
                }

                if (index >= m_dims.Count)
                    throw GetIndexOutOfRangeException(index, m_dims.Count - 1);

                return m_dims[index];
            }
        }

        public int Width => (m_dims.Count >= 1) ? m_dims[0] : 0;

        public int Height => (m_dims.Count >= 2) ? m_dims[1] : 0;

        #region Private

        private static IndexOutOfRangeException GetIndexOutOfRangeException(int index, int maxIndex)
        {
            return new IndexOutOfRangeException($"Index {index} is greater than max index {maxIndex}.");
        }

        private static IImmutableList<int> ProcessDimensions(IEnumerable<int> dimensions)
        {
            ImmutableList<int>.Builder newDimensionsBuilder = ImmutableList.CreateBuilder<int>();

            foreach (int item in dimensions)
            {
                if (item < 0)
                    throw new InvalidDimensionsException($"Number {item} is not a valid dimension.");

                newDimensionsBuilder.Add(item);

                if (newDimensionsBuilder.Count > MaxDimensions)
                    throw new InvalidDimensionsException($"Maximum number of dimensions is {MaxDimensions}.");
            }

            return newDimensionsBuilder.ToImmutable();
        }

        #endregion  
    }
}
