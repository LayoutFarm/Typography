//MIT, 2015, Michael Popoloski's SharpFont

using System;

namespace Typography.OpenFont
{

    class InvalidFontException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidFontException"/> class.
        /// </summary>
        public InvalidFontException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidFontException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public InvalidFontException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidFontException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public InvalidFontException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
