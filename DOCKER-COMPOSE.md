# Run the Course Learning Platform with Docker Compose

Compose starts the Angular frontend, .NET API, and a project-specific Redis container. It connects to the SQL Server database already installed on the Windows host, so the existing database and course data stay in place.

## One-time setup

1. Copy `.env.example` to `.env` in the project root.
2. In `.env`, set `DB_CONNECTION_STRING` to your SQL Server database, SQL login, and password. Keep `host.docker.internal,1433` if SQL Server accepts TCP connections on port 1433. SQL Server must allow connections from Docker.
3. Replace `JWT_KEY` with a random secret at least 32 characters long. SMTP values are optional.

The `.env` file is ignored by Git and should not be committed.

## Start the project

From the project root, run:

```powershell
docker compose up --build
```

Open `http://localhost:4200`. The API is available at `http://localhost:5198`.

To stop the services, press **Ctrl+C**. Start them again later with:

```powershell
docker compose up
```

The Compose Redis container is named by the project and is not published on host port 6379, so it does not conflict with `politicians_tracker_redis`. Its data is kept in a Docker volume. To remove the Compose services but keep Redis data, run `docker compose down`. To remove its cached data too, run `docker compose down --volumes`.

If you normally run Angular or the API directly on Windows, stop those processes before Compose starts because Compose uses ports 4200 and 5198.
