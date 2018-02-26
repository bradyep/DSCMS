# Grab latest version of dotnet core
FROM microsoft/dotnet:2.0.0-sdk
 
# Add everything in this directory and below to the container
ADD . /project

# Install vim and sqlite3 for convenience
RUN ["apt-get", "update"]
RUN ["apt-get", "install", "-y", "vim"]
RUN ["apt-get", "install", "-y", "sqlite3", "libsqlite3-dev"]

# Restore and run our dotnet core project
WORKDIR /project
RUN ["dotnet", "restore"]
ENTRYPOINT ["dotnet", "run"]

# Create mount for externally held (non-container) database data file
# VOLUME /home/myappdata/dscms
# VOLUME /bin/Debug/netcoreapp1.0/data
# RUN mkdir /dscms-data
# RUN echo "hello world" > /dscms-data/greeting.txt
# VOLUME ["/dscms-data"]
