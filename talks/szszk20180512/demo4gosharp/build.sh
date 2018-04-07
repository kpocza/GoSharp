#! /bin/bash

pushd .
cd ../../../src
dotnet restore GoSharp.sln
dotnet build GoSharp.sln
popd
cp ../../../src/GoSharp.PerfTest/bin/Debug/netcoreapp2.0/* .

