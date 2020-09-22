using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaleApi.Models.Users
{
    public enum MessageSide
    {
        Support,
        Client
    }
    public enum TemplateType
    {
        IncomingText,
        OutGoingText,
    }
    public enum MessageTarget
    {
        TargetUser,
        Admin,
        Global,
        AllUsersExceptAdmin
    }
    public class ChatMessage
    {
        public TemplateType TemplateType
        {
            get;
            set;
        }
        public MessageTarget Target { get; set; }
        public int ChatMessageId { get; set; }
        public int UserId { get; set; }
        public int TargetUser { get; set; }
        public string Text { get; set; }

        public string ProfileImage { get; set; }

        public string DateTime { get; set; }

        public string Username { get; set; }

        public string OwnerProfileImage { get; set; }
    }
}
