build-S3CopyFile:
	dotnet publish s3_copy_file_dotnet -f net8.0 -c Release -o bin/publish

	cp ./bin/publish/* $(ARTIFACTS_DIR) -r