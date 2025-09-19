# DSCMS

The Dead Simple Content Management System: a fully functional, fast and simple CMS that runs on ASP.NET Core and SQLite inside an easily deployable Docker container.

## How it Works

### Routing

User requests are handled by `DSCMSController.Content` which is the default route (defined in `startup.cs`) and serves up either specific content or items of a certain content type. The format is as follows: `/{contentType}/{content}`. So for example, `/blog` would display a list of blog posts, while `/blog/my-first-post` would display a specific blog post.

### Models

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

1. Put together the new container with `docker build -t bradyep/dscms .`
2. Push the new container to docker hub with: `docker push bradyep/dscms`
3. Log on to the remove server: `ssh bradyep@66.228.49.247`
4. Stop the currently running nffyi container: `sudo docker stop [id]`
5. Remove the old docker container: `sudo docker rm [id]`
6. Remove the old docker image to save space: `sudo docker rmi [id]`
7. Get the newly updated image: `sudo docker pull bradyep/dscms`
8. Start up the the new container: `sudo docker run -d -p 127.0.0.1:5000:5000 -it --mount source=dscms-data,target=/dscms-data bradyep/dscms`

## Project Status

This project is currently in active development. Currently updating to modern ASP.NET Core standards and practices. 

Certain parts of the system are still hard-coded, overly tied to my personal webiste and need to be made dynamic. 

Although the system is currently focused on blog posts, the goal is to make it a fully functional CMS that can handle multiple content types and taxonomies.

## License

This project is licensed under the Apache License - see the LICENSE file for details.
