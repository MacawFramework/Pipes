namespace Pipes

open System.IO
open System.Collections.Generic

module Conn =   
  /// Generates a new empty Conn 
  let conn() = new Conn()

  /// Assigns a value to a key into the Conn structure. If the value exists, updates it.
  let assign (key : string) (value : obj) (conn : Conn) : Conn =
    if conn.ContainsKey(key) then
      do conn.Remove(key) |> ignore
      do conn.Add(key, value)
      conn
    else
      do conn.Add(key, value)
      conn

  /// Stops some pipelines to be run. 
  /// The idea of halt is to be set when you don't want to modify anything
  /// more into the connection. So, most of the functions will return an 
  /// error if the connection is halted.
  let halt (conn : Conn) : Conn = 
    assign PipesConnectionHalted true conn  

  /// Checks if the conn is stopped.
  let isHalted (conn: Conn) : bool = 
    if conn.ContainsKey(PipesConnectionHalted) then
      conn.[PipesConnectionHalted] :?> bool
    else false
