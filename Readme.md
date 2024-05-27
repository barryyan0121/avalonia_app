# Cross-Platform .Net Application
This is a cross-platform .NET application that can run on Windows, Linux, and MacOS. It is a simple application that 
reads from a MySQL database and displays the production data in various views. The application is written in C# and 
uses the open source framework called [AvaloniaUI](https://avaloniaui.net/).

## Installation
To install the application, download the latest .NET Core SDK from [here](https://dotnet.microsoft.com/download) and 
install the [Avalonia templates](https://avaloniaui.net/GettingStarted#installation) using visual studio / JetBrains Rider Extension, or using .NET CLI:
```bash
dotnet new install Avalonia.Templates
```

## Start the Mysql Docker Container
To start the MySQL database, navigate to the `Dockerfile` directory and run the following command:
```bash
docker build -t my-mysql-image .
```
After the image is built, run the following command to start the MySQL container:
```bash
docker run -d --name mysql-container -p 3306:3306 my-mysql-image
```
To stop the MySQL container, run the following command:
```bash
docker stop mysql-container
```

