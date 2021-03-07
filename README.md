# FileCatalog
[![MIT License][license-shield]][license-url]
[![LinkedIn][linkedin-shield]][linkedin-url]


<!-- ABOUT THE PROJECT -->
## About The Project

A Simple API to manage a catalog of files demonstrating how to:

* Store files in database tables
* Using Open API 3 spec to download and upload files directly from Swagger
* Limit upload by file size

### Built With

* [.NET Core 3.1](https://dotnet.microsoft.com/)
* [Swagger](https://swagger.io/)
* [Dapper](https://github.com/StackExchange/Dapper)
* [MS SQL Server](https://www.microsoft.com/en-us/sql-server/)


## Getting Started

How to put all this stuff to run?

### Prerequisites

Firstly, you'll need:
1. [Visual Studio](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/) installed.
1. A SQL Server database instance accessible and running.
1. Some old files to start uploading and playing with.

### And Now What?

1. Clone this repo.
2. Open the solution in your IDE.
3. Edit following **appsettings.development.json** entries:
```json
  "ConnectionStrings": {
    "SqlServer": "< PUT YOUR CONNECTION STRING HERE >"
  },
  "SupportedFileTypes": [
    "application/pdf",
    "text/plain",
    .
    .
    .
    "< ADD ANY MIME TYPES YOU WANT >"
  ],
  "DBSeedFolder": "C:\\SomeFolderFullOfFilesToPopulateDatabase"
```
4. Have fun!

**Note:** App will try to populate data every time it connects to a DB without predefined tables.
If you don't want this to happen, open **Startup.cs** and comment this line:
```csharp
    app.BuildDatabase(Configuration);
```



<!-- USAGE EXAMPLES -->
## Usage

Press F5, wait until a Swagger page opens in your browser and follow instructions. 
All you need it there and in the code.



<!-- LICENSE -->
## License

Distributed under the MIT License (AKA "Do whatever you want" license).



<!-- CONTACT -->
## Contact

Ricardo (Rick) Alves
<br/>[ricardo.alkain@gmail.com](mailto:ricardo.alkain@gmail.com) | 
[Linkedin](https://www.linkedin.com/in/ricardo-alkain/)




<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[license-shield]: https://img.shields.io/github/license/othneildrew/Best-README-Template.svg?style=for-the-badge
[license-url]: https://github.com/ricardoalkain/FileCatalog/LICENSE.txt
[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=for-the-badge&logo=linkedin&colorB=555
[linkedin-url]:  https://www.linkedin.com/in/ricardo-alkain/

