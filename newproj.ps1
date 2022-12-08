dotnet new globaljson;

dotnet new classlib -o nvul_compiler;
dotnet new WebApi -o nvul_server;
dotnet new Console -o consoleTestApp;
dotnet new xunit -o nvul_server.tests;
dotnet new xunit -o nvul_compiler.tests;

dotnet new sln --name nvul;
dotnet sln add nvul_compiler/nvul_compiler.csproj;
dotnet sln add nvul_server/nvul_server.csproj;
dotnet sln add consoleTestApp/consoleTestApp.csproj;
dotnet sln add nvul_server.tests/nvul_server.tests.csproj;
dotnet sln add nvul_compiler.tests/nvul_compiler.tests.csproj;

dotnet new gitignore;

npx create-react-app nvul_client --template typescript;

New-Item nvul_server/dockerfile;
New-Item nvul_client/dockerfile;
New-Item docker-compose.yml;
New-Item docker-compose.override.yml;
New-Item .dockerignore;
Set-Content .dockerignore '**/.classpath
**/.dockerignore
**/.env
**/.git
**/.gitignore
**/.project
**/.settings
**/.toolstarget
**/.vs
**/.vscode
**/*.*proj.user
**/*.dbmdl
**/*.jfm
**/azds.yaml
**/bin
**/charts
**/docker-compose*
**/Dockerfile*
**/node_modules
**/npm-debug.log
**/obj
**/secrets.dev.yaml
**/values.dev.yaml
LICENSE
README.md'
git init;

pause "Press any key to exit"