SHELL := /bin/bash

COMPOSE := docker compose

.PHONY: help restore build test docker-build docker-up docker-up-build docker-down docker-logs docker-ps run-example

help:
	@echo "Available targets:"
	@echo "  make restore            Restore solution dependencies"
	@echo "  make build              Build solution"
	@echo "  make test               Run unit tests"
	@echo "  make run-example        Start example stack with Docker (api + database + caddy)"
	@echo "  make docker-build       Build Docker images"
	@echo "  make docker-up          Start Docker stack in background"
	@echo "  make docker-up-build    Build and start Docker stack in background"
	@echo "  make docker-down        Stop and remove Docker stack"
	@echo "  make docker-logs        Follow Docker stack logs"
	@echo "  make docker-ps          Show Docker stack services status"

restore:
	dotnet restore QuickApi.sln

build:
	dotnet build QuickApi.sln

test:
	dotnet test tests/QuickApi.Tests.Unit/QuickApi.Tests.Unit.csproj

docker-build:
	$(COMPOSE) build

docker-up:
	$(COMPOSE) up -d

docker-up-build:
	$(COMPOSE) up --build -d

docker-down:
	$(COMPOSE) down

docker-logs:
	$(COMPOSE) logs -f

docker-ps:
	$(COMPOSE) ps

run-example: docker-up-build
	@echo "Example stack started."
	@echo "API docs: https://api.localhost/scalar/v1"
