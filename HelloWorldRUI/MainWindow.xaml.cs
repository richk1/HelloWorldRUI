using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace HelloWorldRUI
{
  public partial class MainWindow : ReactiveWindow<AppViewModel>
  {
    public MainWindow() {
      InitializeComponent();
      ViewModel = new AppViewModel();

      this.WhenActivated(d => {
        this.OneWayBind(ViewModel, viewModel => viewModel.Lang,
            view => view.LangTextBlock.Text).DisposeWith(d);
        this.OneWayBind(ViewModel, viewModel => viewModel.Greeting,
            view => view.GreetingTextBlock.Text).DisposeWith(d);
      });
    }
  }
}

// Normally the ViewModel goes in a separate file
namespace HelloWorldRUI
{
  public class AppViewModel : ReactiveObject
  {
    [Reactive] public string Greeting { get; set; } = "Greeting";
    [ObservableAsProperty] public string Lang { get; }

    private readonly int maxCount = 100;
    private readonly int dwellDuration = 2;

    public AppViewModel() {
      Dictionary<string, string> Greetings = new() {
                { "English", "Hello World!" },
                { "French", "Bonjour le monde!" },
                { "German", "Hallo Welt!" },
                { "Japanese", "Kon'nichiwa sekai!" },
                { "Spanish", "¡Hola Mundo!" },
            };
      string[] keys = [.. Greetings.Keys];

      // select next language every 2 seconds (100 times)
      Observable.Interval(TimeSpan.FromSeconds(dwellDuration))
          .Take(maxCount)
          .Select(_ => keys[(Array.IndexOf(keys, Lang) + 1) % keys.Length])
          .ObserveOn(RxApp.MainThreadScheduler)
          .ToPropertyEx(this, x => x.Lang, "Language");

      // update Greeting when language changes
      this.WhenAnyValue(x => x.Lang)
          .Where(lang => keys.Contains(lang))
          .Select(x => Greetings[x])
          .ObserveOn(RxApp.MainThreadScheduler)
          .Subscribe(x => Greeting = x);
    }
  }
}
