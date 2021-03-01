using System;

namespace ChatProtocol
{

    public class InvalidMessageException : Exception
    {
        public InvalidMessageException() 
        { 

        }

        public InvalidMessageException(string message) : base(message) 
        { 

        }

        public InvalidMessageException(string message, Exception inner) : base(message, inner) 
        { 

        }
    }
}
