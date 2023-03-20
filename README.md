

# Quick start

## Download dotnet for windows
### https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-7.0.202-windows-x64-installer

## Install dotnet (double click the exe)

## Run the app
### Go to the directory where you can see the file that ends with .csproj
    cd Stayin.Auth
    
### then type
    dotnet run

## If you don't have and SQL server database and you'd like to test the app
    open the file: appsettings.json
    uncomment the line
    //"Default": "Data Source=.\\Database\\Stayin.db;"
    and comment the next line
    open the file /ApplicationServices/ServiceConfigurationExtensions.cs
    replace .UseSqlServer on line 24 by .UseSqLite
    you're set