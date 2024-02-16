
# README #
This is the the Security Token Provider service based on Identity Server framework and Asp.netcore Identity which provides you with the Endpoints for
- Token Generation
- User Management in Identity.
- Role Management

### What is this repository for? ###

* Quick summary
* Version v1

### How do I get set up? ###

* Summary of set up

* Configuration
  You will need to setup an env file with the following configurations :
```
#IdentityServer
HOST=
SECRET_KEY=
CLIENT_ID=
CLIENT_NAME=

#Web App - Auth service
SERVER_URL=

#twillo
ACCOUNT_SID=
AUTH_TOKEN=
FROM_NUMBER=

#SendGrid
FROM_EMAIL=
EMAIL_NAME=
EMAIL_API_KEY=

#Db Connection
MYSQL_CONNECTION_STRING=
```

* Dependencies
  Identity Server
Identity
EF Core

* Database configuration

#This currently works on the Azure SQL, but can be configured for other RDBMS or NRDBMS
#the connection string is provided in the appsettings.json
Updated to MySQL -

*
Deployment instructions
If running on docker will need : MySql : $ docker pull mysql:latest
Please update the required settings in the docker file such as docker container and new relic licence key.







