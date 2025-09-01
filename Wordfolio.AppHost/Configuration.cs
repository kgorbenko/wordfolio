namespace Wordfolio.AppHost;

public class Configuration
{
    public const string DatabaseOptionsSection = "DatabaseOptions";
}

public record DatabaseOptions(string DataBindMount, int Port);