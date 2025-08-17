namespace PhotoUtil;

using System;

public class MetadataInfo {
    private string _directory;
    private string _tag;
    private string _description;

    public MetadataInfo(string directory, string tag, string description) {
        SetDirectory(directory);
        SetTag(tag);
        SetDescription(description);
    }

    public string GetDirectory() => _directory;
    public void SetDirectory(string directory) {
        _directory = directory ?? "";
    }

    public string GetTag() => _tag;
    public void SetTag(string tag) {
        _tag = tag ?? "";
    }

    public string GetDescription() => _description;
    public void SetDescription(string description) {
        _description = description ?? "";
    }
}
