namespace PhotoUtil;

using PhotoStatus;
using PhotoDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

public class FileSystem {
    private static StatusCode _statusCode;

    public static StatusCode GetStatusCode() => _statusCode;
    public static void SetStatusCode(StatusCode newStatusCode) => _statusCode = newStatusCode;

    public FileSystem() {}

    public static char CheckPath(string path) {
        if (File.Exists(path)) return 'F';
        if (Directory.Exists(path)) return 'D';
        return 'E';
    }

    public static List<string> FilesInDirectory(string directory) {
        var listOfFiles = new List<string>();
        SetStatusCode(StatusCode.NoError);

        try {
            listOfFiles.Add(Path.GetFullPath(directory));
            listOfFiles.AddRange(Directory.GetFiles(directory).Select(Path.GetFullPath));
        } catch (IOException) {
            SetStatusCode(StatusCode.FileSystemError);
        }

        return listOfFiles;
    }

    public static DBFile GetFileInformation(string filename) {
        SetStatusCode(StatusCode.NoError);

        var dbFile = new DBFile();
        try {
            var fileInfo = new FileInfo(filename);
            if (!fileInfo.Exists || (fileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory) {
                SetStatusCode(StatusCode.FileSystemNotFile);
                return dbFile;
            }

            string fullpath = fileInfo.FullName;
            string location = fileInfo.DirectoryName ?? "";
            string fname = Path.GetFileNameWithoutExtension(filename);
            string extension = Path.GetExtension(filename).TrimStart('.');
            string timestamp = fileInfo.LastWriteTime.ToString("yyyyMMdd HHmmss");
            long size = fileInfo.Length;
            long checksum = CalculateChecksum(fileInfo);

            dbFile.SetFullpath(fullpath);
            dbFile.SetLocation(location);
            dbFile.SetFilename(fname);
            dbFile.SetExtension(extension);
            dbFile.SetTimestamp(timestamp);
            dbFile.SetSize(size);
            dbFile.SetChecksum(checksum);
            dbFile.SetKeywords(new HashSet<string>());
            dbFile.SetMetadata(new HashSet<MetadataInfo>());

        } catch (IOException) {
            SetStatusCode(StatusCode.FileSystemError);
        }

        return dbFile;
    }

    public static long CalculateChecksum(FileInfo fileInfo) {
        SetStatusCode(StatusCode.NoError);
        try {
            using var stream = fileInfo.OpenRead();
            // TODO
            //using var crc32 = new Crc32();
            //byte[] hash = crc32.ComputeHash(stream);
            // return BitConverter.ToUInt32(hash, 0);
            return 0;
        } catch (IOException) {
            SetStatusCode(StatusCode.FileSystemError);
            return 0;
        }
    }

    public static string ExtractFilename(string filename) {
        return Path.GetFileName(filename);
    }

    public static string FormatFileSize(long sizeInBytes) {
        string[] units = {"B", "KB", "MB", "GB", "TB"};
        double size = sizeInBytes;
        int unitIndex = 0;
        while (size >= 1024 && unitIndex < units.Length - 1) {
            size /= 1024;
            unitIndex++;
        }
        return string.Format("{0:0.00} {1}", size, units[unitIndex]);
    }

    public static bool CompareFiles(string path1, string path2) {
        SetStatusCode(StatusCode.NoError);

        try {
            var file1 = new FileInfo(path1);
            var file2 = new FileInfo(path2);

            if (!file1.Exists || !file2.Exists || file1.Length != file2.Length) return false;

            using var stream1 = file1.OpenRead();
            using var stream2 = file2.OpenRead();
            var buffer1 = new byte[8192];
            var buffer2 = new byte[8192];
            int read1, read2;

            do {
                read1 = stream1.Read(buffer1, 0, buffer1.Length);
                read2 = stream2.Read(buffer2, 0, buffer2.Length);

                if (read1 != read2 || !buffer1.AsSpan(0, read1).SequenceEqual(buffer2.AsSpan(0, read2)))
                    return false;
            } while (read1 > 0);

            return true;
        } catch (IOException) {
            SetStatusCode(StatusCode.FileSystemError);
            return false;
        }
    }
}
