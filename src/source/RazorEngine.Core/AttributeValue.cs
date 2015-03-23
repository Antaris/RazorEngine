namespace RazorEngine
{
    using System;
    /// <summary>
    /// Razor Html Attribute value
    /// </summary>
    public class AttributeValue
    {
        /// <summary>
        /// Creates a new Razor Html Attribute value.
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="value"></param>
        /// <param name="literal"></param>
        public AttributeValue(PositionTagged<string> prefix, PositionTagged<object> value, bool literal)
        {
            Prefix = prefix;
            Value = value;
            Literal = literal;
        }

        /// <summary>
        /// The prefix of the attribute.
        /// </summary>
        public PositionTagged<string> Prefix { get; private set; }

        /// <summary>
        /// The Value of the attribute.
        /// </summary>
        public PositionTagged<object> Value { get; private set; }

        /// <summary>
        /// Indicates whether the attribute is a lital.
        /// </summary>
        public bool Literal { get; private set; }

        /// <summary>
        /// Convert from a tuple.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static AttributeValue FromTuple(Tuple<Tuple<string, int>, Tuple<object, int>, bool> value)
        {
            return new AttributeValue(value.Item1, value.Item2, value.Item3);
        }

        /// <summary>
        /// Convert from a tuple.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static AttributeValue FromTuple(Tuple<Tuple<string, int>, Tuple<string, int>, bool> value)
        {
            return new AttributeValue(value.Item1, new PositionTagged<object>(value.Item2.Item1, value.Item2.Item2), value.Item3);
        }

        /// <summary>
        /// Convert from a tuple
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator AttributeValue(Tuple<Tuple<string, int>, Tuple<object, int>, bool> value)
        {
            return FromTuple(value);
        }

        /// <summary>
        /// Convert from a tuple.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator AttributeValue(Tuple<Tuple<string, int>, Tuple<string, int>, bool> value)
        {
            return FromTuple(value);
        }
    }
}