using System.Windows.Input;
namespace ZeroTrace.App.ViewModels;
public sealed class RelayCommand : ICommand { private readonly Action _execute; public RelayCommand(Action execute)=>_execute=execute; public event EventHandler? CanExecuteChanged; public bool CanExecute(object? p)=>true; public void Execute(object? p)=>_execute(); }
