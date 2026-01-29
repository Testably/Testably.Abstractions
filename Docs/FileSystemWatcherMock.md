# FileSystemWatcherMock

This document explains the implementation of the `FileSystemWatcherMock` class.

## Events

Before we can go into the different events the operating systems fire, we first have to define the locations the entry
can be moved from and to.

| Location | Description                                            |
|----------|--------------------------------------------------------|
| _        | Entry was created or deleted                           |
| Outside  | The location is outside the watching path              |
| Inside   | The location is immediately inside the watching path   |
| Nested   | The location is nested under the watching path         |
| Deep     | The location is deeply nested inside the watching path |

For `IncludeSubdirectories = false` we have the following event table:

| From -> To        | Linux     | Windows   | Mac       |
|-------------------|-----------|-----------|-----------|
| _ -> Inside       | `Created` | `Created` | `Created` |
| Inside -> _       | `Deleted` | `Deleted` | `Deleted` |
| _ -> Nested       | -         | -         | -         |
| Nested -> _       | -         | -         | -         |
| _ -> Deep         | -         | -         | -         |
| Deep -> _         | -         | -         | -         |
| Outside -> Inside | `Created` | `Created` | `Created` |
| Inside -> Inside  | `Renamed` | `Renamed` | `Renamed` |
| Inside -> Outside | `Deleted` | `Deleted` | `Deleted` |
| Outside -> Nested | -         | -         | -         |
| Nested -> Nested  | -         | -         | -         |
| Nested -> Outside | -         | -         | -         |
| Outside -> Deep   | -         | -         | -         |
| Deep -> Deep      | -         | -         | -         |
| Deep -> Outside   | -         | -         | -         |
| Inside -> Nested  | `Deleted` | `Deleted` | `Renamed` |
| Inside -> Deep    | `Deleted` | `Deleted` | `Renamed` |
| Nested -> Inside  | `Created` | `Created` | `Created` |
| Deep -> Inside    | `Created` | `Created` | `Created` |
| Nested -> Deep    | -         | -         | -         |
| Deep -> Nested    | -         | -         | -         |

For `IncludeSubdirectories = true` we have the following event table:

| From -> To        | Linux     | Windows               | Mac       |
|-------------------|-----------|-----------------------|-----------|
| _ -> Inside       | `Created` | `Created`             | `Created` |
| Inside -> _       | `Deleted` | `Deleted`             | `Deleted` |
| _ -> Nested       | `Created` | `Created`             | `Created` |
| Nested -> _       | `Deleted` | `Deleted`             | `Deleted` |
| _ -> Deep         | `Created` | `Created`             | `Created` |
| Deep -> _         | `Deleted` | `Deleted`             | `Deleted` |
| Outside -> Inside | `Created` | `Created`             | `Created` |
| Inside -> Inside  | `Renamed` | `Renamed`             | `Renamed` |
| Inside -> Outside | `Deleted` | `Deleted`             | `Deleted` |
| Outside -> Nested | `Created` | `Created`             | `Created` |
| Nested -> Nested  | `Renamed` | `Renamed`             | `Renamed` |
| Nested -> Outside | `Deleted` | `Deleted`             | `Deleted` |
| Outside -> Deep   | `Created` | `Created`             | `Created` |
| Deep -> Deep      | `Renamed` | `Renamed`             | `Renamed` |
| Deep -> Outside   | `Deleted` | `Deleted`             | `Deleted` |
| Inside -> Nested  | `Renamed` | `Deleted` + `Created` | `Renamed` |
| Inside -> Deep    | `Renamed` | `Deleted` + `Created` | `Renamed` |
| Nested -> Inside  | `Renamed` | `Deleted` + `Created` | `Renamed` |
| Deep -> Inside    | `Renamed` | `Deleted` + `Created` | `Renamed` |
| Nested -> Deep    | `Renamed` | `Deleted` + `Created` | `Renamed` |
| Deep -> Nested    | `Renamed` | `Deleted` + `Created` | `Renamed` |

