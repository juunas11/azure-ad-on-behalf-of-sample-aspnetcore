namespace ApiOnBehalfSample.Options
{
    public class AuthenticationOptions
    {
        public string ClientId { get; set; }
        public string AppIdUri { get; set; }
        public string AadInstance { get; set; }
        public string Tenant { get; set; }
        public string Authority => AadInstance + Tenant;

        public string ClientSecret { get; set; }
    }
}
