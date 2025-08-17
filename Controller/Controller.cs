namespace PhotoController;

using PhotoStatus;
using PhotoDB;
using PhotoView;

public class Controller {
    private View _view;
    private DB _db;
    private CmdInterpreter _interpreter;

    public Controller(string[] args) {
        _view = new View();
        _view.FullProgramInfo();

        string dbFilename = GetFilename(args);
        _db = new DB(dbFilename);
        switch (_db.GetStatusCode()) {
            case StatusCode.NoError:
                break;
            case StatusCode.DbFileDoesNotExist:
            case StatusCode.DbFileIncompatibleFormat:
            case StatusCode.DbFileReadError:
                _view.PrintStatus(_db.GetStatusCode());
                break;
            default:
                _view.PrintStatus(StatusCode.UnexpectedStatus);
                break;
        }

        _view.Print("");
        _view.PrintDBStatistics(_db.GetDBStatistics());

        _interpreter = new CmdInterpreter(_db, _view);
    }

    private string GetFilename(string[] args) {
        string fileName;
        if (args.Length == 0) {
            fileName = DB.DefaultDbFilename;
            _view.Print("The default name of the DB file will be used: " + fileName);
        } else {
            fileName = args[0];
            if (!fileName.Contains('.')) {
                fileName += ".pdb";
            }
            _view.Print("The DB filename: " + fileName);
        }
        return fileName;
    }

    public void Run() {
        _view.Print("");
        using (CLI cli = new CLI()) {
            _interpreter.SetCLI(cli);
            bool quit = false;
            while (!quit) {
                _view.PrintPrompt();
                Command cmd = cli.ReadCommand();
                _interpreter.ExecuteCommand(cmd);
                quit = _interpreter.GetQuitSignal();
            }
        }
    }
}
