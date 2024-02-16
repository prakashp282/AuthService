# README #

This is the the Auth service which provides you with the Endpoints for

- User Authentication
- User Management

### What is this repository for? ###

* Quick summary

This is a POC for Authentication(OpenId connect) and Authorization(OAuth2.0) Service using different Identity Providers
and can be configured to extend further.

- Because we are doing a Backchannel(from server to server to get token) which is more secure, We have demonstrated the
  ROPC - Resource owner password credential flow as we own the Authorization Server.
- If need to extend to use other IdentityProvider google or twitter would switch to Authorization Code flow. As that
  would be called a Front + back channel communication (Recommended).

- We do support Client credential (complete back channel) flow.

* Version v1

- Basic Authentication with Username Password.
- User Signup.
- MFA, Enrollment and verification.
- User managment - password update, phone update, account update.
- Infosec Requirments.

### How do I get set up? ###

* Summary of set up

* Configuration
  You will need to setup an env file with the following configurations :

```
PORT=5001
CLIENT_ORIGIN_URL=

#Auth0 configurations
AUTH0_AUDIENCE=
AUTH0_DOMAIN=
AUTH0_CLIENT_ID=
AUTH0_CLIENT_SECRET=
AUTH0_DB_CONNECTION=

#new relic licence key
NEW_RELIC_KEY=

#IdentityServer
IDENTITY_SERVER_HOST=
IDENTITY_SERVER_AUDIENCE=
IDENTITY_SERVER_CLIENT_ID=
IDENTITY_SERVER_CLIENT_SECRET=

#Configure TOKEN PROVIDER
USE_IDENTITY_SERVER=
```

* Dependencies


* Database configuration
* Deployment instructions

Please update the required settings in the docker file such as docker container and new relic licence key.
Then it can be deployed independently using docker by running the command

```
sh exec.sh
```




