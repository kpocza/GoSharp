if (!(Test-Path "nuget.exe"))
{
   (New-Object System.Net.WebClient).DownloadFile("https://dist.nuget.org/win-x86-commandline/v4.0.0/nuget.exe", "nuget.exe")
}

dotnet restore ..\src\GoSharp.sln
msbuild ..\src\GoSharp.sln /t:Rebuild /p:Configuration=Release
msbuild ..\src\GoSharp.NetFw.sln /t:Rebuild /p:Configuration=Release

rm -Rec -Force pck\lib
mkdir pck\lib
mkdir pck\lib\net452
mkdir pck\lib\netstandard2.0

cp ..\src\GoSharp\bin\Release\GoSharp.dll pck\lib\net452
cp ..\src\GoSharp\bin\Release\netstandard2.0\GoSharp.dll pck\lib\netstandard2.0

.\nuget pack pck\GoSharp.nuspec
