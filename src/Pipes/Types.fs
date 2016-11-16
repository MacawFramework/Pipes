namespace Pipes

open System.Collections.Generic

[<AutoOpen>]
module Types = 

  type Conn = Dictionary<string, obj>

  type RequestMethod = GET | HEAD | POST | PUT | DELETE | OPTION | TRACE | PATCH | UNKNOWN

  (* OWIN strings *)
  let [<Literal>] OwinRequestMethod = "owin.RequestMethod"
  let [<Literal>] OwinRequestHeaders = "owin.RequestHeaders"
  let [<Literal>] OwinRequestBody = "owin.RequestBody"