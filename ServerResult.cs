using System;
using System.Collections.Generic;
using System.Text;

namespace Forgetive.Server
{
    public class ServerResult
    {
        public ServerResult() { }

        public ServerResult(bool issucc, string note)
        {
            IsSuccess = issucc;
            Note = note;
        }

        public bool IsSuccess { get; set; }

        public string Note { get; set; }

        public string InternalEx { get; set; }

        public string Value() => Convert.ToInt32(IsSuccess).ToString() + Note;
    }
}
