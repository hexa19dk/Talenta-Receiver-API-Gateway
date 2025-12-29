#!/usr/bin/env bash

set -e
set -o pipefail

if [ $# -eq 0 ]
  then
    echo "Usage: deploy.sh [version]"
    exit 1
fi

IMAGE_HUAWEI="swr.ap-southeast-4.myhuaweicloud.com/$HUAWEI_PROJECT/$SERVICE_NAME:$1"

echo $IMAGE_HUAWEI

IMAGE_HUAWEI_PATH="$ARGOCD_PROJECT/hcloud/$SERVICE_NAME/overlays/$BRANCH_NAME"

git clone git@gitssh.bluebird.id:argocd/kubernetes-cluster.git && cd kubernetes-cluster
sed -i "s#image: .*#image: $IMAGE_HUAWEI#" $IMAGE_HUAWEI_PATH/deployment-patch.yaml
git add . && git commit -m "[$SERVICE_NAME] - Update Version To $1" && git push && cd ..

echo "Huawei Kubernetes"
sed -i "s#env: .*#env: $BRANCH_NAME#" k8s/huawei-application.yaml
sed -i "s/project: projectid/project: $ARGOCD_PROJECT/" k8s/huawei-application.yaml
sed -i "s/namespace: namespace/namespace: $NAMESPACE_HUAWEI/" k8s/huawei-application.yaml
sed -i "s#path: .*#path: $IMAGE_HUAWEI_PATH#" k8s/huawei-application.yaml
sed -i "s/name: clustername/name: $CLUSTER_NAME_HUAWEI/" k8s/huawei-application.yaml
sed -i "s/name: apps_name/name: $BRANCH_NAME-$SERVICE_NAME/" k8s/huawei-application.yaml
kubectl apply -f k8s/huawei-application.yaml -n argocd --kubeconfig=huawei-kubeconfig.conf
