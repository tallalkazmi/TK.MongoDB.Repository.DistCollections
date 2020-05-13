using System;

namespace TK.MongoDB.Distributed.Models
{
    /// <summary>
    /// Use this attribute on properties to be distributed over multiple collections
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class DistributeAttribute : Attribute
    {
        /// <summary>
        /// Zero-based sub-division distribution level. Zero is the top level and does not needs to be defined.
        /// </summary>
        public int Level { get; set; }
    }
}
