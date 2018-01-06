using System;
using System.Collections.Generic;

namespace HSRP
{
    public interface IEventable
    {
        List<Tuple<EventType, Event>> Events { set; get; }
        string[] Immunities { get; set; }
    }
}
