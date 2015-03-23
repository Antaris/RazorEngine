using RazorEngine.Common;

namespace RazorEngine
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// PositionTagged
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerDisplay("({Position})\"{Value}\"")]
    public class PositionTagged<T>
    {
        private PositionTagged()
        {
            Position = 0;
            Value = default(T);
        }

        /// <summary>
        /// Creates a new PositionTagged instance
        /// </summary>
        /// <param name="value"></param>
        /// <param name="offset"></param>
        public PositionTagged(T value, int offset)
        {
            Position = offset;
            Value = value;
        }

        /// <summary>
        /// The position.
        /// </summary>
        public int Position { get; private set; }
        /// <summary>
        /// The value.
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        /// Checks if the given object equals the current object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            PositionTagged<T> other = obj as PositionTagged<T>;
            return other != null &&
                   other.Position == Position &&
                   Equals(other.Value, Value);
        }

        /// <summary>
        /// Calculates a hash-code for the current instance.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return HashCodeCombiner.Start()
                .Add(Position)
                .Add(Value)
                .CombinedHash;
        }

        /// <summary>
        /// Returns Value.ToString().
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Value.ToString();
        }

        /// <summary>
        /// convert implicitely to the value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator T(PositionTagged<T> value)
        {
            return value.Value;
        }

        /// <summary>
        /// Convert from a tuple.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator PositionTagged<T>(Tuple<T, int> value)
        {
            return new PositionTagged<T>(value.Item1, value.Item2);
        }

        /// <summary>
        /// Checks if the given instances are equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(PositionTagged<T> left, PositionTagged<T> right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Checks if the given instances are not equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(PositionTagged<T> left, PositionTagged<T> right)
        {
            return !Equals(left, right);
        }
    }
}
