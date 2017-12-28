using System;

namespace HSRP
{
    [Flags]
    public enum TargetType
    {
        Self = 1,
        Target = 2,
        All = 4
    }
}
