namespace Wordfolio.AppHost;

public class Configuration
{
    public const string DatabaseOptionsSection = "DatabaseOptions";
    public const string FixedFrontendPortOptionsSection = "FixedFrontendPortOptions";
}

public record DatabaseOptions(string DataBindMount, int Port);

public record FixedFrontendPortOptions(int FrontendPort);
