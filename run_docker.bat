docker rm -f OpenSky.Website
docker run -d --name OpenSky.Website -p 5001:80 openskywebsite