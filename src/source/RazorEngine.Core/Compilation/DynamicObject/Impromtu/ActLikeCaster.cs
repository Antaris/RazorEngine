using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using RazorEngine.Compilation.ImpromptuInterface.Dynamic;

namespace RazorEngine.Compilation.ImpromptuInterface
{
    /// <summary>
    /// Extends the <see cref="ImpromptuForwarder"/> class to allow implicit
    /// and explicit conversions to any interface type.
    /// </summary>
    public class ActLikeCaster : ImpromptuForwarder
    {
        private List<Type> _interfaceTypes;

        /// <summary>
        /// handles any conversion call.
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns>true if successful.</returns>
        public override bool TryConvert(System.Dynamic.ConvertBinder binder, out object result)
        {
            result = null;

            if (binder.Type.IsInterface)
            {
                _interfaceTypes.Insert(0, binder.Type);
                result = Impromptu.DynamicActLike(Target, _interfaceTypes.ToArray());
                return true;
            }

            if (binder.Type.IsInstanceOfType(Target))
            {
                result = Target;
            }

            return false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActLikeCaster"/> type.
        /// </summary>
        /// <param name="target">the target object for call forwarding.</param>
        /// <param name="types">the supported interface types.</param>
        public ActLikeCaster(object target, IEnumerable<Type> types)
            : base(target)
        {
            _interfaceTypes = types.ToList();
        }

#if !SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="ActLikeCaster"/> type.
        /// </summary>
        /// <param name="info">the serialization info.</param>
        /// <param name="context">the streaming context.</param>
        public ActLikeCaster(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

#endif
    }
}
