#!/bin/zsh
docker build -t my-mysql-image . &&
docker run -d --name mysql-container -p 3306:3306 my-mysql-image

# jdbc connection string: 
# jdbc:mysql://localhost:3306/sample_db?user=sample_user&password=sample_password

# mysql connection string:
# "Server=localhost;Port=3306;Database=sample_db;Uid=sample_user;Pwd=sample_password;"

# terminal connection string:
# mysql -h 127.0.0.1 -P 3306 -u sample_user -psample_password
