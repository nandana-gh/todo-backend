pipeline {
    agent any

    environment {
        // Defines the Docker image name
        IMAGE_NAME = 'todo-backend'
    }

    stages {
        stage('Checkout') {
            steps {
                // Checkout the code from the Git repository
                checkout scm
            }
        }

        stage('Restore Dependencies') {
            steps {
                echo 'Restoring .NET dependencies...'
                // Use 'bat' if running directly on a Windows Jenkins agent without WSL/Bash
                // bat 'dotnet restore ToDoApi/ToDoApi.csproj'
                sh 'dotnet restore ToDoApi/ToDoApi.csproj'
            }
        }

        stage('Build API') {
            steps {
                echo 'Building .NET application for production...'
                // bat 'dotnet build ToDoApi/ToDoApi.csproj -c Release --no-restore'
                sh 'dotnet build ToDoApi/ToDoApi.csproj -c Release --no-restore'
            }
        }

        stage('Build Docker Image') {
            steps {
                echo 'Building Docker image...'
                script {
                    // Build the Docker image using the Dockerfile in the current directory
                    def customImage = docker.build("${IMAGE_NAME}:${env.BUILD_ID}")
                }
            }
        }

        // Optional: Add a stage to push to a Docker Registry (like Docker Hub or AWS ECR)
        /*
        stage('Push Docker Image') {
            steps {
                script {
                    docker.withRegistry('https://registry.hub.docker.com', 'docker-hub-credentials-id') {
                        def customImage = docker.image("${IMAGE_NAME}:${env.BUILD_ID}")
                        customImage.push()
                        customImage.push('latest')
                    }
                }
            }
        }
        */
    }

    post {
        always {
            echo 'Pipeline finished!'
        }
        success {
            echo 'Build succeeded!'
        }
        failure {
            echo 'Build failed. Please check the logs.'
        }
    }
}
