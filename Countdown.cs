using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Encodings
{
    /// <summary>
    /// Threads synchronizer. Analog of CountdownEvent class in .NET 4
    /// </summary>
    public class CountdownEvent
    {
        object _locker = new object();
        int _value;

        public CountdownEvent() { }
        public CountdownEvent(int initialCount) { _value = initialCount; }

        public void Signal() { AddCount(-1); }

        public void AddCount(int amount)
        {
            lock (_locker)
            {
                _value += amount;
                if (_value <= 0) Monitor.PulseAll(_locker);
            }
        }

        public void Wait()
        {
            lock (_locker)
                while (_value > 0)
                    Monitor.Wait(_locker);
        }
    }
}
