# Setup development environment

1. Specify username and password for the database:

    `Wordfolio.AppHost> dotnet user-secrets set Parameters:postgres-username <postgres-username>`

    `Wordfolio.AppHost> dotnet user-secrets set Parameters:postgres-password <postgres-password>`

2. Execute identity migrations:

    `dotnet ef database update --startup-project .\Wordfolio.Api\Wordfolio.Api\Wordfolio.Api.fsproj --connection "Host=localhost;Port=5432;Database=wordfoliodb;User ID=<postgres-username>;Password=<postgres-password>"`

3. Execute wordfolio migrations:

    ` dotnet fm migrate -p PostgreSQL15_0 -a ".\Wordfolio.Api\Wordfolio.Api.Migrations\bin\Debug\net9.0\Wordfolio.Api.Migrations.dll" -c "Host=localhost;Port=5432;Database=wordfoliodb;User ID=<postgres-username>;Password=<postgres-password>"`