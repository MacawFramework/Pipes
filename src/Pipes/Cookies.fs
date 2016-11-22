namespace Pipes

open System
open System.Globalization
open System.Net

type Cookie = {
  Name : string
  Value : string
  Domain : string option
  Path : string option
  HttpOnly : bool
  Secure : bool
  ExpiresAt : DateTime option
}

module Cookies = 

  let cookie (name : string) (value : string) (httpOnly : bool) 
             (secure : bool) (path : string option) (domain : string option) 
             (expiresAt : DateTime option) : Cookie = {
    Name = name; 
    Value = value; 
    HttpOnly = httpOnly; 
    Secure = secure; 
    Path = path;
    Domain = domain; 
    ExpiresAt = expiresAt;
  }

  let defaultCookie (name : string) (value : string) : Cookie =
    cookie name value false false None None None

(* 
  let private fromString (cookieString : string) : Cookie = 
    let splitCookie = cookieString.Split([|';'|], StringSplitOptions.RemoveEmptyEntries)
    let cookie = defaultCookie "foo" "bar"
    Array.fold(fun acc str -> 
      let s = str.Split('=')
      if isNull s then
        match str with
        | " Secure" -> { acc with Secure = true }
        | " HttpOnly" -> { acc with HttpOnly = true }
      else 
        match s.[0] with
        | " path" -> { acc with Path = Some s.[1] }
        | " domain" -> { acc with Domain = Some s.[1] }
      
*)
  let private toString (cookie: Cookie) : string = 
    let encodeString value = WebUtility.UrlEncode(value)

    let name = encodeString cookie.Name
    let value = "=" + (encodeString cookie.Value)

    let path = 
      match cookie.Path with
      | Some "" | None -> "; path=/"
      | Some path -> "; path=" + path
      

    let expiresAt = 
      match cookie.ExpiresAt with
      | Some expAt -> 
        "; expires="
        + expAt.ToUniversalTime().ToString("ddd, dd-MMM-yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo)
        + " GMT"
      | None -> String.Empty

    let domain = 
      match cookie.Domain with
      | Some domain -> "; domain=" + domain
      | None -> String.Empty

    let secure = if cookie.Secure then "; Secure" else String.Empty
    let httpOnly = if cookie.HttpOnly then "; HttpOnly" else String.Empty

    name + value + path + expiresAt + domain + secure + httpOnly

  let setCookie (cookie : Cookie) (headers : Headers) : Headers = 
    match headers.TryGetValue("Cookie") with
    | true, unparsedCookies -> headers
    | false, _ -> headers

  let getCookies (headers : Headers) : Cookie list option = 
    match headers.TryGetValue("Cookie") with
    | true, unparsedCookies ->
      if unparsedCookies.Length > 0 then
        Some []
      else None
    | false, _ -> None