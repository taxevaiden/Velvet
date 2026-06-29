namespace Velvet.Windowing
{
    /// <summary>Thrown when a <see cref="VelvetWindow"/> operation fails.</summary>
    public sealed class WindowingException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="WindowingException"/> class with a specified error message.</summary>
        public WindowingException(string message) : base(message) { }

        /// <summary>Initializes a new instance of the <see cref="WindowingException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
        public WindowingException(string message, Exception inner) : base(message, inner) { }
    }
}