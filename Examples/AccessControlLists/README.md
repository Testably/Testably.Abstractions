# Working with access control lists (ACL)
Example how to work with access control lists in the `MockFileSystem`.

In order to be able to support simulating access restrictions, the `MockFileSystem` provides a `IAccessControlStrategy`
that can be set via `MockFileSystem.WithAccessControlStrategy(...)` which simulates access restrictions.

The included default implementation uses a callback to determine whether or not to grant access to a given file or directory.

## Example
The example shows how to implement a custom strategy and provides a test where access to one path is granted 
and access to a second path is denied. In the second case an `IOException` is thrown when trying to 
read the file.
