using System;
using System.Collections.Generic;
using System.Text;

namespace TTPackageClass
{
    public class TPIParserException : Exception
    {
        public TPIParserException(string Message, Exception InnerException = null) : base (Message, InnerException)
        {
            if(InnerException != null)
            {
                Message += "\nPlease view Inner Exception for the source of exception.";
            }
        }
    }
}
