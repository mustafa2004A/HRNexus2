## Database Setup

1. Open SQL Server Management Studio.
2. Restore the database backup file from:

Database/HRNexus.bak

3. Make sure the database name is:

HRNexusDb

4. Update the connection string in:

HRNexus.API/appsettings.json

Example:

"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=HRNexusDb;Trusted_Connection=True;TrustServerCertificate=True;"
}

5. Run the project from Visual Studio or using:

dotnet run --project HRNexus.API
