#!/bin/zsh
docker build -t my-mysql-image . &&
docker run -d --name mysql-container -p 3306:3306 my-mysql-image

# connection string: 
# jdbc:mysql://localhost:3306/sample_db?user=sample_user&password=sample_password