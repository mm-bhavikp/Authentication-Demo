namespace Authentication_Application.ViewModels
{
    public class EnableAuthenticatorViewModel
    {
        public string Username { get; set; }
        public string SecretKey { get; set; }
        public string AuthenticatorUri { get; set; }
        public string Code { get; set; }
    }
}
