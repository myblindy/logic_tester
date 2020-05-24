using System;
using System.Collections.Generic;
using System.Text;

namespace logic
{
    public interface ILogic
    {
        public int Region { get; set; }
        public int Counter { get; set; }
        public int State { get; set; }

    }
}
