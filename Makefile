.PHONY: lint build

lint:
	dotnet tool run dotnet-format --check --workspace \(Un\)holy.sln -v quiet

build:
	msbuild -verbosity:quiet
