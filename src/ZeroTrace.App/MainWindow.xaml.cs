using System.Windows;
using ZeroTrace.App.ViewModels;
namespace ZeroTrace.App;
public partial class MainWindow : Window { public MainWindow(){ InitializeComponent(); DataContext=new MainViewModel(); } }
