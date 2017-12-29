using System;

namespace HSRP
{
    [Flags]
    public enum TargetType
    {
        None = 0,
        Self = 1,
        Target = 2,
        All = 4
    }
}
