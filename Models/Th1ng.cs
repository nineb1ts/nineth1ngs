using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace nineth1ngs.Models;

public partial class Th1ng : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string text = string.Empty;

    [ObservableProperty]
    private bool isCompleted;

    [ObservableProperty]
    private DateTime createdAt;

    [ObservableProperty]
    private DateTime? completedAt;

    private bool isEditing;

    private string editText = string.Empty;

    [NotMapped]
    public bool IsEditing
    {
        get => isEditing;
        set => SetProperty(ref isEditing, value);
    }

    [NotMapped]
    public string EditText
    {
        get => editText;
        set => SetProperty(ref editText, value);
    }
}
