namespace PhotoStatus;

public enum StatusCode {
    NoError,
    UnexpectedStatus,
    UnknownCommand,
    DbFileDoesNotExist,
    DbPathDoesNotExist,
    DbFileDirDoesNotExist,
    DbFileDirKeywordDoesNotExist,
    DbFileIncompatibleFormat,
    DbFileReadError,
    DbFileNotSerializable,
    DbFileWriteError,
    InvalidNumberOfArguments,
    PathDoesNotExist,
    FileSystemError,
    FileSystemNotFile,
    FileSystemNotImage
}
