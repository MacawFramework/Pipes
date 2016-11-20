namespace Pipes

open System.Security.Cryptography
open System.Text
open System

module Crypto =
  /// Generates random bytes.
  let randomKey (size : int) : byte[] =
    use provider = RandomNumberGenerator.Create()
    let mutable bytes = Array.zeroCreate<byte> size
    do provider.GetBytes(bytes)
    bytes

  /// Provides key generation using PBKDF2 / Rfc2898.
  let randomPassPhrase (pass : string) (salt : byte[]) (size : int) (itr : int) : byte[] =
    use provider = 
      if itr < 10000 then 
        new Rfc2898DeriveBytes(pass, salt, 10000) 
      else 
        new Rfc2898DeriveBytes(pass, salt, itr)
    provider.GetBytes(size)

  // Should these two be static?
  let private key = randomKey 32
  let private iv = randomKey 16

  /// Encrypts data using AES.
  let encrypt (data : string) : string =
    use provider = Aes.Create()
    use encryptor = provider.CreateEncryptor(key, iv)
    let input = Encoding.UTF8.GetBytes data
    let output = encryptor.TransformFinalBlock(input, 0, input.Length)
    Convert.ToBase64String(output)

  /// Decrypts data using AES.
  let decrypt (data : string) : string =
    use provider = Aes.Create()
    use decryptor = provider.CreateDecryptor(key, iv)
    try
      let input = Convert.FromBase64String(data)
      let output = decryptor.TransformFinalBlock(input, 0, input.Length)
      Encoding.UTF8.GetString(output)
    with
      | :? FormatException as ex -> ""
      | :? CryptographicException as ex -> ""
      | :? ArgumentException as ex -> ""

  /// Creates an hmac256 from given data.
  let hmac (data : string) : byte[] = 
    let key = randomKey 64
    let input = Encoding.UTF8.GetBytes(data)
    use hmac = new HMACSHA256(key)
    hmac.ComputeHash(input) 