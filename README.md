# DAS Token Service

A service to provide a single point of distribution of tokens to other DAS services.

## Consuming

### Consuming from HTTP

All requests require a Authorization header using a bearer token obtained from Azure AD.

### Consuming from .NET

A nuget package exists to easy consuming the service.

```powershell
Install-Package SFA.DAS.TokenService.Api.Client
```

All code examples will assume the use of this package. You can create the client by:

```csharp
var configuration = new TokenServiceApiClientConfiguration
{
    ApiBaseUrl = "https://some.server", // Url to the token service
    ClientId = "Your-Client-Id", // Client id provided to your application
    ClientSecret = "Your-Secret", // Secret provided to your application
    IdentifierUri = "https://some-uri", // IdentifierUri provided for environment
    Tenant = "some-tenant" // Tenent provided for environment
};
var client = new TokenServiceApiClient(configuration);
```


## Privileged Access

The privileged access endpoint provides the access code for HMRC apis. You can request using the http request:
```
GET https://some.server/api/PrivilegedAccess
```

Which will return something like:
```
{
  "AccessCode": "a172ed1ee4adbaa339895655ab875a6",
  "ExpiryTime": "2017-02-22T18:01:33.907979Z"
}
```

The equivilant C# is:
```csharp
var token = await client.GetPrivilegedAccessTokenAsync();
```

The AccessCode can be used to connect to HMRC apis. It can be cached in the application until the ExpiryTime (Which is in UTC).