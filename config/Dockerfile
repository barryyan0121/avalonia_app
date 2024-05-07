# 使用 MySQL 官方镜像作为基础镜像
FROM mysql:latest

# 设置 MySQL 环境变量
ENV MYSQL_ROOT_PASSWORD=root_password \
    MYSQL_DATABASE=sample_db \
    MYSQL_USER=sample_user \
    MYSQL_PASSWORD=sample_password \
    MYSQL_CHARSET=utf8mb4

# 设置环境变量
ENV LANG=C.UTF-8

# 复制 init.sql 文件到容器内的 /docker-entrypoint-initdb.d/ 目录下
COPY init.sql /docker-entrypoint-initdb.d/

EXPOSE 3306