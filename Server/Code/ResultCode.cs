using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Code
{
    public class ResultCode : EzAspDotNet.Code.ResultCode
    {
        public ResultCode(int id, string name) : base(id, name)
        {
        }

        public static ResultCode UsingUserId = new(10000, "UsingUserId");
        public static ResultCode UsingNotificationId = new(10002, "UsingNotificationId");
    }
}
