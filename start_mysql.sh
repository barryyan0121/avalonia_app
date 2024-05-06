#!/bin/zsh
docker run -d --name mysql-container -p 3306:3306 \
  -e MYSQL_ROOT_PASSWORD=root_password \
  -e MYSQL_DATABASE=sample_db \
  -e MYSQL_USER=sample_user \
  -e MYSQL_PASSWORD=sample_password \
  -v ./init.sql:/docker-entrypoint-initdb.d/init.sql \
  mysql:latest

# connection string: 
# jdbc:mysql://localhost:3306/sample_db?user=sample_user&password=sample_password