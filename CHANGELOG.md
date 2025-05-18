# v2.0.1

Forgot to add a shutdown method to the HTTP server, so it can be properly disposed of when the game exits. Not sure if this is needed, but it doesn't hurt to have it.

# v2.0.0

Added two new features to MoCore:

- Centralized HTTP server, so plugins can use the same port to listen for requests. See the new `GetHttpHandler` method in IMoPlugin for more information.
	- The short version: In the new method, return an object that implements IMoHttpHandler, and the server will call it when a request comes in. Your code can modify the response from there.
- Moved the Variable system from SlipChat, meaning now your plugin can simply call `VariableHandler.ParseVariables` to parse a string with variables in it, and it will return the string with the variables replaced with their values. This is also available via the HTTP server, for those feeling fancy.

# v1.0.0

Initial release!