namespace PhotoDB;

using System.Collections.Generic;

public class DBData {
    private int _lastFileID;
    private Dictionary<int, DBFile> _files = new();
    private Dictionary<string, int> _fullpaths = new();
    private Dictionary<string, HashSet<int>> _locations = new();
    private Dictionary<string, HashSet<int>> _filenames = new();
    private Dictionary<string, HashSet<int>> _extensions = new();
    private Dictionary<string, HashSet<int>> _timestamps = new();
    private Dictionary<long, HashSet<int>> _sizes = new();
    private Dictionary<long, HashSet<int>> _checksums = new();
    private Dictionary<string, HashSet<int>> _keywords = new();
    private Dictionary<string, HashSet<int>> _metadataTags = new();
    private HashSet<int> _duplicates = new();
    private HashSet<int> _potentialDuplicates = new();

    public DBData() {}

    public int LastFileID {
        get => _lastFileID;
        set => _lastFileID = value;
    }

    public int NextFileID() {
        LastFileID++;
        return LastFileID;
    }

    public List<string> GetKeywords() {
        var keys = new List<string>(_keywords.Keys);
        keys.Sort();
        return keys;
    }

    public List<string> GetDirectories() {
        var dirs = new List<string>(_locations.Keys);
        dirs.Sort();
        return dirs;
    }

    public DBFile? GetFile(int fileID) => _files.TryGetValue(fileID, out var file) ? file : null;

    public void AddFile(DBFile file) => _files[file.GetID()] = file;
    public void RemoveFile(int fileID) => _files.Remove(fileID);
    public void AddFilePath(string fullpath, int fileID) => _fullpaths[fullpath] = fileID;
    public void RemoveFilePath(string fullpath) => _fullpaths.Remove(fullpath);

    public void AddToIndex<TKey>(Dictionary<TKey, HashSet<int>> index, TKey key, int fileID) {
        if (!index.TryGetValue(key, out var set)) index[key] = set = new();
        set.Add(fileID);
    }

    public void RemoveFromIndex<TKey>(Dictionary<TKey, HashSet<int>> index, TKey key, int fileID) {
        if (index.TryGetValue(key, out var set)) {
            set.Remove(fileID);
            if (set.Count == 0) index.Remove(key);
        }
    }

    public void AddFileLocation(string location, int fileID) => AddToIndex(_locations, location, fileID);
    public void RemoveFileLocation(string location, int fileID) => RemoveFromIndex(_locations, location, fileID);

    public void AddFileFilename(string filename, int fileID) => AddToIndex(_filenames, filename, fileID);
    public void RemoveFileFilename(string filename, int fileID) => RemoveFromIndex(_filenames, filename, fileID);

    public void AddFileExtension(string extension, int fileID) => AddToIndex(_extensions, extension, fileID);
    public void RemoveFileExtension(string extension, int fileID) => RemoveFromIndex(_extensions, extension, fileID);

    public void AddFileTimestamp(string timestamp, int fileID) => AddToIndex(_timestamps, timestamp, fileID);
    public void RemoveFileTimestamp(string timestamp, int fileID) => RemoveFromIndex(_timestamps, timestamp, fileID);

    public void AddFileSize(long size, int fileID) => AddToIndex(_sizes, size, fileID);
    public void RemoveFileSize(long size, int fileID) => RemoveFromIndex(_sizes, size, fileID);

    public void AddFileChecksum(long checksum, int fileID) => AddToIndex(_checksums, checksum, fileID);
    public void RemoveFileChecksum(long checksum, int fileID) => RemoveFromIndex(_checksums, checksum, fileID);

    public void AddFileKeyword(string keyword, int fileID) => AddToIndex(_keywords, keyword.ToUpper(), fileID);
    public void RemoveFileKeyword(string keyword, int fileID) => RemoveFromIndex(_keywords, keyword.ToUpper(), fileID);

    public void AddFileMetadataTag(string tag, int fileID) => AddToIndex(_metadataTags, tag, fileID);
    public void RemoveFileMetadataTag(string tag, int fileID) => RemoveFromIndex(_metadataTags, tag, fileID);

    public int GetFileID(string fullpath) => _fullpaths.TryGetValue(fullpath, out var id) ? id : 0;

    public int GetFileID(string location, string filename, string extension) {
        _filenames.TryGetValue(filename, out var fileIDs1);
        _locations.TryGetValue(location, out var fileIDs2);
        _extensions.TryGetValue(extension, out var fileIDs3);

        foreach (var id in fileIDs1 ?? new()) {
            if ((fileIDs2?.Contains(id) ?? false) && (fileIDs3?.Contains(id) ?? false))
                return id;
        }
        return 0;
    }

    public HashSet<int> FindPotentialDuplicatesIDs(long size, long checksum) {
        _sizes.TryGetValue(size, out var bySize);
        _checksums.TryGetValue(checksum, out var byChecksum);

        var found = new HashSet<int>();
        foreach (var id in bySize ?? new()) {
            if ((byChecksum?.Contains(id) ?? false)) found.Add(id);
        }
        return found;
    }

    public HashSet<int>? GetFileIDsInLocation(string location) => _locations.TryGetValue(location, out var s) ? s : null;
    public HashSet<int>? GetFileIDsWithKeyword(string keyword) => _keywords.TryGetValue(keyword.ToUpper(), out var s) ? s : null;

    public void AddDuplicate(int id) => _duplicates.Add(id);
    public void RemoveDuplicate(int id) => _duplicates.Remove(id);
    public void AddPotentialDuplicate(int id) => _potentialDuplicates.Add(id);
    public void RemovePotentialDuplicate(int id) => _potentialDuplicates.Remove(id);

    public Dictionary<string, int> GetDBStatistics() => new() {
        ["FILES"] = _files.Count,
        ["DIRS"] = _locations.Count,
        ["KEYS"] = _keywords.Count,
        ["DUPS"] = _duplicates.Count,
        ["DUP?S"] = _potentialDuplicates.Count
    };
}
