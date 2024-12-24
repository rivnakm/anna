FROM rust:latest as build

WORKDIR /usr/src/anna
COPY . .

RUN cargo install --path .

FROM debian:12-slim

COPY --from=build /usr/local/cargo/bin/anna /usr/local/bin/anna

CMD ["anna"]
