#!/bin/zsh

# 检查并停止正在运行的容器
if [ "$(docker ps -q -f name=mysql-container)" ]; then
    echo "Stopping running mysql-container..."
    docker stop mysql-container
fi

# 检查并移除容器
if [ "$(docker ps -aq -f name=mysql-container)" ]; then
    echo "Removing mysql-container..."
    docker rm mysql-container
fi

# 构建新的镜像
echo "Building new Docker image..."
docker build -t my-mysql-image .

# 运行新的容器
echo "Running new container..."
docker run --cpus="4" -d --name mysql-container -p 3306:3306 my-mysql-image

# jdbc connection string:
# jdbc:mysql://127.0.0.1:3306/sample_db?user=sample_user&password=sample_password

# mysql connection string:
# "Server=localhost;Port=3306;Database=sample_db;Uid=sample_user;Pwd=sample_password;"

# terminal connection string:
# mysql -h 127.0.0.1 -P 3306 -u sample_user -psample_password
