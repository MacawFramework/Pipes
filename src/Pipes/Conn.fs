namespace Pipes

open System.IO
open System.Collections.Generic

module Conn =   
  /// Generates a new empty Conn 
  let conn() = new Conn()

  /// Assigns a key, value into the Conn structure.
  let assign (key : string) (value : obj) (conn : Conn) : Conn = 
    conn.Add(key, value)
    conn

  module Request = 
    /// Asynchronously returns the body contents.
    let body (conn : Conn) : Async<byte [] option> = 
      async {
        if conn.ContainsKey(OwinRequestBody) then
          let b = conn.[OwinRequestBody] :?> Stream
          if isNull b then
            return None
          else 
            let mutable data = Array.zeroCreate (b.Length :> int)
            do! b.AsyncRead(data)
            return Some data
        else return None
      }

    /// Returns the request method. If it cannot figure out which one,
    /// returns unknown.
    let method (conn : Conn) : RequestMethod = 
      if conn.ContainsKey(OwinRequestMethod) then
        match conn.[OwinRequestMethod] :?> string with
        | "GET" | "get" -> GET
        | "HEAD" | "head" -> HEAD
        | "POST" | "post" -> POST
        | "PUT" | "put" -> PUT
        | "DELETE" | "delete" -> DELETE
        | "OPTION" | "option" -> OPTION
        | "TRACE" | "trace" -> TRACE
        | "PATCH" | "patch" -> PATCH
        | _ -> UNKNOWN
      else UNKNOWN
    
    /// Removes the request header, if it exists.
    let deleteHeader (key : string) (conn : Conn) : Conn = 
      if conn.ContainsKey(OwinRequestHeaders) then
        let headers = conn.[OwinRequestHeaders] :?> Dictionary<string, string[]>
        if headers.Remove(key) then
          conn.[OwinRequestHeaders] <- headers
          conn
        else conn // Nothing to remove
      else conn // The header wasn't set. Wierd.
  
    /// Puts a header into the request.
    let putHeader (key : string) (value : string[]) (conn : Conn) : Conn = 
      if conn.ContainsKey(OwinRequestHeaders) then
        let headers = conn.[OwinRequestHeaders] :?> Dictionary<string, string[]>
        do headers.Add(key, value)
        conn.[OwinRequestHeaders] <- headers
        conn
      else // The header wasn't set. Wierd.
        let header = new Dictionary<string, string[]>()
        do header.Add(key, value)
        conn.Add(OwinRequestHeaders, header)
        conn


  module Response =
    let foo = 0
