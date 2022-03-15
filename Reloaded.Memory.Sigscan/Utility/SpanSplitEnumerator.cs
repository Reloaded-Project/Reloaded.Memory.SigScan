using System;

namespace Reloaded.Memory.Sigscan.Utility
{
    /// <summary>
    /// Creates a <see cref="SpanSplitEnumerator{TSpanType}"/> that allows for the efficient enumeration of a string
    /// to be split.
    /// </summary>
    /// <typeparam name="TSpanType">The item type held by the span..</typeparam>
    public ref struct SpanSplitEnumerator<TSpanType> where TSpanType : IEquatable<TSpanType>
    {
        /// <summary>
        /// The item to split on.
        /// </summary>
        public TSpanType  SplitItem { get; private set; }

        /// <summary>
        /// The current state of the span.
        /// </summary>
        public ReadOnlySpan<TSpanType> Current { get; private set; }

        /// <summary>
        /// The original span this struct was instantiated with.
        /// </summary>
        private ReadOnlySpan<TSpanType> _original;
        private bool _reachedEnd;

        /// <summary>
        /// Moves the span to the next element delimited by the item to split by.
        /// </summary>
        /// <returns>True if the item has moved. False if there is no item to move to.</returns>
        public bool MoveNext()
        {
            var index = _original.IndexOf(SplitItem);
            if (index == -1)
            {
                if (_reachedEnd)
                    return false;

                Current = _original;
                _reachedEnd = true;
                return true;
            }

            // Move to next token.
            Current   = _original.Slice(0, index);
            _original = _original.Slice(index + 1);

            return true;
        }

        /// <summary>
        /// Creates an enumerator used to split spans by a specific item.
        /// </summary>
        /// <param name="item">The span to split items within.</param>
        /// <param name="splitItem">The item to split on.</param>
        public SpanSplitEnumerator(ReadOnlySpan<TSpanType> item, TSpanType splitItem)
        {
            _original  = item;
            Current    = _original;
            SplitItem  = splitItem;
            _reachedEnd = false;
        }
    }
}
