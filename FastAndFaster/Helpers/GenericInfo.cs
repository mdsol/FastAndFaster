using System;

namespace FastAndFaster.Helpers
{
    public class GenericInfo
    {
        /// <summary>
        /// The indexes of generic type in the parameters of a method. 
        /// </summary>
        public int[] GenericTypeIndex { get; set; } = new int[0];

        /// <summary>
        /// The concrete types of a generic method.
        /// </summary>
        public Type[] GenericType { get; set; } = new Type[0];
    }
}
