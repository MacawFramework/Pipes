namespace Pipes

open Pipes.Conn

open System
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
      let rMethod = conn.[OwinRequestMethod] :?> string
      match rMethod.ToLowerInvariant() with
      | "get" -> GET
      | "head" -> HEAD
      | "post" -> POST
      | "put" -> PUT
      | "delete" -> DELETE
      | "option" -> OPTION
      | "trace" -> TRACE
      | "patch" -> PATCH
      | _ -> UNKNOWN
    else UNKNOWN
  
  /// Get the value in the header by key.
  let getHeader (key : string) (conn : Conn) : string[] option= 
    match conn.TryGetValue(OwinRequestHeaders) with
    | true, h ->
      let headers = h :?> Headers
      match headers.TryGetValue(key) with
      | true, value -> Some value
      | false, _ -> None
    | false, _ -> None

  /// Removes the header, if it exists.
  let deleteHeader (key : string) (conn : Conn) : Conn = 
    match conn.TryGetValue(OwinRequestHeaders) with
    | true, h -> 
      let headers = h :?> Headers
      if headers.Remove(key) then
        conn.[OwinRequestHeaders] <- headers
        conn
      else conn // Nothing to remove
    | false, _ -> conn

  /// Puts or updates a header into the request.
  let putHeader (key : string) (value : string[]) (conn : Conn) : Conn = 
    match conn.TryGetValue(OwinRequestHeaders) with
    | true, h -> 
      let headers = h :?> Headers
      match headers.TryGetValue(key) with
      | true, _ -> do headers.[key] <- value
      | false, _ -> do headers.Add(key, value)
      do conn.[OwinRequestHeaders] <- headers
      conn

    | false, _ ->
      let headers = Headers()
      do headers.Add(key, value)
      do conn.Add(OwinRequestHeaders, headers)
      conn