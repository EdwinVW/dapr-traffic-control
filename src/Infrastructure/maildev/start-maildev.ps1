# using older version of maildev (newer version did not render HTML mail correctly)
docker run -d -p 4000:80 -p 4025:25 --name dtc-maildev maildev/maildev:latest
