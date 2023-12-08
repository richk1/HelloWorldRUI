using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

using CommunityToolkit.Mvvm.ComponentModel;

namespace HelloWorldBasic;
public partial class MainWindow : Window
{
  private readonly AppViewModel viewModel;
  public MainWindow() {
    InitializeComponent();
    viewModel = new AppViewModel();

    GreetingTextBlock.SetBinding(TextBlock.TextProperty, new Binding("Greeting") { Source = viewModel });
    LangTextBlock.SetBinding(TextBlock.TextProperty, new Binding("Language") { Source = viewModel });
  }
}

public partial class AppViewModel : ObservableObject
{
  [ObservableProperty] private string greeting = "Greeting";
  [ObservableProperty] private string language = "Language";
  private readonly Dictionary<string, string> greetingsDict;
  private readonly string[] keys;
  private readonly int maxCount = 100;
  private readonly int dwellDuration = 2;

  private readonly DispatcherTimer timer;
  private readonly EventHandler th;
  private int tickCount = 0;

  public AppViewModel() {
    greetingsDict = new Dictionary<string, string>() {
      { "English", "Hello World!" },
      { "French", "Bonjour le monde!" },
      { "German", "Hallo Welt!" },
      { "Japanese", "Kon'nichiwa sekai!" },
      { "Spanish", "¡Hola Mundo!" },
    };
    keys = [.. greetingsDict.Keys];

    // Create a timer and associated event handler to rotate through the languages
    timer = new() { Interval = TimeSpan.FromSeconds(dwellDuration) };
    timer.Tick += (th = new EventHandler(TickHandler));
    timer.Start();
  }

  // update Greeting when language changes
  partial void OnLanguageChanged(string value) {
    Greeting = keys.Contains(value) ? greetingsDict[value] : string.Empty;
  }

  // select next language every 2 seconds (100 times)
  private void TickHandler(object? sender, EventArgs e) {
    Language = keys[tickCount % keys.Length];

    tickCount++;
    if (tickCount >= maxCount) {
      timer.Stop();
      timer.Tick -= th;
    }
  }
}
