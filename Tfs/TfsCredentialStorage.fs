[<AutoOpen>]
module SourceLink.TfsCredentialStorage

open System
open System.Reflection
open Microsoft.VisualStudio.Services.Common
open Microsoft.TeamFoundation.Client

type TfsClientCredentialStorage with
    static member TokenAsString (token:IssuedToken) =
        let m = typeof<TfsClientCredentialStorage>.GetMethod("GetTokenAsString", BindingFlags.NonPublic ||| BindingFlags.Static ||| BindingFlags.DeclaredOnly)
        m.Invoke(null, [| token |]) :?> string

    static member TokenFromString (credentialType:VssCredentialsType) (tokenValue:string) =
        let m = typeof<TfsClientCredentialStorage>.GetMethod("GetTokenFromString", BindingFlags.NonPublic ||| BindingFlags.Static ||| BindingFlags.DeclaredOnly)
        m.Invoke(null, [| credentialType; tokenValue |]) :?> IssuedToken

type IssuedTokenCredential with
    member x.Token
        with get() =
            let p = typeof<IssuedTokenCredential>.GetProperty("InitialToken", BindingFlags.NonPublic ||| BindingFlags.Instance ||| BindingFlags.DeclaredOnly)
            p.GetValue x :?> IssuedToken

type FederatedCredential with
    member x.TokenValue with get() = TfsClientCredentialStorage.TokenAsString x.Token

type CookieCredential with
    static member FromTokenValue tokenValue = 
        let token = TfsClientCredentialStorage.TokenFromString VssCredentialsType.Federated tokenValue
        CookieCredential(false, token :?> CookieToken)

type SimpleWebTokenCredential with
    static member FromTokenValue tokenValue =
        let token = TfsClientCredentialStorage.TokenFromString VssCredentialsType.ServiceIdentity tokenValue
        SimpleWebTokenCredential(null, null, token :?> SimpleWebToken)