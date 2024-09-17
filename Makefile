build-S3CopyFile:
	dotnet publish -r linux-x64 -c Release -o bin/publish

	cp ./bin/publish/* $(ARTIFACTS_DIR) -r