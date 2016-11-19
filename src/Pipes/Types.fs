namespace Pipes

open System.Collections.Generic

[<AutoOpen>]
module Types = 
  type Configuration = {
    Debug: bool;
    MaxRequestBodySize: int
  }

  let defaultConfiguration = {
    Debug = true;
    MaxRequestBodySize = 8;
  }

  type Conn = Dictionary<string, obj>
  type RequestMethod = GET | HEAD | POST | PUT | DELETE | OPTION | TRACE | PATCH | UNKNOWN
  type ResponseStatus = 
    | OK = 200 
    | CREATED = 201 
    | DELETED = 202

  type Error = {
    Message : string
    Status : ResponseStatus
  }

  type Pipe = 
    | Success of Conn 
    | Error of Conn * Error

  /// Bind function.
  let (>>=) (pipe : Pipe) (f : Conn -> Pipe) : Pipe = 
    match pipe with
    | Success conn -> f conn
    | Error (conn, err) -> Error (conn, err)

  /// Map function.
  let (<!>) (pipe : Pipe) (f : Conn -> Conn) : Pipe =
    match pipe with
    | Success conn -> Success (f conn)
    | Error (conn, err) -> Error (conn, err)

  /// Unlifts from the elevated world.
  let (=!>) (pipe : Pipe) (f : Conn -> 'T) : 'T =
    match pipe with
    | Success conn -> f conn
    | Error (conn, _) -> f conn

  (* Pipes internal strings *)
  let [<Literal>] PipesConfiguration = "pipes.Configuration"
  let [<Literal>] PipesConnectionHalted = "pipes.ConnectionHalted"

  (* OWIN strings *)
  let [<Literal>] OwinRequestMethod = "owin.RequestMethod"
  let [<Literal>] OwinRequestHeaders = "owin.RequestHeaders"
  let [<Literal>] OwinRequestBody = "owin.RequestBody"
  let [<Literal>] OwinResponseBody = "owin.ResponseBody"
  let [<Literal>] OwinResponseHeaders = "owin.ResponseHeaders"
  let [<Literal>] OwinResponseStatusCode = "owin.ResponseStatusCode"