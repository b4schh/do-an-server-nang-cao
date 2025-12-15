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
                    env.IMAGE_TAG = sh(
                        script: "date +%Y%m%d-%H%M%S",
                        returnStdout: true
                    ).trim()

                    echo "Build image tag: ${env.IMAGE_TAG}"
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
                        
                        # Stop existing containers
                        docker compose -f docker-compose.prod.yml --env-file .env.prod down || true
                        
                        # Pull images with retry (skip if timeout persists)
                        for i in 1 2 3; do
                            echo "Attempt $i to pull images..."
                            if docker compose -f docker-compose.prod.yml --env-file .env.prod pull 2>/dev/null; then
                                echo "Images pulled successfully"
                                break
                            fi
                            if [ $i -eq 3 ]; then
                                echo "Pull failed after 3 attempts, using existing images..."
                            else
                                echo "Pull failed, retrying in 5 seconds..."
                                sleep 5
                            fi
                        done
                        
                        # Start containers (will use local images if pull failed)
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
