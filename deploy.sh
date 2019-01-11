dotnet publish -c Release 

cp dockerfile ./bin/release/netcoreapp2.2/publish

docker build -t sdg-dotnet-image-test-1-image ./bin/release/netcoreapp2.2/publish

docker tag sdg-dotnet-image-test-1-image registry.heroku.com/sdg-dotnet-image-test-1/web

docker push registry.heroku.com/sdg-dotnet-image-test-1/web

heroku container:release web -a sdg-dotnet-image-test-1

# sudo chmod 755 deploy.sh
# ./deploy.sh