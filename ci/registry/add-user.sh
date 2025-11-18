#!/bin/bash

# Script to add users to Docker Registry with htpasswd authentication
# Usage: ./add-user.sh <username> <password>

if [ -z "$1" ] || [ -z "$2" ]; then
    echo "Usage: ./add-user.sh <username> <password>"
    echo "Example: ./add-user.sh admin admin123"
    exit 1
fi

USERNAME=$1
PASSWORD=$2
HTPASSWD_FILE="./auth/htpasswd"

echo "Adding user: $USERNAME"

# Create auth directory if it doesn't exist
mkdir -p ./auth

# Use docker to run htpasswd
docker run --rm --entrypoint htpasswd httpd:2 -Bbn "$USERNAME" "$PASSWORD" >> "$HTPASSWD_FILE"

echo "✅ User '$USERNAME' added successfully!"
echo "📁 Password file: $HTPASSWD_FILE"
echo ""
echo "To apply changes, restart the registry:"
echo "  docker compose restart registry"
echo ""
echo "To login:"
echo "  docker login localhost:5000 -u $USERNAME"
