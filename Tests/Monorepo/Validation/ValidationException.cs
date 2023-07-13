using System;
using System.Runtime.Serialization;

namespace TechTonic.Monorepo.Validation.UnitTests
{
    /// <summary>
    /// Represents errors that occur during Impact Analysis
    /// </summary>
    [Serializable]
    public class ValidationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class.
        /// </summary>
        public ValidationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        public ValidationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="inner">The inner exception</param>
        public ValidationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class.
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        protected ValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
