namespace PhotoDB;

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using PhotoStatus;
using PhotoUtil;

public class DB {
    public static readonly string DefaultDbFilename = "photo_db.pdb";

    private DBData _data = new();
    private string _dbFilename = DefaultDbFilename;
    private bool _dataChanged = true;
    private StatusCode _statusCode = StatusCode.NoError;

    public DB(string dbFilename) {
        SetStatusCode(StatusCode.NoError);
        SetDbFilename(dbFilename);
        ReadDB();
    }

    public bool IsChanged() => _dataChanged;
    public bool IsSaved() => !_dataChanged;
    public void DataChanged(bool changed) => _dataChanged = changed;
    public void DataSaved(bool saved) => _dataChanged = !saved;

    public string GetDbFilename() => _dbFilename;
    public void SetDbFilename(string filename) {
        if (string.IsNullOrEmpty(filename)) throw new ArgumentException("DB filename must be specified!");
        if (_dbFilename != filename) {
            _dbFilename = filename;
            DataChanged(true);
        }
    }

    public StatusCode GetStatusCode() => _statusCode;
    public void SetStatusCode(StatusCode code) => _statusCode = code;

    public void ReadDB() {
        try {
            using var stream = new FileStream(_dbFilename, FileMode.Open);
            // TODO
            // var formatter = new BinaryFormatter(); 
            // _data = (DBData)formatter.Deserialize(stream);
            DataChanged(false);
            SetStatusCode(StatusCode.NoError);
        } catch (FileNotFoundException) {
            SetStatusCode(StatusCode.DbFileDoesNotExist);
        // TODO
        // } catch (SerializationException) {
        //     SetStatusCode(StatusCode.DbFileIncompatibleFormat);
        } catch (IOException) {
            SetStatusCode(StatusCode.DbFileReadError);
        }
    }

    public void WriteDB() {
        try {
            using var stream = new FileStream(_dbFilename, FileMode.Create);
            // TODO
            // var formatter = new BinaryFormatter();
            // formatter.Serialize(stream, _data);
            DataSaved(true);
            SetStatusCode(StatusCode.NoError);
        // TODO
        // } catch (SerializationException) {
        //     SetStatusCode(StatusCode.DbFileNotSerializable);
        } catch (IOException) {
            SetStatusCode(StatusCode.DbFileWriteError);
        }
    }

    public int AddFile(DBFile file) {
        var keywords = new HashSet<string>();
        int oldID = _data.GetFileID(file.GetFullpath());
        if (oldID != 0) {
            file.SetID(oldID);
            keywords = GetFile(oldID).GetKeywords();
            RemoveFile(oldID);
        } else {
            file.SetID(NextFileID());
        }

        _data.AddFile(file);
        int id = file.GetID();
        _data.AddFilePath(file.GetFullpath(), id);
        _data.AddFileLocation(file.GetLocation(), id);
        _data.AddFileFilename(file.GetFilename(), id);
        _data.AddFileExtension(file.GetExtension(), id);
        _data.AddFileTimestamp(file.GetTimestamp(), id);
        _data.AddFileSize(file.GetSize(), id);
        _data.AddFileChecksum(file.GetChecksum(), id);

        if (oldID != 0) {
            foreach (var keyword in keywords) AddKeyword(keyword, id);
        }

        foreach (var meta in file.GetMetadata()) {
            _data.AddFileMetadataTag(meta.GetTag(), id);
        }

        foreach (var pid in _data.FindPotentialDuplicatesIDs(file.GetSize(), file.GetChecksum())) {
            if (pid != id) {
                file.AddPotentialDuplicate(pid);
                var other = _data.GetFile(pid);
                other?.AddPotentialDuplicate(id);
                _data.AddPotentialDuplicate(id);
                _data.AddPotentialDuplicate(pid);
                AddKeyword("DUP?", id);
                AddKeyword("DUP?", pid);
            }
        }

        DataChanged(true);
        return oldID;
    }

    public void RemoveFile(int id) {
        var file = _data.GetFile(id);
        if (file == null) return;

        _data.RemoveFile(id);
        _data.RemoveFilePath(file.GetFullpath());
        _data.RemoveFileLocation(file.GetLocation(), id);
        _data.RemoveFileFilename(file.GetFilename(), id);
        _data.RemoveFileExtension(file.GetExtension(), id);
        _data.RemoveFileTimestamp(file.GetTimestamp(), id);
        _data.RemoveFileSize(file.GetSize(), id);
        _data.RemoveFileChecksum(file.GetChecksum(), id);

        foreach (var kw in file.GetKeywords()) _data.RemoveFileKeyword(kw, id);
        foreach (var meta in file.GetMetadata()) _data.RemoveFileMetadataTag(meta.GetTag(), id);

        RemoveFileDuplicateInformation(file);
        DataChanged(true);
    }

    public void RemoveFileDuplicateInformation(DBFile file) {
        int id = file.GetID();

        foreach (var dupID in file.GetDuplicates()) {
            var dup = _data.GetFile(dupID);
            dup?.RemoveDuplicate(id);
            if (dup?.GetDuplicates().Count == 0) {
                _data.RemoveDuplicate(dupID);
                RemoveKeyword("DUP", dupID);
            }
        }

        foreach (var pid in file.GetPotentialDuplicates()) {
            var other = _data.GetFile(pid);
            other?.RemovePotentialDuplicate(id);
            if (other?.GetPotentialDuplicates().Count == 0) {
                _data.RemovePotentialDuplicate(pid);
                RemoveKeyword("DUP?", pid);
            }
        }

        file.SetDuplicates(null);
        file.SetPotentialDuplicates(null);
        _data.RemoveDuplicate(id);
        _data.RemovePotentialDuplicate(id);
        RemoveKeyword("DUP", id);
        RemoveKeyword("DUP?", id);
    }

    public Dictionary<int, int> ProcessDuplicates(int fileID) {
        var found = new Dictionary<int, int>();
        var file = _data.GetFile(fileID);
        var candidates = _data.FindPotentialDuplicatesIDs(file.GetSize(), file.GetChecksum());
        var confirmed = new HashSet<int> { fileID };

        foreach (var id in candidates) {
            if (id != fileID && FileSystem.CompareFiles(file.GetFullpath(), _data.GetFile(id).GetFullpath())) {
                confirmed.Add(id);
            }
        }

        if (confirmed.Count > 1) {
            foreach (var id in confirmed) {
                RemoveFileDuplicateInformation(_data.GetFile(id));
                found[id] = confirmed.Count - 1;
            }
            foreach (var id in confirmed) {
                var f = _data.GetFile(id);
                foreach (var dupID in confirmed) {
                    if (id != dupID) {
                        f.AddDuplicate(dupID);
                        AddKeyword("DUP", id);
                        _data.AddDuplicate(id);
                    }
                }
            }
        } else {
            RemoveFileDuplicateInformation(file);
        }

        DataChanged(true);
        return found;
    }

    public int NextFileID() => _data.NextFileID();
    public int GetFileID(string fullpath) => _data.GetFileID(fullpath);
    public int GetFileID(string location, string filename, string extension) => _data.GetFileID(location, filename, extension);
    public DBFile? GetFile(int fileID) => _data.GetFile(fileID);

    public HashSet<int>? GetFileIDs(string key, char where) {
        return where switch {
            'F' => _data.GetFileID(key) is var id && id != 0 ? new HashSet<int> { id } : null,
            'D' => _data.GetFileIDsInLocation(key),
            'K' => _data.GetFileIDsWithKeyword(key),
            _ => throw new ArgumentException("Invalid location type")
        };
    }

    public void AddKeyword(string keyword, int fileID) {
        var file = _data.GetFile(fileID);
        file?.AddKeyword(keyword);
        _data.AddFileKeyword(keyword, fileID);
        DataChanged(true);
    }

    public void RemoveKeyword(string keyword, int fileID) {
        var file = _data.GetFile(fileID);
        file?.RemoveKeyword(keyword);
        _data.RemoveFileKeyword(keyword, fileID);
        DataChanged(true);
    }

    public List<string> GetKeywords() => _data.GetKeywords();
    public List<string> GetDirectories() => _data.GetDirectories();
    public Dictionary<string, int> GetDBStatistics() => _data.GetDBStatistics();
}
