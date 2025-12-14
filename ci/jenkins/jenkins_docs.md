| Thành phần          | Ý nghĩa                             |
| ------------------- | ----------------------------------- |
| Build từ Dockerfile | Jenkins custom có Docker CLI        |
| Mount docker.sock   | Cho phép Jenkins build & push image |
| Mount jenkins.yaml  | Jenkins tự config user, quyền admin |
| Expose port 8080    | Vào Jenkins UI                      |
| Expose port 50000   | Cho agent trong tương lai           |
