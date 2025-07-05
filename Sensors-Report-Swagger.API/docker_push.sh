DOCKER_LATEST="viobiscu/sensors-report-swagger-api:latest"

cd ..
docker build -f Sensors-Report-Swagger.API/Dockerfile -t "$DOCKER_LATEST" .
echo "Built image: $DOCKER_IMAGE"
echo "You can now push the image and update your deployment YAML with:"
echo "  image: $DOCKER_IMAGE"

read -p "Do you want to push the image $DOCKER_LATEST to Docker Hub? (Y/n): " confirm
confirm=${confirm:-y}
if [[ "$confirm" == "y" || "$confirm" == "Y" ]]; then
  docker push "$DOCKER_LATEST"
  echo "Pushed images: $DOCKER_LATEST"
fi
