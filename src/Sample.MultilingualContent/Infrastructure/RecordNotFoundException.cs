using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.MultilingualContent
{
    public class RecordNotFoundException : Exception
    {
        public RecordNotFoundException() : base() { }
        public RecordNotFoundException(string message) : base(message) { }
    }

    public class InvalidRequestException: Exception
    {
        public InvalidRequestException() : base() { }
        public InvalidRequestException(string message) : base(message) { }
    }
}
