# Drive Management
This example illustrates how to define multiple drives and use space management on individual drives.

The test project shows some common use cases:
- Add and use an additional drive in the `MockFileSystem`
- Change the default drive (or directory) used for relative paths
- Define and limit the maximum total size of a drive.
  When this limit is exhausted, an `IOException` is thrown for all file operations that write file content.
