namespace PhotoController;

using System;
using System.Collections.Generic;
using System.IO;
using PhotoView;

public class CLI : IDisposable {
    private TextReader _input;

    public CLI() {
        _input = Console.In;
    }

    public Command ReadCommand() {
        List<string> argList = new List<string>();

        try {
            string? line = _input.ReadLine();
            if (line != null) {
                var currentWord = new System.Text.StringBuilder();
                bool insideQuotes = false;

                foreach (char c in line) {
                    if (c == '"') {
                        if (!insideQuotes) {
                            insideQuotes = true;
                        } else {
                            insideQuotes = false;
                            argList.Add(currentWord.ToString());
                            currentWord.Clear();
                        }
                    } else if (char.IsWhiteSpace(c) && !insideQuotes) {
                        if (currentWord.Length > 0) {
                            argList.Add(currentWord.ToString());
                            currentWord.Clear();
                        }
                    } else {
                        currentWord.Append(c);
                    }
                }

                if (currentWord.Length > 0) {
                    argList.Add(currentWord.ToString());
                }
            }
        } catch (IOException) {
            Console.Error.WriteLine("IOException occurred");
        }

        string cmd = argList.Count > 0 ? argList[0] : "";
        string[] cmdArgs = argList.Count > 1 ? argList.GetRange(1, argList.Count - 1).ToArray() : Array.Empty<string>();

        return new Command(cmd, cmdArgs);
    }

    public char AskYesNo(View view, string message, bool cancel) {
        string prompt = cancel ? " (Yes/No/Cancel)" : " (Yes/No)";
        string? response;

        while (true) {
            view.Print(message + prompt + ": ", false);
            try {
                response = _input.ReadLine()?.Trim().ToLower();
            } catch (IOException) {
                Console.Error.WriteLine("IOException occurred while reading input");
                continue;
            }

            if (response == "y" || response == "yes") {
                return 'Y';
            } else if (response == "n" || response == "no") {
                return 'N';
            } else if (cancel && (response == "c" || response == "cancel")) {
                return 'C';
            } else {
                view.Print("Invalid response. Please enter 'Yes', 'No'" + (cancel ? ", or 'Cancel'" : "") + ".");
            }
        }
    }

    public void Dispose() {
        if (_input != null && _input != Console.In) {
            _input.Dispose();
        }
    }
}
