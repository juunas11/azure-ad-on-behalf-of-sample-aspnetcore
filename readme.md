# Azure AD On-Behalf-Of authentication sample

This sample contains an ASP.NET Core 2.0 API that calls Microsoft Graph API
as the user which called this API.

The API uses Azure AD authentication, so in order to use it, you will need
to register it in your Azure Active Directory.

The app will need the *Sign in and read user profile* delegated permission on
Microsoft Graph, so make sure to set that in Required permissions.

After registering the app, fill in the necessary settings in appsettings.json:

```json
{
  "Authentication": {
    "AadInstance": "https://login.microsoftonline.com/",
    "AppIdUri": "App ID URI from app's Properties",
    "ClientId": "Application ID from the app's blade",
    "Tenant": "Azure AD tenant id or one of the verified domains, e.g. tenantname.onmicrosoft.com"
  }
}
```

And user secrets:

```json
{
  "Authentication": {
    "ClientSecret": "Create Key in the Keys blade, put value here"
  }
}
```

You can access user secrets by right-clicking on the project in Visual Studio
and clicking *Manage User Secrets*.

If you want, you can obviously put the secret in appsettings.json,
but using user secrets means you can't accidentally commit it into version control.