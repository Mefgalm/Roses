module Handle.JWTService

open Microsoft.IdentityModel.Tokens
open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System
open System.Linq
open System.Text
open Kernel.Domain.DomainTypes


type Issuer = private Issuer of string
    with 
        static member Create issuer = Issuer issuer //TODO check for null or empty
        static member Get (Issuer issuer) = issuer


type Audience = private Audience of string
    with
        static member Create audience = Audience audience
        static member Get (Audience audience) = audience


type Secret = private Secret of string
    with
        static member Create secret = Secret secret
        static member Get (Secret secret) = secret


type DaysToExpire = private DaysToExpire of float
    with
        static member Create daysToExpire =
            if daysToExpire > 0.
            then DaysToExpire daysToExpire
            else failwith "Days to expire should be Positive"
        static member Get (DaysToExpire daysToExpire) = daysToExpire
        

type TokenPayload = 
    { Id : Guid
      Email: Email
      Roles: seq<string> }

let private generateSymmetricSecurityKey (secret: string) = 
    SymmetricSecurityKey <| Encoding.ASCII.GetBytes secret


let generate secret issuer audience daysToExpire now tokenPayload = 
    let claims = seq {
        yield new Claim(ClaimTypes.Sid, tokenPayload.Id.ToString())
        yield new Claim(ClaimTypes.Email, tokenPayload.Email |> Email.Get)
        yield! tokenPayload.Roles |> Seq.map(fun role -> new Claim(ClaimTypes.Role, role))
    }

    let jwt = JwtSecurityToken(
                issuer = (issuer |> Issuer.Get),
                audience = (audience |> Audience.Get),
                claims = claims,
                notBefore = Nullable(now),
                expires = Nullable(now.AddDays(daysToExpire |> DaysToExpire.Get)),
                signingCredentials = SigningCredentials(generateSymmetricSecurityKey (secret |> Secret.Get), SecurityAlgorithms.HmacSha256))

    JwtSecurityTokenHandler()
        .WriteToken(jwt)


let validateToken secret issuer audience (token: string) =
    try 
        let _, securityToken = 
                JwtSecurityTokenHandler()
                    .ValidateToken(token, 
                               TokenValidationParameters(ValidAudience = (audience |> Audience.Get),
                                                         ValidIssuer = (issuer |> Issuer.Get),
                                                         IssuerSigningKey = generateSymmetricSecurityKey (secret |> Secret.Get)))
        
        securityToken <> null
    with _ -> false


let getPayload secret issuer audience (token: string) =
    let throwIfError failMessage = function
        | Ok x -> x
        | Error errors -> failwithf "%s >> :%A" failMessage errors

    if validateToken secret issuer audience token then
        let jwtSecurityToken = JwtSecurityTokenHandler().ReadToken(token) :?> JwtSecurityToken

        if jwtSecurityToken = null then
            None
        else
            let claims = jwtSecurityToken.Claims |> Seq.cast<Claim>
            let id = claims |> Seq.tryFind(fun x -> x.Type = ClaimTypes.Sid)
            let email = claims |> Seq.tryFind(fun x -> x.Type = ClaimTypes.Email)
            
            match id, email with
            | Some cId, Some cEmail -> Some { Id = Guid.Parse cId.Value
                                              Email = cEmail.Value |> Email.Create |> throwIfError "GetPayload! Email.Create! Impossible case!"
                                              Roles = claims |> Seq.filter(fun x -> x.Type = ClaimTypes.Role) |> Seq.map(fun x -> x.Value) }
            | _ -> None
    else 
        None