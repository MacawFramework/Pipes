namespace Pipes

open Pipes.Conn

open System.IO
open System.Collections.Generic

module Response =
  /// Asynchronously writes an array of bytes into the Response
  /// body. It also sets the connection as halt, since you shouldn't
  /// edit any response related keys (because they were already sent). 
  let asyncWrite (toWrite : byte[]) (conn : Conn) = 
    async {
      if conn.ContainsKey(OwinResponseBody) && not <| isHalted conn then
        let b = conn.[OwinResponseBody] :?> Stream
        do! b.AsyncWrite(toWrite)
        do halt conn |> ignore
        return ()
      else
        do halt conn |> ignore
        return ()
    }

  /// Sets the status code to be sent to the client, if the connection is not halted. 
  let setStatus (status : int) (conn : Conn) : Conn =
    if isHalted conn then
      conn
    else
      if conn.ContainsKey(OwinResponseStatusCode) then
        do conn.[OwinResponseStatusCode] <- status
        conn
      else
        do conn.Add(OwinResponseStatusCode, status)
        conn


  /// Get the response header by key.
  let getHeader (key : string) (conn : Conn) : string[] option= 
    if conn.ContainsKey(OwinResponseHeaders) then
      let headers = conn.[OwinResponseHeaders] :?> Dictionary<string, string[]>
      if headers.ContainsKey(key) then
        Some headers.[key]
      else None
    else None

  /// Removes the response header, if it exists and if the connection is not halted.
  let deleteHeader (key : string) (conn : Conn) : Conn = 
    if conn.ContainsKey(OwinResponseHeaders) && not <| isHalted conn then
      let headers = conn.[OwinResponseHeaders] :?> Dictionary<string, string[]>
      if headers.Remove(key) then
        conn.[OwinResponseHeaders] <- headers
        conn
      else conn // Nothing to remove
    else conn // The header wasn't set. Wierd.

  /// Puts or updates a header into the response header, if it's not halted.
  let putHeader (key : string) (value : string[]) (conn : Conn) : Conn = 
    if not <| isHalted conn then
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
    else 
      conn