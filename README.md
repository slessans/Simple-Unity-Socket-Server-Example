Simple Unity Socket Server Example
==================================

Example of using unity socket server, library and docs can be found at [https://github.com/slessans/unity-socket-server/](https://github.com/slessans/unity-socket-server/)


How to Use
==========

Open the only scene and run the project. In the console you will see (along with probably a lot of 
other debug output) the host and port number on which to connect. Connect to this and the server will
start reading.

Send position values (3-tuples of floats, separated by "&") and the cube in the scene will update it's position
accordingly.

An example sequence of tokens could be "0.0,0.0,0.0&1.0,1.0,1.0&". Remember if sending only one token, the separator 
must still be at the end or it won't be read.
