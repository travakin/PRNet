using System;
using System.Runtime.Serialization;

namespace PRNet.Exceptions {

    [Serializable]
    public class EmptyDatagramException : Exception {
        public EmptyDatagramException() {
        }

        public EmptyDatagramException(string message) : base(message) {
        }

        public EmptyDatagramException(string message, Exception innerException) : base(message, innerException) {
        }

        protected EmptyDatagramException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }

    [Serializable]
    public class EntityNotFoundException : Exception {
        public EntityNotFoundException() {
        }

        public EntityNotFoundException(string message) : base(message) {
        }

        public EntityNotFoundException(string message, Exception innerException) : base(message, innerException) {
        }

        protected EntityNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}