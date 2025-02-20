﻿namespace AspNetAuth.SSO.AuthorizationServer;

public class JwtTokenOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SecurityKey { get; set; } = string.Empty;
}
