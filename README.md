# DSCMS

Dead Simple CMS - Lightweight Content Management Using Dotnet and SQLite

## Production Deployment

1. Put together the new container with `docker build -t bradyep/dscms .`
2. Push the new container to docker hub with: `docker push bradyep/dscms`
3. Log on to the remove server: `ssh bradyep@66.228.49.247`
4. Stop the currently running nffyi container: `sudo docker stop [id]`
5. Remove the old docker container: `sudo docker rm [id]`
6. Remove the old docker image to save space: `sudo docker rmi [id]`
7. Get the newly updated image: `sudo docker pull bradyep/dscms`
8. Start up the the new container: `sudo docker run -d -p 127.0.0.1:5000:5000 -it --mount source=dscms-data,target=/dscms-data bradyep/dscms`
