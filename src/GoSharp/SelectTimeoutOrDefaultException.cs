using System;

namespace Go
{
    public class SelectTimeoutOrDefaultException : Exception
    {
        public SelectTimeoutOrDefaultException() : base("Either timeout or default case can be specified but not both of them.")
        {
            
        }
    }
}
