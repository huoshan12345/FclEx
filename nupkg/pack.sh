#!/bin/bash
packFolder=$PWD
slnPath="$packFolder/.."
srcPath="$slnPath/src"

projects=(
"FclEx"
"FclEx.Http"
"FclEx.Npoi"
)

rm -f *.nupkg
cd $slnPath

dotnet restore
dotnet clean -c Release

echo "project count: ${#projects[@]}"
for project in "${projects[@]}"; do 
	echo "packing for [$project]"
	projectFolder="$srcPath/$project"
    cd $projectFolder	
	dotnet pack -c Release --include-symbols -v m --output $packFolder
done

cd $packFolder
