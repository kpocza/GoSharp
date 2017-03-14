if (!(Test-Path "nuget.exe"))
{
   (New-Object System.Net.WebClient).DownloadFile("https://dist.nuget.org/win-x86-commandline/v4.0.0/nuget.exe", "nuget.exe")
}

dotnet restore ..\GoSharp.sln
msbuild ..\GoSharp.sln /t:Rebuild
msbuild ..\GoSharp.NetFw.sln /t:Rebuild

rm -Rec -Force pck\lib
mkdir pck\lib
mkdir pck\lib\net452
mkdir pck\lib\netstandard1.2

cp ..\src\GoSharp\bin\Debug\GoSharp.dll pck\lib\net452
cp ..\src\GoSharp\bin\Debug\netstandard1.2\GoSharp.dll pck\lib\netstandard1.2

.\nuget pack pck\GoSharp.nuspec
