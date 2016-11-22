namespace Pipes

open Pipes.Conn

open System.IO
open System.Collections.Generic

type ResponseStatus = 
  | Continue = 100
  | SwitchingProtocols = 101
  | Processing = 102
  | Checkpoint = 103
  | Ok = 200 
  | Created = 201 
  | Accepted = 202
  | NonAuthoritativeInformation = 203
  | NoContent = 204
  | ResetContent = 205
  | PartialContent = 206
  | MultipleStatus = 207
  | AlreadyReported = 208
  | IMUsed = 226
  | MultipleChoices = 300
  | MovedPermanently = 301
  | Found = 302
  | SeeOther = 303
  | NotModified = 304
  | UseProxy = 305
  | SwitchProxy = 306
  | TemporaryRedirect = 307
  | PermanentlyRedirect = 308
  | BadRequest = 400
  | Unauthorized = 401
  | PaymentRequired = 402
  | Forbidden = 403
  | NotFound = 404
  | MethodNotAllowed = 405
  | NotAcceptable = 406
  | ProxyAuthenticationRequired = 407
  | RequestTimeOut = 408
  | Conflict = 409
  | Gone = 410
  | LengthRequired = 411
  | PreconditionFailed = 412
  | PayloadTooLarge = 413
  | UriTooLong = 414
  | UnsupportedMediaType = 415
  | RangeNotSatisfiable = 416
  | ExpectationFailed = 417
  | ImATeapot = 418
  | ImAFox = 419
  | EnhanceYourCalm = 420
  | MisdirectedRequest = 421
  | UnprocessableEntity = 422
  | Locked = 423
  | FailedDependency = 424
  | UpgradeRequired = 426
  | PreconditionRequired = 428
  | TooManyRequiests = 429
  | RequestHeaderFieldsTooLarge = 431
  | LoginTimeOut = 440
  | NoResponse = 444
  | RetryWith = 449
  | BlockedByWindowsParentalControls = 450
  | UnavailableForLegalReasons = 451
  | SSLCertificateError = 495
  | SSLCertificateRequired = 496
  | HTTPRequestSentToHTTPSPort = 497
  | ClientCloseRequest = 499
  | InternalServerError = 500
  | NotImplemented = 501
  | BadGateway = 502
  | ServiceUnavailable = 503
  | GatewayTimeOut = 504
  | HTTPVersionNotSupported = 505
  | VariantAlsoNegotiates = 506
  | InsufficientStorage = 507
  | LoopDetected = 508
  | NotExtended = 510
  | NetworkAuthenticationRequired = 511


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
    match conn.TryGetValue(OwinResponseHeaders) with
    | true, h ->
      let headers = h :?> Headers
      match headers.TryGetValue(key) with
      | true, value -> Some value
      | false, _ -> None
    | false, _ -> None

  /// Removes the response header, if it exists and if the connection is not halted.
  let deleteHeader (key : string) (conn : Conn) : Conn = 
    match conn.TryGetValue(OwinResponseHeaders) with
    | true, h -> 
      let headers = h :?> Headers
      if headers.Remove(key) then
        conn.[OwinResponseHeaders] <- headers
        conn
      else conn // Nothing to remove
    | false, _ -> conn

  /// Puts or updates a header into the response header, if it's not halted.
  let putHeader (key : string) (value : string[]) (conn : Conn) : Conn = 
    if not <| isHalted conn then
      match conn.TryGetValue(OwinResponseHeaders) with
      | true, h -> 
        let headers = h :?> Headers
        match headers.TryGetValue(key) with
        | true, _ -> do headers.[key] <- value
        | false, _ -> do headers.Add(key, value)
        do conn.[OwinResponseHeaders] <- headers
        conn
      | false, _ ->
        let headers = Headers()
        do headers.Add(key, value)
        do conn.Add(OwinResponseHeaders, headers)
        conn
    else 
      conn

    // let getCookies (conn : Conn) : Cookies list = 