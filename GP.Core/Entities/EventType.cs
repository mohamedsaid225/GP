using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GP.Core.Entities
{
    public enum EventType
    {
        // AI Events
        HeadMovement,
        EyeGaze,
        PhoneDetected,
        MultipleFaces,
        EarpieceDetected,

        // Browser Events
        TabSwitch,
        FullscreenExit,
        FocusLoss
    }
}
