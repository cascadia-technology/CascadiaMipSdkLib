using System;

namespace CascadiaMipSdkLib
{
    public class CreateChannelResult<T>
    {
        public T Channel { get; set; }
        public bool Success { get; set; }
        public Exception Exception { get; set; }
    }
}