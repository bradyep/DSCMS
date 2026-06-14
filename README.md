# DSCMS

The Dead Simple Content Management System: a fully functional, fast and simple CMS that runs on ASP.NET Core and SQLite inside an easily deployable Docker container.

## How it Works

### Configuration

`appsettings.json` is used for local development and `appsettings.Production.json` is used for production deployments. The production configuration file is copied over during the docker build process.

### Routing

User requests are handled by `DSCMSController.Content` which is the default route (defined in `startup.cs`) and serves up either specific content or items of a certain content type. The format is as follows: `/{contentType}/{content}`. So for example, `/blog` would display a list of blog posts, while `/blog/my-first-post` would display a specific blog post.

### Models

Each model has its own CRUD controller and views which are mapped in `Program.cs` to fall under the `/Admin` route.

The database schema is defined in `\Data\ApplicationDbContext.cs`. 

The models are as follows:

#### Layout

Razor page that contains everything needed to represent the layout of an entire HTML page. Includes metadata, file location, and associated templates.

#### Template

Razor template that contains all the HTML needed to represent specific content.

#### ContentType

Defines a certain type of content, such as "Blog Post" or "News Article".

#### Content

Represents a content entity (such as a blog post) with metadata, relationships, and associated content items.

#### ContentTypeItem

Represents a type of item associated with a specific content type (such as teaser text for a blog post), including its metadata and related content.

#### ContentItem

Represents an actual child item of content (eg. blog post teaser text "my first post"). Associated with a specific content type and content item type.

## How To

### Administration

Browse to the `/admin` route of your DSCMS instance. 

### Production Deployment

**Note**: Swap in the correct version number in the commands below

1. Put together the new container: `docker build -t bradyep/dscms:v1.0.0 .`
2. Push the new container to docker hub: `docker push bradyep/dscms:v1.0.0`
3. Log on to the remote server: `ssh bradyep@66.228.49.247`
4. Stop the currently running nffyi container: `sudo docker stop [id]`
5. Remove the old docker container: `sudo docker rm [id]`
6. Remove the old docker image to save space: `sudo docker rmi [id]`
7. Get the newly updated image: `sudo docker pull bradyep/dscms:v1.0.0`
8. Start up the the new container: `sudo docker run -d -p 127.0.0.1:5000:5000 -it --mount source=dscms-data,target=/dscms-data bradyep/dscms:v1.0.0`

## Server

* The data directory on the docker host is: `/var/lib/docker/volumes/dscms-data/_data`
* The data directory in the docker image is `/dscms-data`

## Testing

The solution contains a single test project, `DSCMS.Tests`, which houses both unit tests and end-to-end (E2E) browser tests.

### Unit Tests

Controller-level unit tests live under `DSCMS.Tests/Controllers/` and use [xUnit](https://xunit.net/) with [Moq](https://github.com/moq/moq4). They test routing logic and controller behaviour in isolation and can be run at any time without a running app.

```powershell
dotnet test DSCMS.Tests --filter "FullyQualifiedName~Controllers"
```

### E2E Tests (Playwright)

End-to-end tests live under `DSCMS.Tests/E2E/` and use [Microsoft Playwright](https://playwright.dev/dotnet/) to drive a real Chromium browser against the running application. They cover:

- Blog page loads by default, posts are visible, and pagination works
- Games, Projects, and About sections load with the correct content

The fixture automatically starts the app before the first test runs and shuts it down afterwards, so no manual setup is needed from VS Test Explorer or the CLI.

**One-time browser install** (per machine, or after a Playwright version bump):

```powershell
pwsh DSCMS.Tests\bin\Debug\net10.0\playwright.ps1 install chromium
```

**Run E2E tests:**

```powershell
# Using the convenience script (starts and stops the app automatically)
pwsh .\Run-E2ETests.ps1

# Or directly via dotnet test (fixture handles the app lifecycle)
dotnet test DSCMS.Tests --filter "FullyQualifiedName~E2E"
```

**Watch the tests run in a real browser window:**

```powershell
$env:HEADED = "1"; dotnet test DSCMS.Tests --filter "FullyQualifiedName~E2E"
```

## Project Status

This project is currently in active development.

Certain parts of the system are still hard-coded, overly tied to my personal webiste and need to be made dynamic.

Although the system is currently focused on blog posts, the goal is to make it a fully functional CMS that can handle different types of websites.

## License

This project is licensed under the Apache License - see the LICENSE file for details.
