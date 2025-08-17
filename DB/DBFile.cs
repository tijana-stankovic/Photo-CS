namespace PhotoDB;

using PhotoUtil;
using System;
using System.Collections.Generic;

public class DBFile {
    private int _id;
    private string _fullpath;
    private string _location;
    private string _filename;
    private string _extension;
    private string _timestamp;
    private long _size;
    private long _checksum;
    private HashSet<string> _keywords;
    private HashSet<MetadataInfo> _metadata;
    private HashSet<int> _duplicates;
    private HashSet<int> _potentialDuplicates;

    public DBFile() {
        _id = 0;
        _fullpath = "";
        _location = "";
        _filename = "";
        _extension = "";
        _timestamp = "";
        _size = 0L;
        _checksum = 0L;
        _keywords = new HashSet<string>();
        _metadata = new HashSet<MetadataInfo>();
        _duplicates = new HashSet<int>();
        _potentialDuplicates = new HashSet<int>();
    }

    public int GetID() => _id;
    public void SetID(int id) {
        if (id <= 0) throw new ArgumentException("File ID must be positive!");
        _id = id;
    }

    public string GetFullpath() => _fullpath;
    public void SetFullpath(string fullpath) {
        if (string.IsNullOrEmpty(fullpath)) throw new ArgumentException("File path must be specified!");
        _fullpath = fullpath;
    }

    public string GetLocation() => _location;
    public void SetLocation(string location) {
        if (string.IsNullOrEmpty(location)) throw new ArgumentException("File location must be specified!");
        _location = location;
    }

    public string GetFilename() => _filename;
    public void SetFilename(string filename) {
        if (string.IsNullOrEmpty(filename)) throw new ArgumentException("Filename must be specified!");
        _filename = filename;
    }

    public string GetExtension() => _extension;
    public void SetExtension(string extension) {
        if (string.IsNullOrEmpty(extension)) throw new ArgumentException("Extension must be specified!");
        _extension = extension;
    }

    public string GetTimestamp() => _timestamp;
    public void SetTimestamp(string timestamp) {
        if (string.IsNullOrEmpty(timestamp)) throw new ArgumentException("Timestamp must be specified!");
        _timestamp = timestamp;
    }

    public long GetSize() => _size;
    public void SetSize(long size) {
        if (size < 0) throw new ArgumentException("Size must not be negative!");
        _size = size;
    }

    public long GetChecksum() => _checksum;
    public void SetChecksum(long checksum) {
        _checksum = checksum;
    }

    public HashSet<string> GetKeywords() => _keywords;
    public void SetKeywords(HashSet<string> keywords) {
        _keywords = keywords ?? new HashSet<string>();
    }

    public HashSet<MetadataInfo> GetMetadata() => _metadata;
    public void SetMetadata(HashSet<MetadataInfo> metadata) {
        _metadata = metadata ?? new HashSet<MetadataInfo>();
    }

    public HashSet<int> GetDuplicates() => _duplicates;
    public void SetDuplicates(HashSet<int> duplicates) {
        _duplicates = duplicates ?? new HashSet<int>();
    }

    public HashSet<int> GetPotentialDuplicates() => _potentialDuplicates;
    public void SetPotentialDuplicates(HashSet<int> potentialDuplicates) {
        _potentialDuplicates = potentialDuplicates ?? new HashSet<int>();
    }

    public void AddKeyword(string keyword) {
        if (string.IsNullOrEmpty(keyword)) throw new ArgumentException("Keyword must be specified!");
        _keywords.Add(keyword.ToUpper());
    }

    public void RemoveKeyword(string keyword) {
        _keywords.Remove(keyword.ToUpper());
    }

    public void AddMetadata(MetadataInfo metadataInfo) {
        if (metadataInfo == null) throw new ArgumentException("Metadata must be specified!");
        _metadata.Add(metadataInfo);
    }

    public void RemoveMetadata(MetadataInfo metadataInfo) {
        _metadata.Remove(metadataInfo);
    }

    public void AddDuplicate(int duplicateFileID) {
        if (duplicateFileID <= 0) throw new ArgumentException("Duplicate file ID must be positive!");
        _duplicates.Add(duplicateFileID);
    }

    public void RemoveDuplicate(int duplicateFileID) {
        _duplicates.Remove(duplicateFileID);
    }

    public void AddPotentialDuplicate(int potentialDuplicateFileID) {
        if (potentialDuplicateFileID <= 0) throw new ArgumentException("Potential duplicate file ID must be positive!");
        _potentialDuplicates.Add(potentialDuplicateFileID);
    }

    public void RemovePotentialDuplicate(int potentialDuplicateFileID) {
        _potentialDuplicates.Remove(potentialDuplicateFileID);
    }
}
