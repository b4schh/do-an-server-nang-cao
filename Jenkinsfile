pipeline {
    agent any

    environment {
        REGISTRY_URL = 'localhost:5000'
        IMAGE_NAME   = 'football-api'
        // IMAGE_TAG sẽ được set ở stage Init
    }

    stages {

        stage('Init') {
            steps {
                script {
                    // Format: 20251215-093045
                    IMAGE_TAG = sh(
                        script: "date +%Y%m%d-%H%M%S",
                        returnStdout: true
                    ).trim()

                    echo "Build image tag: ${IMAGE_TAG}"
                }
            }
        }

        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Build Docker Image') {
            steps {
                sh '''
                    docker build -t ${IMAGE_NAME}:${IMAGE_TAG} -f Dockerfile .
                '''
            }
        }

        stage('Tag Image') {
            steps {
                sh '''
                    docker tag ${IMAGE_NAME}:${IMAGE_TAG} ${REGISTRY_URL}/${IMAGE_NAME}:${IMAGE_TAG}
                    docker tag ${IMAGE_NAME}:${IMAGE_TAG} ${REGISTRY_URL}/${IMAGE_NAME}:latest
                '''
            }
        }

        stage('Login to Registry') {
            steps {
                sh 'echo "admin123" | docker login ${REGISTRY_URL} -u admin --password-stdin'
            }
        }

        stage('Push Image') {
            steps {
                sh '''
                    docker push ${REGISTRY_URL}/${IMAGE_NAME}:${IMAGE_TAG}
                    docker push ${REGISTRY_URL}/${IMAGE_NAME}:latest
                '''
            }
        }

        stage('Deploy') {
            steps {
                withCredentials([file(credentialsId: 'env-prod-file', variable: 'ENV_PROD')]) {
                    sh '''
                        cp $ENV_PROD .env.prod
                        docker compose -f docker-compose.prod.yml --env-file .env.prod down || true
                        docker compose -f docker-compose.prod.yml --env-file .env.prod pull
                        docker compose -f docker-compose.prod.yml --env-file .env.prod up -d --force-recreate
                    '''
                }
            }
        }
    }

    post {
        success {
            echo "Deployment completed successfully!"
            echo "Image pushed: ${REGISTRY_URL}/${IMAGE_NAME}:${IMAGE_TAG}"
        }
        failure {
            echo 'Deployment failed! Please check the logs.'
        }
    }
}
