FROM maven:3.9-eclipse-temurin-21 AS build
WORKDIR /app

COPY pom.xml .
RUN mvn dependency:go-offline -B

COPY src ./src
RUN mvn package -DskipTests

FROM tomcat:10-jdk21
WORKDIR /usr/local/tomcat/webapps/

RUN rm -rf ROOT

COPY --from=build /app/target/vinheriaagnello.war ./ROOT.war

EXPOSE 8080
CMD ["catalina.sh", "run"]
