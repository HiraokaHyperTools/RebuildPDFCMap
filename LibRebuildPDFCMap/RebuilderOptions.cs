using System;
using System.Collections.Generic;
using System.Text;

namespace LibRebuildPDFCMap
{
    public class RebuilderOptions
    {
        public Action<string> Logger { get; set; }
        public bool ForceRebuildCMap { get; set; }
    }
}
