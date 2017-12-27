using System.Collections.Generic;

namespace HSRP
{
    public interface IEventable
    {
        Dictionary<EventType, Event> Events { set; get; }
    }
}
