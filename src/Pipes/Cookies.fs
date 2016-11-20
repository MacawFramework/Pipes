namespace Pipes

open Pipes.Request
open Pipes.Response

open System

type Cookie = {
  Name : string
  Value : string
  Domain : string option
  HttpOnly : bool
  Secure : bool
  ExpiresAt : DateTime option
}

module Cookies = 

  let cookie (name : string) (value : string) (httpOnly : bool) (secure : bool) (domain : string option) (expiresAt : DateTime option) : Cookie = {
    Name = name; Value = value; HttpOnly = httpOnly; Secure = secure; Domain = domain; ExpiresAt = expiresAt;
  }

  let defaultCookie (name : string) (value : string) : Cookie =
    cookie name value false false None None

