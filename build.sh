#!/usr/bin/env bash

set -e
set -o pipefail

if [ $# -eq 0 ]
  then
    echo "Usage: build.sh [version]"
    exit 1
fi

echo "Build Image"
docker build -t $SERVICE_NAME:$1 --build-arg SERVICE_VERSION=$1 .  --network host

#echo "Scanning Image"
#trivy image $SERVICE_NAME:$1

echo "Huawei Image"
docker tag $SERVICE_NAME:$1 swr.ap-southeast-4.myhuaweicloud.com/$HUAWEI_PROJECT/$SERVICE_NAME:$1
docker push swr.ap-southeast-4.myhuaweicloud.com/$HUAWEI_PROJECT/$SERVICE_NAME:$1
docker rmi swr.ap-southeast-4.myhuaweicloud.com/$HUAWEI_PROJECT/$SERVICE_NAME:$1

echo "Delete Image"
docker rmi $SERVICE_NAME:$1
