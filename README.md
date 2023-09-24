## Movies challenge
This project is a test challenge for a job interview. It is a simple web application that allows you to search for movies and see their details.

## How to run
To run this project you need to have docker installed. Then you can run the following command:
```
docker compose up -d
```
Also, the provided API has an error inside, so you need to run the following command to fix it:
```
docker compose exec api mv /app/amovies-db.json /app/movies-db.json
```
## How to use
Take a look at the [cUrls.txt](cUrls.txt) file to see some examples of how to use the API.
To create requests you can use the Swagger UI at http://localhost:7629/swagger

