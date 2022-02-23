# spoty-app

## Demo and try out
### Note: you will need an active Spotify Premium account to use this application
[TBD link to youtube]
[TBD link to hosted app]

## Features
### Shared playlist (URL: "/rooms/<some spotify user id>")
- Host your session and modify settings like password/friend-only access/public or private room (public setting decides whether your room will be exposed to other people)
- Supports multiple windows opened at once
- Chatting with people in the room
- See users connected to your room
- See information on currently played track
- Change track on embedded spotify player/ or from any device which has spotify in it, and this track will be played for all listeners in your room. 
If you prefer to use phone app/tv/computer app player - make sure to set device there to "Spotify Discovery"

### Song tracker (URL: "/home")
- Track songs you've already heard before
- Autoskip songs which system detected as "Tracked"
- Autoplaylist that adds fresh songs is currently bugged and won't work correctly, so you will see that error for now

### Rooms explorer (URL: "/?page=<page number>")
- Explore and join public rooms listed on root URL, specify page to be displayed more results (/?page=1)
- Rooms will display their host and currently active song, so you could select room based of your music preferences
- Rooms that aren't set as "public" by hosts will not be displayed here

### Development
Development flow is quite straightforward:

- install dev HTTPS certificate if needed for dotnet app
- run SpotifyDiscovery launch configuration for dotnet app
- Inside src directory run "docker-compose" to instanciate redis, mongo and serilog containers

You may implement database seeding for development environment, as docker containers won't be persisted once they've been removed
Alternatively you can attach volume to your database container to reference data of your local machine, thus persisting dev data on container removal

### Architecture

#### Architecture diagram
![App architecture diagram](/assets/spotiapp_arch-arch.drawio.png)

Server app:
- .netcore 5.0
- Request rate limiting rules
- CORS disabled
- Distributed redis cache container in private network for SignalR connections, profiles and shared player information, cache for HTTP requests data (Distributed caching is useful for the case of horizontal scaling for server app, a cluster can be set to add more instances/replicas)
- Pagination with expiring cache, cancellation token for fetching shared rooms (URL: "/?page=<page number>") (To reduce round-trips to the database)
- SignalR websocket communcation with client to perform updates on connected client (connected users, chat, active song played by host, host settings changes that may affect connected person)
- Auth through Spotify API, certain controllers have SpotifyAuth filter to allow authorized-only requests
- Reverse proxy (Mainly for prod HTTPS/TLS and port forwarding, but also useful for horizontal scaling of server)
- Load secrets into app from Azure Key Vault through managed identity without exposing secrets through config files on VM
- App event realtime logging: log into container's console, Serilog UI (Serilog UI is exposed on public IP, but on first run it requires admin username and password)
- App event log ingestion to database (Persist event logs and use later to trace errors/ bugs of application, ingestion can't happen from outside of container network thus adding security)
- High reliability/availability Azure Cosmosdb for mongodb database (keep logs, account information, rooms information, song tracker information)

Client app:
- React
- SignalR client
- SpotifySDK
- Localstorage is used for access and refresh tokens
- React-Router v6 for page navigation and redirects

#### Production deployment flow diagram
This process is quite simple and manual, it doesn't require passing client/server tests/ code coverage. 
No webhooks with github branch to start a devops pipeline

![App production deployment](/assets/spotiapp_arch-deployment.drawio.png)
