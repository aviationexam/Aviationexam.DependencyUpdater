.PHONY: commit-lock restore-dotnet

commit-lock: restore-dotnet
	git commit ../*.lock.json -m "Update packages.lock.json"

restore-dotnet:
	dotnet restore --force-evaluate

format:
	dotnet format --no-restore -v diag
