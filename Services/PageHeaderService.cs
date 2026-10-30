namespace NawafAppointment.Services;

public class PageHeaderOptions
{
    public string Title { get; set; } = "مواعيد نواف";
    public string? Subtitle { get; set; }
    public string? IconClass { get; set; }
    public bool ShowAvatar { get; set; }
    public bool ShowAddButton { get; set; }
    public string AddButtonText { get; set; } = "إضافة موعد";
}

public class PageHeaderService
{
    public event Action? OnChange;

    public PageHeaderOptions Current { get; private set; } = new()
    {
        Title = "مواعيد نواف",
        Subtitle = "متابعة المواعيد الطبية",
        ShowAvatar = true,
        ShowAddButton = true
    };

    private Func<Task>? _addHandler;

    public void Configure(PageHeaderOptions options)
    {
        Current = options;
        Notify();
    }

    public void SetSubtitle(string? subtitle)
    {
        Current.Subtitle = subtitle;
        Notify();
    }

    public void RegisterAddHandler(Func<Task> handler) => _addHandler = handler;

    public void UnregisterAddHandler() => _addHandler = null;

    public async Task TriggerAddAsync()
    {
        if (_addHandler is not null)
            await _addHandler();
    }

    private void Notify() => OnChange?.Invoke();
}
