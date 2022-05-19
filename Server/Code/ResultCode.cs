
namespace Server.Code
{
    public class ResultCode : EzAspDotNet.Protocols.Code.ResultCode
    {
        public ResultCode(int id, string name) : base(id, name)
        {
        }

        public readonly static ResultCode UsingUserId = new(10000, "UsingUserId");
        public readonly static ResultCode UsingNotificationId = new(10002, "UsingNotificationId");
        public readonly static ResultCode NotFoundSummoner = new(10003, "NotFoundSummoner");

        public readonly static ResultCode NotFoundNotification = new(10004, "NotFoundNotification");
    }
}
