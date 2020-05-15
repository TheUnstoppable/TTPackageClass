# TTPackageClass
A C# class to read TPI files which was made by TT Team on C&amp;C Renegade.

Example Usages:

- To create an instance of `TTFSDataClass` and clone from a remote TTFS host:
```csharp
Uri url = new Uri("http://path.to/your-ttfs")
TTFSDataClass Data = TTFSClass.FromTTFS(url);
```

- To create an instance of `TTFSDataClass` and parse from a file:
```csharp
string Location = @"C:\Path\To\TTFS\Location\packages.dat";
TTFSDataClass Data = TTFSClass.FromFile(Location);
```

- To create an instance of `TPIPackageClass` and parse from a file:
```csharp
string Location = @"C:\Path\To\TTFS\Location\packages\12345678.tpi";
TPIPackageClass Package = TPIClass.FromFile(Location);
```

Also, you can save your files after modifying however you want like the example below.
```csharp
MemoryStream DataStream = TTFSClass.Save(Data);
MemoryStream PackageStream = TPIClass.Save(Package);

TTFSClass.Save(Data, @"C:\Path\To\File\mypackages.dat");
TPIClass.Save(Package, @"C:\Path\To\File\mytpi.tpi");
```

__Modifying__ parameters of TTFS Data / TPI Packages are **planned** and still a WIP.
