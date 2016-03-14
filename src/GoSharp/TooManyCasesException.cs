using System;

namespace Go
{
    public class TooManyCasesException : Exception
    {
        public TooManyCasesException() : base("At most 64 select cases can be specified")
        {
            
        }
    }
}
