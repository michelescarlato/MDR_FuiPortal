using Microsoft.Extensions.Configuration;

namespace MDR_FuiPortal.Server;

public class Credentials : ICredentials
{
    private readonly IConfiguration _configuration;

    public Credentials(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetConnectionString(string db_name)
    {
        var connectionString = _configuration.GetConnectionString(db_name);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException($"Connection string for '{db_name}' not found in configuration.");
        }
        return connectionString;
    }
}