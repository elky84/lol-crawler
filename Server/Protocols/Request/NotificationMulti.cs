using System.Collections.Generic;

namespace Server.Protocols.Request
{
    public class NotificationMulti
    {
        public List<Common.NotificationCreate> Datas { get; set; }
    }
}
