using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonar.Enums
{
    public enum AuthResult
    {
        /// <summary>
        /// Should not happen
        /// </summary>
        Unknown,

        /// <summary>
        /// Login Successful
        /// </summary>
        LoginSuccess,

        /// <summary>
        /// Login failed
        /// </summary>
        LoginFailure,

        /// <summary>
        /// Registration successful
        /// </summary>
        RegisterSuccess,

        /// <summary>
        /// Registration failed
        /// </summary>
        RegisterFailure,

        /// <summary>
        /// An error occured
        /// </summary>
        Error
    }
}
