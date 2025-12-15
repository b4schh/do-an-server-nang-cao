pipeline {
    agent any

    environment {
        REGISTRY_URL = 'localhost:5000'
        IMAGE_NAME = 'football-api'
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
                sh 'ls -al'
            }
        }

        stage('Build Docker Image') {
            steps {
                sh 'docker build -t ${IMAGE_NAME} -f Dockerfile .'
            }
        }

        stage('Tag Image') {
            steps {
                sh 'docker tag ${IMAGE_NAME} ${REGISTRY_URL}/${IMAGE_NAME}:latest'
            }
        }

        stage('Login to Registry') {
            steps {
                sh 'echo "admin123" | docker login ${REGISTRY_URL} -u admin --password-stdin'
            }
        }

        stage('Push Image to Registry') {
            steps {
                sh 'docker push ${REGISTRY_URL}/${IMAGE_NAME}:latest'
            }
        }

        stage('Deploy') {
            steps {
                sh '''
                    docker compose -f docker-compose.prod.yml --env-file .env.prod down
                    docker compose -f docker-compose.prod.yml --env-file .env.prod pull
                    docker compose -f docker-compose.prod.yml --env-file .env.prod up -d --force-recreate
                '''
            }
        }
    }

    post {
        success {
            echo 'Deployment to Production completed successfully!'
        }
        failure {
            echo 'Deployment failed! Please check the logs.'
        }
    }
}
