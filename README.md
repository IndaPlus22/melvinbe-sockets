# Socket League

Like Rocket League but with less dimensions and no thread safety.

## Controls

* WASD: Drive
* Space: Dash
* LShift: Boost 

## Instructions

### Build:
* Have .NET 6.0 installed.
* You probably need to have <b>MonoGame.Framework.DesktopGL</b> and <b>MonoGame.Content.Builder.Task</b> installed as NuGet packages.
* Open the project solution in at least Visual Studio 2019 (any lower may or may not work).
* Build the application through Visual studio.

Note: If you want to run the server on any other IP address than LocalHost, you will have to change it manually in code :)
 
### Run:
* To start a server: Run the Server.exe file located at <b>\Server\bin\Release\net6.0\Server.exe</b>.
* To start a client: Run the SocketLeague.exe located at <b>\Server\bin\Release\net6.0\SocketLeague.exe</b>.

## Possible Improvements
* Thread safety in this program is basically non-existant, but it seems to work anyways. 
* The server was originally supposed to support multiple games at once but I ran out of time
* Some UI to keep scores would also be nice