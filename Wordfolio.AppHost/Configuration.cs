namespace Wordfolio.AppHost;

public class Configuration
{
    public const string DatabaseOptionsSection = "DatabaseOptions";
    public const string FixedPortOptionsSection = "FixedPortOptions";
}

public record DatabaseOptions(string DataBindMount, int Port);

public record FixedPortOptions(int ApiPort, int FrontendPort);
