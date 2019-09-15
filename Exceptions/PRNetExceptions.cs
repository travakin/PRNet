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

	[Serializable]
	public class NullPacketException : Exception {

		public NullPacketException() {
		}

		public NullPacketException(string message) : base(message) {
		}

		public NullPacketException(string message, Exception innerException) : base(message, innerException) {
		}

		protected NullPacketException(SerializationInfo info, StreamingContext context) : base(info, context) {
		}
	}

	[Serializable]
	public class UnidentifiedPacketException : Exception {

		public UnidentifiedPacketException() {
		}

		public UnidentifiedPacketException(string message) : base(message) {
		}

		public UnidentifiedPacketException(string message, Exception innerException) : base(message, innerException) {
		}

		protected UnidentifiedPacketException(SerializationInfo info, StreamingContext context) : base(info, context) {
		}
	}
}