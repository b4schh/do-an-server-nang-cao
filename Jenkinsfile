pipeline {
    agent any

    environment {
        REGISTRY_URL = 'localhost:5000'
        IMAGE_NAME   = 'football-api'
        // Địa chỉ máy production - THAY ĐỔI NÀY
        PRODUCTION_HOST = '192.168.1.200'  // IP máy production
        PRODUCTION_USER = 'deploy'          // User có quyền docker
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

        stage('Run Unit Tests') {
            steps {
                script {
                    echo "========================================"
                    echo "🧪 Running Unit Tests"
                    echo "========================================"
                    
                    // Chạy tests trong Docker container có .NET SDK
                    sh '''
                        docker run --rm \
                            -v $(pwd):/src \
                            -w /src \
                            mcr.microsoft.com/dotnet/sdk:8.0 \
                            sh -c 'dotnet restore && dotnet test src/DoAn.Tests/DoAn.Tests.csproj --no-restore --verbosity normal'
                    '''
                    
                    echo "✅ All tests passed!"
                }
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

        stage('Push Image to Registry') {
            steps {
                sh '''
                    docker push ${REGISTRY_URL}/${IMAGE_NAME}:${IMAGE_TAG}
                    docker push ${REGISTRY_URL}/${IMAGE_NAME}:latest
                '''
                echo "✅ Image pushed to registry successfully!"
            }
        }

        stage('Deploy to Production') {
            steps {
                script {
                    echo "========================================"
                    echo "🚀 Deploying to Production Server"
                    echo "Host: ${PRODUCTION_HOST}"
                    echo "Image: ${IMAGE_NAME}:${IMAGE_TAG}"
                    echo "========================================"
                    
                    // Option 1: Deploy qua SSH (yêu cầu setup SSH key)
                    // Uncomment nếu đã setup SSH
                    /*
                    sshagent(['production-ssh-key']) {
                        sh '''
                            ssh ${PRODUCTION_USER}@${PRODUCTION_HOST} "
                                cd /path/to/app &&
                                ./deploy-production.sh
                            "
                        '''
                    }
                    */
                    
                    // Option 2: Manual deploy instruction
                    echo """
                    ========================================
                    ⚠️  MANUAL DEPLOYMENT REQUIRED
                    ========================================
                    
                    Chạy lệnh sau trên máy PRODUCTION (${PRODUCTION_HOST}):
                    
                    cd /path/to/football-field-booking-api
                    ./deploy-production.sh
                    
                    Hoặc trên Windows:
                    deploy-production.bat
                    
                    ========================================
                    """
                }
            }
        }
    }

    post {
        success {
            echo """
            ========================================
            ✅ CI Pipeline completed successfully!
            ========================================
            
            Build Info:
            - Image: ${REGISTRY_URL}/${IMAGE_NAME}:${IMAGE_TAG}
            - Registry: ${REGISTRY_URL}
            
            Next Steps:
            1. Image đã được push lên registry
            2. Chạy deploy script trên máy production
            3. Verify deployment tại ${PRODUCTION_HOST}
            
            ========================================
            """
        }
        failure {
            echo 'CI Pipeline failed! Please check the logs.'
        }
    }
}
