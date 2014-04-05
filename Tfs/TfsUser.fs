namespace SourceLink

open System
open SourceLink
open Microsoft.VisualStudio.Services.Common
open Microsoft.TeamFoundation.Client

type TfsUser(credentials:TfsClientCredentials) =
    new() = TfsUser(TfsClientCredentials())
    member x.Credentials with get() = credentials

    static member VisualStudio
        with get() =
            let vssToken = TfsClientCredentialStorage.RetrieveConnectedUserToken()
            let storage = TfsClientCredentialStorage()
            let token = storage.RetrieveToken(Uri vssToken.Resource, VssCredentialsType.Federated)
            let cred = CookieCredential(false, token :?> CookieToken)
            TfsUser(TfsClientCredentials cred)

    static member FromCookieToken (token:string) =
        TfsUser(TfsClientCredentials(CookieCredential.FromTokenValue token))

    static member FromSimpleWebToken (token:string) =
        TfsUser(TfsClientCredentials(SimpleWebTokenCredential.FromTokenValue token))