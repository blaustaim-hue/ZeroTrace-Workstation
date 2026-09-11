using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ZeroTrace.Diagnostics.System;
using ZeroTrace.Enterprise;
namespace ZeroTrace.App.ViewModels;
public sealed class MainViewModel : INotifyPropertyChanged { private string _statusHeadline="Ready"; private string _modeText="Mode: Read-only"; public MainViewModel()=>RunDiagnosticCommand=new RelayCommand(RunFoundationDiagnostic); public string StatusHeadline { get=>_statusHeadline; private set { _statusHeadline=value; OnPropertyChanged(); } } public string ModeText { get=>_modeText; private set { _modeText=value; OnPropertyChanged(); } } public ICommand RunDiagnosticCommand {get;} public event PropertyChangedEventHandler? PropertyChanged; private async void RunFoundationDiagnostic(){ StatusHeadline="Scanning..."; var mode=new EnterpriseDetector().Detect(); var result=await new BasicSystemDiagnostic().RunAsync(); StatusHeadline=result.Success?"Foundation PASS":"Foundation FAIL"; ModeText=$"Mode: {mode} • Evidence: {result.Evidence.Count}"; } private void OnPropertyChanged([CallerMemberName]string? n=null)=>PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(n)); }
