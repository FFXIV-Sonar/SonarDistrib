using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarUtils
{
    public class HappySocketException : Exception
    {
        public HappySocketException() : base() { }
        public HappySocketException(string? message) : base(message) { }
        public HappySocketException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
