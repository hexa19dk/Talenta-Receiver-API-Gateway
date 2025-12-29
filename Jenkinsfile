pipeline {
    agent {
        node {
            label 'slave-01'
            customWorkspace "workspace/${env.BRANCH_NAME}/src/git.bluebird.id/bbone/bbone-talenta-receiver/"
        }
    }
    environment {
        SERVICE_NAME = 'talenta-receiver'
        ARGOCD_PROJECT = "bbone"
        SWR_USERNAME = credentials('username-swr-huawei-ms')
        SWR_PASSWORD = credentials('password-swr-huawei-ms')
    }
    options {
        buildDiscarder(logRotator(daysToKeepStr: env.BRANCH_NAME == 'master' ? '90' : '30'))
    }
    stages {
        stage('Checkout') {
            steps {
                echo 'Checking out from Git'
                checkout scm
            }
        }
        stage('Prepare') {
            steps {
                sh "docker login -u ${env.SWR_USERNAME} -p ${env.SWR_PASSWORD} swr.ap-southeast-4.myhuaweicloud.com"
            }
        }
         stage('Build and Deploy Multi Cloud') {
            environment {
                VERSION_PREFIX = '2.0'
            }
            stages {
                stage('Deploy to Multi Cloud Kubernetes Develop') {
                    when {
                        branch 'develop'
                    }
                    environment {
                        VERSION = "${env.VERSION_PREFIX}-dev${env.BUILD_NUMBER}"
                        BRANCH_NAME = "develop"

                        HUAWEI_PROJECT = "bbone_dev_bluebirdgroup"
                        CLUSTER_NAME_HUAWEI = "cce_huawei_bbone_dev_bluebirdgroup_bbone_dev-internal-cluster"
                        NAMESPACE_HUAWEI = "bb-one"
                    }
                    steps {
                        withCredentials([
                            file(credentialsId: 'a5c372d1-9105-4d45-b7f1-a6f5f1acfb12', variable: 'gkubeconfig'),
                            file(credentialsId: '1ffe0701-2c4a-4497-8c3f-9f941a382e3a', variable: 'hkubeconfig')
                        ]) {
                            sh "cp $gkubeconfig gcp-kubeconfig.conf"
                            sh "cp $hkubeconfig huawei-kubeconfig.conf"
                            sh "chmod 644 gcp-kubeconfig.conf"
                            sh "chmod 644 huawei-kubeconfig.conf"
                            sh 'chmod +x build.sh'
                            sh './build.sh $VERSION'
                            sh 'chmod +x deploy.sh'
                            sh './deploy.sh $VERSION $NAMESPACE'
                        }
                    }
                }
                stage('Deploy to Multi Cloud Kubernetes Staging') {
                    when {
                        branch 'staging'
                    }
                    environment {
                        VERSION = "${env.VERSION_PREFIX}-stg${env.BUILD_NUMBER}"
                        BRANCH_NAME = "staging"

                        HUAWEI_PROJECT = "bbone_dev_bluebirdgroup"
                        CLUSTER_NAME_HUAWEI = "cce_huawei_bbone_dev_bluebirdgroup_bbone_dev-internal-cluster"
                        NAMESPACE_HUAWEI = "bb-one"
                    }
                    steps {
                        withCredentials([
                            file(credentialsId: 'a5c372d1-9105-4d45-b7f1-a6f5f1acfb12', variable: 'gkubeconfig'),
                            file(credentialsId: '1ffe0701-2c4a-4497-8c3f-9f941a382e3a', variable: 'hkubeconfig')
                        ]) {
                            sh "cp $gkubeconfig gcp-kubeconfig.conf"
                            sh "cp $hkubeconfig huawei-kubeconfig.conf"
                            sh "chmod 644 gcp-kubeconfig.conf"
                            sh "chmod 644 huawei-kubeconfig.conf"
                            sh 'chmod +x build.sh'
                            sh './build.sh $VERSION'
                            sh 'chmod +x deploy.sh'
                            sh './deploy.sh $VERSION $NAMESPACE'
                        }
                    }
                }
                stage('Deploy to Multi Cloud Kubernetes Production') {
                    when {
                        tag "v*"
                    }
                    environment {
                        VERSION = "${env.VERSION_PREFIX}-multicloud-prd-${env.TAG_NAME}"
                        BRANCH_NAME = "production"

                        HUAWEI_PROJECT = "bbone_prd_bluebirdgroup"
                        CLUSTER_NAME_HUAWEI = "cce_huawei_bbone_prd_bluebirdgroup_bbone_prd-internal-cluster"
                        NAMESPACE_HUAWEI = "bb-one"
                    }
                    steps {
                        withCredentials([
                            file(credentialsId: 'a5c372d1-9105-4d45-b7f1-a6f5f1acfb12', variable: 'gkubeconfig'),
                            file(credentialsId: '1ffe0701-2c4a-4497-8c3f-9f941a382e3a', variable: 'hkubeconfig')
                        ]) {
                            sh "cp $gkubeconfig gcp-kubeconfig.conf"
                            sh "cp $hkubeconfig huawei-kubeconfig.conf"
                            sh "chmod 644 gcp-kubeconfig.conf"
                            sh "chmod 644 huawei-kubeconfig.conf"
                            sh 'chmod +x build.sh'
                            sh './build.sh $VERSION'
                            sh 'chmod +x deploy.sh'
                            sh './deploy.sh $VERSION $NAMESPACE'
                        }
                    }
                }
            }
        }
    }
    post {
        cleanup {
            /* clean up our workspace */
            deleteDir()
            /* clean up tmp directory */
            dir("${workspace}@tmp") {
                deleteDir()
            }
            /* clean up script directory */
            dir("${workspace}@script") {
                deleteDir()
            }
        }
    }
}
