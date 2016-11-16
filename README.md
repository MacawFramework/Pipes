# Pipes

Pipes is a library to handle the OWIN environment's mutations in a nicer way. 
Most of the functions in this modules receives a `Conn` type and returns it, 
mutated, so you can build pipelines of functions.

The objective of this library is to support fully the OWIN specification - 
including the extensions - and provide types and functions to handle most of the 
needs of web developers. 