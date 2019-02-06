using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace HelloWorldRUI
{
    public partial class MainWindow : ReactiveWindow<AppViewModel>
    {
        public MainWindow()
        {
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

        public extern string Lang { [ObservableAsProperty] get; }
        //private readonly ObservableAsPropertyHelper<string> _lang;
        //public string Lang => _lang.Value;

        public AppViewModel()
        {
            Dictionary<string, string> Greetings = new Dictionary<string, string>() {
                { "English", "Hello World!" },
                { "French", "Bonjour le monde!" },
                { "German", "Hallo Welt!" },
                { "Japanese", "Kon'nichiwa sekai!" },
                { "Spanish", "¡Hola Mundo!" },
            };
            //var keys = Greetings.Keys.ToArray();
            string[] keys = Greetings.Keys.ToArray();

            // select next language every 2 seconds (100 times)
            //_lang = Observable.Interval(TimeSpan.FromSeconds(2))
            Observable.Interval(TimeSpan.FromSeconds(2))
                .Take(100)
                .Select(_ => keys[(Array.IndexOf(keys, Lang) + 1) % keys.Count()])
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
