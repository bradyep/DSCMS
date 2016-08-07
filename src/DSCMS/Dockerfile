FROM microsoft/dotnet
 
ADD . /project
 
WORKDIR /project
 
RUN ["dotnet", "restore"]
 
ENTRYPOINT ["dotnet", "run"]