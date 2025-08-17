using PhotoDB;
using PhotoStatus;
using PhotoUtil;
using PhotoView;

namespace PhotoController;

public class CmdInterpreter
{
    private DB _db;
    private View _view;
    private StatusCode _statusCode;
    private bool _quitSignal;
    private CLI? _cli;

    public CmdInterpreter(DB db, View view)
    {
        _db = db;
        _view = view;
        _statusCode = StatusCode.NoError;
        _quitSignal = false;
        _cli = null;
    }

    public StatusCode GetStatusCode() => _statusCode;

    public void SetStatusCode(StatusCode statusCode) => _statusCode = statusCode;

    public bool GetQuitSignal() => _quitSignal;

    public void SetQuitSignal(bool quitSignal) => _quitSignal = quitSignal;

    public void SetCLI(CLI cli) => _cli = cli;

    public void ExecuteCommand(Command cmd)
    {
        SetStatusCode(StatusCode.NoError);

        string command = cmd.Name.ToUpper();

        switch (command)
        {
            case "":
                break;
            case "H":
            case "HELP":
                Help();
                break;
            case "AB":
            case "ABOUT":
                About();
                break;
            case "E":
            case "X":
            case "EXIT":
                Exit();
                break;
            case "SAVE":
                Save(cmd.Args);
                break;
            case "A":
            case "ADD":
                Add(cmd.Args);
                break;
            case "AK":
                AddKeyword(cmd.Args);
                break;
            case "R":
            case "REMOVE":
                Remove(cmd.Args);
                break;
            case "RK":
                RemoveKeyword(cmd.Args);
                break;
            case "L":
            case "LIST":
                List(cmd.Args);
                break;
            case "LK":
                ListKeywords(cmd.Args);
                break;
            case "LD":
            case "LF":
                ListDirectories(cmd.Args);
                break;
            case "D":
            case "DETAILS":
                Details(cmd.Args);
                break;
            case "DUP":
            case "DD":
            case "DUPLICATES":
                Duplicates(cmd.Args);
                break;
            case "S":
            case "SCAN":
                Scan(cmd.Args);
                break;
            default:
                SetStatusCode(StatusCode.UnknownCommand);
                _view.PrintStatus(GetStatusCode());
                break;
        }
    }

    private void Help()
    {
        _view.Print("List of available commands:");
        _view.Print("- HELP (H)");
        _view.Print("  Display page with list of commands.");
        _view.Print("- ABOUT (AB)");
        _view.Print("  Display information about program.");
        _view.Print("- EXIT (E, X)");
        _view.Print("  Exiting the program.");
        _view.Print("  If there are unsaved changes, the program will display a control question.");
        _view.Print("- SAVE [<db-filename>]");
        _view.Print("  Saving the current memory state to a local file.");
        _view.Print("  Default name for this file: photo_db.pdb");
        _view.Print("  New filename can be specified as parameter.");
        _view.Print("  The name of the file can also be specified as a parameter when starting the program.");
        _view.Print("- ADD (A)");
        _view.Print("    - ADD <folder> or <filename>");
        _view.Print("      Adds all images from the specified <folder> or");
        _view.Print("      only the one specified by <filename> to the in-memory database.");
        _view.Print("    - ADD KEYWORD <keyword> <folder> or <filename>");
        _view.Print("      All images from the specified folder <folder> or");
        _view.Print("      only the one specified by <filename> get the keyword specified by <keyword>.");
        _view.Print("- AK");
        _view.Print("  Short form for ADD KEYWORD command. For details, see ADD command.");
        _view.Print("- REMOVE (R)");
        _view.Print("    - REMOVE <folder> or <filename>");
        _view.Print("      Removes all images from the specified <folder> (including the folder) or");
        _view.Print("      only the one specified by <filename> from the in-memory database.");
        _view.Print("    - REMOVE KEYWORD <keyword> <folder> or <filename>");
        _view.Print("      only the one specified by <filename> will have the specified <keyword> removed from them.");
        _view.Print("- RK");
        _view.Print("  Short form for REMOVE KEYWORD command. For details, see REMOVE command.");
        _view.Print("- LIST (L)");
        _view.Print("    - LIST <keyword> or <folder> or <file>");
        _view.Print("      Lists all images that have the specified keyword or belong to the specified folder.");
        _view.Print("    - LIST KEYWORDS (LIST KEYS)");
        _view.Print("      Lists all existing keywords in the database.");
        _view.Print("    - LIST DIRECTORIES (LIST DIRS, LIST FOLDERS)");
        _view.Print("      Lists all existing directories (folders) in the database.");
        _view.Print("    - LIST");
        _view.Print("      Displays database statistics.");
        _view.Print("- LK");
        _view.Print("  Short form for LIST KEYWORDS command. For details, see LIST command.");
        _view.Print("- LD (LF)");
        _view.Print("  Short form for LIST DIRECTORIES command. For details, see LIST command.");
        _view.Print("- DETAILS (D)");
        _view.Print("  DETAILS <keyword> or <folder> or <file>");
        _view.Print("  Lists all images that have the given keyword or belong to the given folder or");
        _view.Print("  given file and displays detailed information about them.");
        _view.Print("- DUPLICATES (DUP, DD)");
        _view.Print("  DUPLICATES <keyword> or <folder> or <file>");
        _view.Print("  Finds duplicates in a set of images determined by a given parameter (comparing files byte by byte).");
        _view.Print("- SCAN (S)");
        _view.Print("  SCAN <keyword> or <folder> or <file>");
        _view.Print("  Compares the set of images determined by the given parameter with the current state on the disk.");
    }

    private void About()
    {
        _view.FullProgramInfo();
    }

    private void Exit()
    {
        if (_db.IsChanged())
        {
            if (_cli == null) throw new InvalidOperationException("Interpreter CLI is not initialized!");

            char response = _cli.AskYesNo(_view, "There are unsaved changes. Do you want to save them?", true);
            switch (response)
            {
                case 'Y':
                    Save();
                    if (_db.GetStatusCode() == StatusCode.NoError)
                        SetQuitSignal(true);
                    break;
                case 'N':
                    SetQuitSignal(true);
                    break;
                case 'C':
                default:
                    break;
            }
        }
        else
        {
            SetQuitSignal(true);
        }
    }

    private void Save(string[] args)
    {
        if (args.Length > 1)
        {
            SetStatusCode(StatusCode.InvalidNumberOfArguments);
            _view.PrintStatus(GetStatusCode());
            return;
        }

        if (args.Length == 1)
        {
            string newDbFilename = args[0];
            if (_db.GetDbFilename() != newDbFilename)
            {
                _db.SetDbFilename(newDbFilename);
            }
        }

        Save();
    }

    private void Save()
    {
        if (_db.IsChanged())
        {
            _db.WriteDB();

            switch (_db.GetStatusCode())
            {
                case StatusCode.NoError:
                    _view.Print($"Changes saved successfully (DB filename: '{_db.GetDbFilename()}').");
                    break;
                case StatusCode.DbFileNotSerializable:
                case StatusCode.DbFileWriteError:
                    _view.PrintStatus(_db.GetStatusCode());
                    break;
                default:
                    _view.PrintStatus(StatusCode.UnexpectedStatus);
                    break;
            }
        }
        else
        {
            _view.Print("There are no changes to save.");
        }
    }

    private void Add(string[] args)
    {
        if (args.Length >= 1 && (args[0].ToUpper() == "KEYWORD" || args[0].ToUpper() == "KEY"))
        {
            AddKeyword(args[1..]);
            return;
        }

        if (args.Length != 1)
        {
            SetStatusCode(StatusCode.InvalidNumberOfArguments);
            _view.PrintStatus(GetStatusCode());
            return;
        }

        string path = args[0];
        switch (FileSystem.CheckPath(path))
        {
            case 'F':
                AddFile(path, false);
                break;
            case 'D':
                AddDirectory(path);
                break;
            case 'E':
                SetStatusCode(StatusCode.PathDoesNotExist);
                _view.PrintStatus(GetStatusCode());
                break;
            default:
                throw new InvalidOperationException("Unknown FileSystem.CheckPath() result!");
        }
    }

    private void AddFile(string filename, bool fullPath)
    {
        string filenameOnly = fullPath ? FileSystem.ExtractFilename(filename) : filename;
        _view.Print($"Processing file '{filenameOnly}'... ", false);

        DBFile file = FileSystem.GetFileInformation(filename);

        switch (FileSystem.GetStatusCode())
        {
            case StatusCode.NoError:
                _view.Print(_db.AddFile(file) == 0 ? "Added." : "Updated.");
                break;
            case StatusCode.FileSystemError:
                SetStatusCode(StatusCode.FileSystemError);
                _view.Print("ERROR! (Error reading file)... Skipped.");
                break;
            case StatusCode.FileSystemNotFile:
                _view.Print("WARNING! (Not a file)... Skipped.");
                break;
            case StatusCode.FileSystemNotImage:
                _view.Print("WARNING! (Not an image)... Skipped.");
                break;
            default:
                throw new InvalidOperationException("Unknown FileSystem error code");
        }
    }

    private void AddDirectory(string directory)
    {
        _view.Print($"Processing directory '{directory}'... ", false);
        List<string> listOfFiles = FileSystem.FilesInDirectory(directory);
        if (listOfFiles.Count == 0 || FileSystem.GetStatusCode() == StatusCode.FileSystemError)
        {
            SetStatusCode(StatusCode.FileSystemError);
            _view.PrintStatus(GetStatusCode());
            return;
        }

        _view.Print($"(found {listOfFiles.Count - 1} file(s))");
        _view.Print("Full path: " + listOfFiles[0]);
        for (int i = 1; i < listOfFiles.Count; i++)
        {
            AddFile(listOfFiles[i], true);
        }
    }

    private void AddKeyword(string[] args)
    {
        if (args.Length != 2)
        {
            SetStatusCode(StatusCode.InvalidNumberOfArguments);
            _view.PrintStatus(GetStatusCode());
            return;
        }

        string keyword = args[0].ToUpper();
        string path = args[1];

        var fileIDs = _db.GetFileIDs(path, 'F');
        if (fileIDs != null)
        {
            _view.Print($"Adding the keyword '{keyword}' to the specified file.");
        }
        else
        {
            fileIDs = _db.GetFileIDs(path, 'D');
            if (fileIDs != null)
            {
                _view.Print($"Adding the keyword '{keyword}' to files in the specified directory.");
                _view.Print($"(found {fileIDs.Count} file(s))");
            }
        }

        if (fileIDs != null)
        {
            foreach (int fileId in fileIDs)
            {
                AddKeywordToFile(keyword, fileId);
            }
        }
        else
        {
            SetStatusCode(StatusCode.DbFileDirDoesNotExist);
            _view.PrintStatus(GetStatusCode());
        }
    }

    private void AddKeywordToFile(string keyword, int fileID)
    {
        DBFile file = _db.GetFile(fileID);
        _view.Print($"Processing file '{file.GetFilename()}.{file.GetExtension()}'... ", false);
        _db.AddKeyword(keyword, fileID);
        _view.Print($"Ok (fileID = {fileID}).");
    }

    private void Remove(string[] args)
    {
        if (args.Length >= 1 && (args[0].ToUpper() == "KEYWORD" || args[0].ToUpper() == "KEY"))
        {
            RemoveKeyword(args[1..]);
            return;
        }

        if (args.Length != 1)
        {
            SetStatusCode(StatusCode.InvalidNumberOfArguments);
            _view.PrintStatus(GetStatusCode());
            return;
        }

        string path = args[0];
        var fileIDs = _db.GetFileIDs(path, 'F');
        if (fileIDs == null)
        {
            fileIDs = _db.GetFileIDs(path, 'D');
            if (fileIDs != null)
            {
                _view.Print($"Processing directory '{path}'... ", false);
                _view.Print($"(found {fileIDs.Count} file(s))");
            }
            else
            {
                SetStatusCode(StatusCode.DbFileDirDoesNotExist);
                _view.PrintStatus(GetStatusCode());
            }
        }

        if (fileIDs != null)
        {
            foreach (int fileId in fileIDs.ToList())
            {
                RemoveFile(fileId);
            }
        }
    }

    private void RemoveFile(int fileID)
    {
        DBFile file = _db.GetFile(fileID);
        _view.Print($"Processing file '{file.GetFilename()}.{file.GetExtension()}'... ", false);
        _db.RemoveFile(fileID);
        _view.Print("Removed.");
    }

    private void RemoveKeyword(string[] args)
    {
        if (args.Length != 2)
        {
            SetStatusCode(StatusCode.InvalidNumberOfArguments);
            _view.PrintStatus(GetStatusCode());
            return;
        }

        string keyword = args[0].ToUpper();
        string path = args[1];

        var fileIDs = _db.GetFileIDs(path, 'F');
        if (fileIDs != null)
        {
            _view.Print($"Removing the keyword '{keyword}' from the specified file.");
        }
        else
        {
            fileIDs = _db.GetFileIDs(path, 'D');
            if (fileIDs != null)
            {
                _view.Print($"Removing the keyword '{keyword}' from files in the specified directory.");
                _view.Print($"(found {fileIDs.Count} file(s))");
            }
        }

        if (fileIDs != null)
        {
            foreach (int fileId in fileIDs)
            {
                RemoveKeywordFromFile(keyword, fileId);
            }
        }
        else
        {
            SetStatusCode(StatusCode.DbFileDirDoesNotExist);
            _view.PrintStatus(GetStatusCode());
        }
    }

    private void RemoveKeywordFromFile(string keyword, int fileID)
    {
        DBFile file = _db.GetFile(fileID);
        _view.Print($"Processing file '{file.GetFilename()}.{file.GetExtension()}'... ", false);
        _db.RemoveKeyword(keyword, fileID);
        _view.Print($"Ok (fileID = {fileID}).");
    }

    private void List(string[] args)
    {
        if (args.Length >= 1 && (args[0].ToUpper() == "KEYWORDS" || args[0].ToUpper() == "KEYS"))
        {
            ListKeywords(args[1..]);
            return;
        }

        if (args.Length >= 1 && (args[0].ToUpper() == "DIRECTORIES" || args[0].ToUpper() == "DIRS" || args[0].ToUpper() == "FOLDERS"))
        {
            ListDirectories(args[1..]);
            return;
        }

        List(args, false);
    }

    private void List(string[] args, bool allDetails)
    {
        if (args.Length == 0)
        {
            _view.PrintDBStatistics(_db.GetDBStatistics());
            return;
        }
        else if (args.Length > 1)
        {
            SetStatusCode(StatusCode.InvalidNumberOfArguments);
            _view.PrintStatus(GetStatusCode());
            return;
        }

        string path = args[0];
        char detailsLevel = ' ';
        var fileIds = _db.GetFileIDs(path, 'F');

        if (fileIds != null)
        {
            detailsLevel = allDetails ? 'A' : 'F';
            _view.Print("The specified file exists in the database.");
        }
        else
        {
            fileIds = _db.GetFileIDs(path, 'D');
            if (fileIds != null)
            {
                detailsLevel = allDetails ? 'A' : 'F';
                _view.Print("The specified directory exists in the database.");
                _view.Print($"(found {fileIds.Count} file(s))");
            }
            else
            {
                string keyword = args[0].ToUpper();
                fileIds = _db.GetFileIDs(keyword, 'K');
                if (fileIds != null)
                {
                    detailsLevel = allDetails ? 'A' : 'D';
                    _view.Print("The specified keyword exists in the database.");
                    _view.Print($"(found {fileIds.Count} file(s))");
                }
            }
        }

        if (fileIds != null)
        {
            foreach (int fileId in fileIds)
            {
                ListFileInfo(fileId, detailsLevel);
            }
        }
        else
        {
            SetStatusCode(StatusCode.DbFileDirKeywordDoesNotExist);
            _view.PrintStatus(GetStatusCode());
        }
    }

    private void ListFileInfo(int fileId, char detailsLevel)
    {
        DBFile file = _db.GetFile(fileId);
        string filenameWithExtension = $"{file.GetFilename()}.{file.GetExtension()}";
        string formattedTimestamp = FormatDateTime(file.GetTimestamp());
        string fileSize = FileSystem.FormatFileSize(file.GetSize());
        string prefix = "   ";

        if (detailsLevel is 'F' or 'D')
        {
            if (filenameWithExtension.Length + fileSize.Length + 3 <= 60)
            {
                _view.Print($"{filenameWithExtension,-60}   {formattedTimestamp}   {fileSize}", false);
            }
            else
            {
                _view.Print($"{filenameWithExtension}   {formattedTimestamp}   {fileSize}", false);
            }

            if (file.GetKeywords().Contains("CHANGED"))
                _view.Print(" (CHANGED)");
            else if (file.GetKeywords().Contains("DELETED"))
                _view.Print(" (DELETED)");
            else
                _view.Print("");

            if (detailsLevel == 'D')
                _view.Print($"{prefix}in: {file.GetLocation()}");

            if (file.GetDuplicates().Count > 0)
                _view.Print($"{prefix}Duplicates: {file.GetDuplicates().Count}");

            if (file.GetPotentialDuplicates().Count > 0)
                _view.Print($"{prefix}Potential duplicates: {file.GetPotentialDuplicates().Count}");
        }
        else if (detailsLevel == 'A')
        {
            _view.Print(filenameWithExtension);
            _view.Print($"{prefix}in: {file.GetLocation()}");
            _view.Print($"{prefix}ID: {file.GetID()}");
            _view.Print($"{prefix}Timestamp: {formattedTimestamp}");
            _view.Print($"{prefix}Size: {fileSize} ({file.GetSize()}byte(s))");
            _view.Print($"{prefix}CRC32: {file.GetChecksum()}");

            _view.Print($"{prefix}Keywords: ", false);
            foreach (string keyword in file.GetKeywords())
            {
                _view.Print($"{keyword} ", false);
            }
            _view.Print("");

            var duplicates = file.GetDuplicates();
            if (duplicates.Count > 0)
            {
                _view.Print($"{prefix}Duplicates: {duplicates.Count}");
                foreach (int dupId in duplicates)
                    _view.Print($"{prefix}{prefix}{_db.GetFile(dupId).GetFullpath()}");
            }

            var potential = file.GetPotentialDuplicates();
            if (potential.Count > 0)
            {
                _view.Print($"{prefix}Potential duplicates: {potential.Count}");
                foreach (int dupId in potential)
                    _view.Print($"{prefix}{prefix}{_db.GetFile(dupId).GetFullpath()}");
            }

            _view.Print($"{prefix}Metadata:");
            foreach (var tag in file.GetMetadata())
            {
                _view.Print($"{prefix}{prefix}{tag.GetDirectory()} {tag.GetTag()} {tag.GetDescription()}");
            }

            _view.Print("------------------------------------------------------");
        }
    }

    private string FormatDateTime(string dateTime)
    {
        string year = dateTime[..4];
        string month = dateTime[4..6];
        string day = dateTime[6..8];
        string hour = dateTime[9..11];
        string minute = dateTime[11..13];
        string second = dateTime[13..15];
        return $"{day}.{month}.{year} {hour}:{minute}:{second}";
    }

    private void ListKeywords(string[] args)
    {
        if (args.Length > 0)
        {
            SetStatusCode(StatusCode.InvalidNumberOfArguments);
            _view.PrintStatus(GetStatusCode());
            return;
        }

        _view.Print("List of keywords in the database:");
        foreach (string keyword in _db.GetKeywords())
        {
            _view.Print($"   {keyword}");
        }
    }

    private void ListDirectories(string[] args)
    {
        if (args.Length > 0)
        {
            SetStatusCode(StatusCode.InvalidNumberOfArguments);
            _view.PrintStatus(GetStatusCode());
            return;
        }

        _view.Print("List of directories in the database:");
        foreach (string dir in _db.GetDirectories())
        {
            _view.Print($"   {dir}");
        }
    }

    private void Details(string[] args)
    {
        List(args, true);
    }

    private void Duplicates(string[] args)
    {
        if (args.Length != 1)
        {
            SetStatusCode(StatusCode.InvalidNumberOfArguments);
            _view.PrintStatus(GetStatusCode());
            return;
        }

        string path = args[0];
        var fileIds = _db.GetFileIDs(path, 'F');

        if (fileIds != null)
        {
            _view.Print("The specified file exists in the database.");
        }
        else
        {
            fileIds = _db.GetFileIDs(path, 'D');
            if (fileIds != null)
            {
                _view.Print("The specified directory exists in the database.");
                _view.Print($"(found {fileIds.Count} file(s))");
            }
            else
            {
                string keyword = path.ToUpper();
                fileIds = _db.GetFileIDs(keyword, 'K');
                if (fileIds != null)
                {
                    _view.Print("The specified keyword exists in the database.");
                    _view.Print($"(found {fileIds.Count} file(s))");
                }
            }
        }

        if (fileIds != null)
        {
            var allDuplicatesFound = new Dictionary<int, int>();
            foreach (var fileId in fileIds.ToList())
            {
                FindDuplicates(fileId, allDuplicatesFound);
            }

            if (allDuplicatesFound.Count == 0)
            {
                _view.Print("No duplicates found.");
            }
        }
        else
        {
            SetStatusCode(StatusCode.DbFileDirKeywordDoesNotExist);
            _view.PrintStatus(GetStatusCode());
        }
    }

    private void FindDuplicates(int fileId, Dictionary<int, int> allDuplicatesFound)
    {
        var file = _db.GetFile(fileId);
        _view.Print($"{file.GetFullpath()}... ", false);

        if (!allDuplicatesFound.TryGetValue(fileId, out var numOfDuplicates))
        {
            var newFound = _db.ProcessDuplicates(fileId);
            foreach (var kvp in newFound)
            {
                allDuplicatesFound[kvp.Key] = kvp.Value;
            }
            allDuplicatesFound.TryGetValue(fileId, out numOfDuplicates);
        }

        if (numOfDuplicates != 0)
        {
            _view.Print($"{numOfDuplicates} duplicate(s)");
        }
        else
        {
            _view.Print("no duplicates.");
        }
    }

    private void Scan(string[] args)
    {
        if (args.Length != 1)
        {
            SetStatusCode(StatusCode.InvalidNumberOfArguments);
            _view.PrintStatus(GetStatusCode());
            return;
        }

        string path = args[0];
        var fileIds = _db.GetFileIDs(path, 'F');

        if (fileIds != null)
        {
            _view.Print("The specified file exists in the database.");
        }
        else
        {
            fileIds = _db.GetFileIDs(path, 'D');
            if (fileIds != null)
            {
                _view.Print("The specified directory exists in the database.");
                _view.Print($"(found {fileIds.Count} file(s))");
            }
            else
            {
                string keyword = path.ToUpper();
                fileIds = _db.GetFileIDs(keyword, 'K');
                if (fileIds != null)
                {
                    _view.Print("The specified keyword exists in the database.");
                    _view.Print($"(found {fileIds.Count} file(s))");
                }
            }
        }

        if (fileIds != null)
        {
            foreach (var fileId in fileIds.ToList())
            {
                ScanFile(fileId);
            }
        }
        else
        {
            SetStatusCode(StatusCode.DbFileDirKeywordDoesNotExist);
            _view.PrintStatus(GetStatusCode());
        }
    }

    private void ScanFile(int fileId)
    {
        var dbFile = _db.GetFile(fileId);
        _view.Print($"{dbFile.GetFullpath()}... ", false);

        var currentFile = FileSystem.GetFileInformation(dbFile.GetFullpath());

        switch (FileSystem.GetStatusCode())
        {
            case StatusCode.NoError:
                if (FileChanged(dbFile, currentFile))
                {
                    _db.AddKeyword("CHANGED", fileId);
                    _db.RemoveKeyword("DELETED", fileId);
                    _view.Print("CHANGED.");
                }
                else
                {
                    _db.RemoveKeyword("CHANGED", fileId);
                    _db.RemoveKeyword("DELETED", fileId);
                    _view.Print("ok.");
                }
                break;

            case StatusCode.FileSystemError:
                SetStatusCode(StatusCode.FileSystemError);
                _view.Print("ERROR! (Error reading file)... Skipped.");
                break;

            case StatusCode.FileSystemNotFile:
                _db.AddKeyword("DELETED", fileId);
                _db.RemoveKeyword("CHANGED", fileId);
                _view.Print("DELETED.");
                break;

            case StatusCode.FileSystemNotImage:
                _db.AddKeyword("CHANGED", fileId);
                _db.RemoveKeyword("DELETED", fileId);
                _view.Print("CHANGED.");
                break;

            default:
                throw new InvalidOperationException("Unknown FileSystem error code");
        }
    }

    private bool FileChanged(DBFile dbFile, DBFile currentFile)
    {
        var dbMeta = dbFile.GetMetadata();
        var curMeta = currentFile.GetMetadata();

        if (dbFile.GetTimestamp() != currentFile.GetTimestamp() ||
            dbFile.GetSize() != currentFile.GetSize() ||
            dbFile.GetChecksum() != currentFile.GetChecksum() ||
            dbMeta.Count != curMeta.Count)
        {
            return true;
        }

        foreach (var dbTag in dbMeta)
        {
            bool found = curMeta.Any(curTag =>
                dbTag.GetDirectory() == curTag.GetDirectory() &&
                dbTag.GetTag() == curTag.GetTag() &&
                dbTag.GetDescription() == curTag.GetDescription());

            if (!found)
                return true;
        }

        return false;
    }
}
