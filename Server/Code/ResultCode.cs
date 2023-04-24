
namespace Server.Code
{
    public class ResultCode : EzAspDotNet.Protocols.Code.ResultCode
    {
        private ResultCode(int id, string name) : base(id, name)
        {
        }

        public static readonly ResultCode UsingUserId = new(10000, "UsingUserId");
        public static readonly ResultCode UsingNotificationId = new(10002, "UsingNotificationId");
        public static readonly ResultCode NotFoundSummoner = new(10003, "NotFoundSummoner");

        public static readonly ResultCode NotFoundNotification = new(10004, "NotFoundNotification");
    }
}
