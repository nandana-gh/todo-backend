pipeline {
    agent any

    environment {
        ACR     = 'todoacr'
        RG      = 'todo-rg'
        AKS     = 'todo-aks'
        LOCATION = 'southindia'
        IMAGE   = 'todo-backend'
        AZ_CLIENT_ID     = credentials('azure-client-id')
        AZ_CLIENT_SECRET = credentials('azure-client-secret')
        AZ_TENANT_ID     = credentials('azure-tenant-id')
    }

    stages {
        stage('Checkout') {
            steps { checkout scm }
        }

        stage('Build image') {
            steps {
                bat 'docker build --platform linux/amd64 -t %ACR%.azurecr.io/%IMAGE%:%BUILD_NUMBER% -t %ACR%.azurecr.io/%IMAGE%:latest .'
            }
        }

        stage('Login to Azure') {
            steps {
                bat 'az login --service-principal -u %AZ_CLIENT_ID% -p %AZ_CLIENT_SECRET% --tenant %AZ_TENANT_ID%'
                bat 'az group create -n %RG% -l %LOCATION%'
                bat 'az acr create -n %ACR% -g %RG% --sku Basic'
                bat 'az acr login -n %ACR%'
            }
        }

        stage('Push to ACR') {
            steps {
                bat 'docker push %ACR%.azurecr.io/%IMAGE%:%BUILD_NUMBER%'
                bat 'docker push %ACR%.azurecr.io/%IMAGE%:latest'
            }
        }

        stage('Deploy to AKS') {
            steps {
                bat 'az aks get-credentials -n %AKS% -g %RG% --overwrite-existing'
                powershell '(Get-Content k8s/api.yaml) -replace "<ACR_NAME>", $env:ACR | Set-Content $env:TEMP\\api.yaml'
                bat 'kubectl apply -f k8s/mysql.yaml'
                bat 'kubectl apply -f %TEMP%\\api.yaml'
                bat 'kubectl set image deployment/todo-api todo-api=%ACR%.azurecr.io/%IMAGE%:%BUILD_NUMBER%'
                bat 'kubectl rollout status deployment/todo-api --timeout=120s'
            }
        }
    }

    post {
        success { echo "todo-backend ${BUILD_NUMBER} deployed to AKS." }
        failure { echo 'todo-backend pipeline failed.' }
        always  { bat 'az logout || exit 0' }
    }
}