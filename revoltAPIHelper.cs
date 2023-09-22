public class SessionInfo
{
    public string token { get; set; }
    public string user_id { get; set; }
    public string alertzyKey { get; set; }
}

public class id
{
    public string channel { get; set; }
}

public class MFAResponse
{
    public string totp_code { get; set; }
}

public class LoginRequest
{
    public string email { get; set; }
    public string password { get; set; }
    public string friendly_name { get; set; }
    public string mfa_ticket { get; set; }
    public MFAResponse mfa_response { get; set; }
}

public class LoginResponse
{
    public string result { get; set; }
    public string user_id { get; set; }
    public string token { get; set; }
    public string ticket { get; set; }
}

public class UnreadMessage
{
    public id _id { get; set; }
    public string? last_id { get; set; }
}

public class Message
{
    public string _id { get; set; }
    public string channel { get; set; }
    public string author { get; set; }
    public string content { get; set; }
}

public class User
{
    public string username { get; set; }
    public Status status { get; set; }
}

public class Status
{
    public string presence { get; set; }
}

public class AlertzyResponse
{
    public string response { get; set; }
}
