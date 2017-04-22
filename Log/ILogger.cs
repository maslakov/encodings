using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Encodings.Log
{
    public interface ILogger : IDisposable
    {
        void Log(String message);
    }
}
