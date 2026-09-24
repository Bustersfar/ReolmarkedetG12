using ReolmarkedetG12.UI.Services;

namespace ReolmarkedetG12.Tests.Fakes;

public class FakeDialogService : IDialogService
{
    public string? LastError { get; private set; }
    public int ErrorCount { get; private set; }
    public string? LastInfo { get; private set; }
    public bool ConfirmResult { get; set; } = true;

    public void ShowError(string message, string title)
    {
        LastError = message;
        ErrorCount++;
    }

    public void ShowInfo(string message, string title) => LastInfo = message;

    public bool Confirm(string message, string title) => ConfirmResult;
}