using System;

namespace Sample.MultilingualContent
{
    public abstract class SomethingWrongException : Exception
    {
        public SomethingWrongException(string message) : base(message) { }

        public abstract object GetDetails();
    }

    public class SomethingWrongException<T> : SomethingWrongException where T : class
    {
        public SomethingWrongException(string message, T details) : base(message)
        {
            this.Details = details;
        }

        public T Details { get; init; }

        public override T GetDetails()
        {
            return Details;
        }
    }
}
