using System;
using System.Runtime.Serialization;

namespace FacturadorEstacionesAPI.Filters
{
    [Serializable]
    internal class DataApiException : Exception
    {
        public DataApiException()
        {
        }

        public DataApiException(string message) : base(message)
        {
        }

        public DataApiException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DataApiException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}