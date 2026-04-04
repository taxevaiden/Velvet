namespace Velvet.Audio
{
    /// <summary>Thrown when a <see cref="VelvetAudioEngine"/> operation fails.</summary>
    public sealed class AudioException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="AudioException"/> class with a specified error message.</summary>
        public AudioException(string message) : base(message) { }

        /// <summary>Initializes a new instance of the <see cref="AudioException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
        public AudioException(string message, Exception inner) : base(message, inner) { }
    }
}