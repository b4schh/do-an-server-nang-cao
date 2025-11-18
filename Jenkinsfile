pipeline {
    agent any

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Build Docker Image') {
            steps {
                sh 'docker build -t football-api -f Dockerfile .'
            }
        }

        stage('Tag & Push to Registry') {
            steps {
                sh 'docker tag football-api localhost:5000/football-api:latest'
                sh 'docker push localhost:5000/football-api:latest'
            }
        }

        stage('Deploy') {
            steps {
                sh '''
                    docker compose -f docker-compose.prod.yml pull
                    docker compose -f docker-compose.prod.yml up -d
                '''
            }
        }
    }
}
