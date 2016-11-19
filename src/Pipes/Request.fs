namespace Pipes

open Pipes.Conn

open System.IO
open System.Collections.Generic

module Request = 
  /// Asynchronously returns the body contents as an array of bytes.
  /// TODO: Evaluate if we need a way to stop big request bodies.
  let asyncRead (conn : Conn) : Async<byte [] option> = 
    async {
      if conn.ContainsKey(OwinRequestBody) then
        let b = conn.[OwinRequestBody] :?> Stream
        if isNull b then
          return None
        else
          use ms = new MemoryStream()
          do! b.CopyToAsync(ms) |> Async.AwaitTask
          return Some (ms.ToArray())
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
  
  /// Get the request header by key.
  let getHeader (key : string) (conn : Conn) : string[] option= 
    if conn.ContainsKey(OwinRequestHeaders) then
      let headers = conn.[OwinRequestHeaders] :?> Dictionary<string, string[]>
      if headers.ContainsKey(key) then
        Some headers.[key]
      else None
    else None

  /// Removes the request header, if it exists.
  let deleteHeader (key : string) (conn : Conn) : Conn = 
    if conn.ContainsKey(OwinRequestHeaders) then
      let headers = conn.[OwinRequestHeaders] :?> Dictionary<string, string[]>
      if headers.Remove(key) then
        conn.[OwinRequestHeaders] <- headers
        conn
      else conn // Nothing to remove
    else conn // The header wasn't set. Wierd.

  /// Puts or updates a header into the request.
  let putHeader (key : string) (value : string[]) (conn : Conn) : Conn = 
    if conn.ContainsKey(OwinRequestHeaders) then
      let headers = conn.[OwinRequestHeaders] :?> Dictionary<string, string[]>
      if headers.ContainsKey(key) then
        do headers.[key] <- value
      else
        do headers.Add(key, value)
      conn.[OwinRequestHeaders] <- headers
      conn
    else // The header wasn't set. Wierd.
      let header = new Dictionary<string, string[]>()
      do header.Add(key, value)
      conn.Add(OwinRequestHeaders, header)
      conn