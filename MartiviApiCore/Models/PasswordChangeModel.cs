using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaleApiCore.Models
{
    public class PasswordChangeStore
    {
        public int PasswordChangeStoreId { get; set; }
        public int Code { get; set; }
        public string Hash { get; set; }
        public long PasswordTime { get; set; }
    }
    public class PasswordChangeRequestModel
    {
        public int Code { get; set; }
        public string Username { get; set; }
        public string NewPassword { get; set; }
    }
    public class RequestPasswordRecoveryCodeModel
    {
        public string Username { get; set; }
    }
    public enum Result
    {
        PasswordChanged = 0,
        CodeSent = 1,
        InvalidCode = 2,
        CodeOutOfDated = 3,
        UserNotFound = 4,
        UnknownError = 5
    }
    public class PasswordChangeResult
    {
        public Result Error { get; set; }
        public string Message { get; set; }
    }
}
