using System;

namespace ChatProtocol
{

    public class MalformedMessageException : Exception
    {
        public MalformedMessageException() 
        { 

        }

        public MalformedMessageException(string message) : base(message) 
        { 

        }

        public MalformedMessageException(string message, Exception inner) : base(message, inner) 
        { 

        }
    }
}
