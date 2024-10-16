### The Cat API Stealer
A `.Net Core Api` responsible for seeding an MySQL server database with objects representing Cat Images. The Cat data are stored on the SQL database while the image itself is stored on the local server hosting the application.


### Prerequisites

- Visual Studio 2022
- .Net Core 8
- .Net Entity Framework 8 
- SQL Server 2022

** Database Setup **
1. Start Database,
2. Start the CatsApi solution. 
3. In Package Manager Console run the following three commands: 
- Add-Migration CatsApiMigration
- Update-Database

**Local Development** 
- Configure the application with the proper connection string to connect to SQL server
```
Server=tcp:127.0.0.1,1433;Initial Catalog=natech;Persist Security Info=False;User ID=SA;Password=<DB_PASSWORD_FOR_USER_SA>;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=False;Connection Timeout=30;
```
- Start the Application

**Swagger Available**
```
http://localhost:5190/swagger/index.html
```