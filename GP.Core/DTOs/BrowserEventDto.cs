using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GP.Core.DTOs
{
    public class BrowserEventDto
    {
        public string EventType { get; set; } // "TabSwitch", "FullscreenExit", "FocusLoss"
        public string? MetadataJson { get; set; }
    }
}
