docker build -f ".\OpenSky.Website\Dockerfile" --force-rm --no-cache -t openskywebsite --build-arg "BUILD_CONFIGURATION=Release" --label "com.microsoft.created-by=visual-studio" --label "com.microsoft.visual-studio.project-name=OpenSky.Website" .