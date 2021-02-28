using System;

namespace ChatProtocol
{

    public class InvalidMessageFormatException : Exception
    {
        public InvalidMessageFormatException() 
        { 

        }

        public InvalidMessageFormatException(string message) : base(message) 
        { 

        }

        public InvalidMessageFormatException(string message, Exception inner) : base(message, inner) 
        { 

        }
    }
}
