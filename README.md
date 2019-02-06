# HelloWorldRUI
An example WPF HelloWorld program written using the ReactiveUI framework.

Original: 3 February 2019<br>
Updated: 5 February 2019


## Motivation
I had an unexpectedly difficult time figuring out how to get a simple starter
program working using ReactiveUI. I found some documentation sources useful, and
others confusing, or lacking SIMPLE examples for the current software versions.

The current ReactiveUI Getting Started page is pretty decent, but either I missed 
it when I started, or it wasn't as good then and has improved since. I remember 
trying to figure stuff out from the DynamicTrader program, which is a wonder 
to behold, but I found myself hopelessly confused by all the advanced stuff going 
on in there.

So, I thought I'd post my working example here on github. It's even more basic
than the 'Getting Started' example. No network calls or even any input. Just
a slightly fancy Hello World that does make use of a few ReactiveUI components.

I'm still learning a lot of this stuff (C#, ReactiveX, ReactiveUI), so I can't
yet say I have a firm understanding of it all, even all the parts used in this
simple example. This code works, but I wouldn't be at all surprised if someone
more experienced could point out at least a few 'bad' things in it.

(My biggest problem was getting the bindings to work. I think that was because 
I was using a version of the Splat service locater code in my app.xaml.cs file 
that was incompatible with the rest of my code. I had tried using 
different versions of that line that I copied from various documentation 
sources/examples, but not evidently ones that were in sync with the rest of my 
code. The sample that finally solved my problem was one of the two buried in the 
ReactvieUI source code itself - ReactiveDemo. This is the code the ReactiveUI 
'Getting Started' page currently walks you through.)

----
## This Example
What it does: <br>
Opens a Window that shows the greeting "Hello World", cycling between a few 
different languages.

----
### Development Environment
I'm using Visual Studio 2019 Enterprise Preview. I assume VS2017 Community Edition 
would work just as well. I'll likely switch to VS2019 Community Edition when the 
preview expires.

### Dependencies
You'll need to install the following NuGet Packages into your environment. They'll
in-turn load others that they require.

* NuGet Packges
    * ReactiveUI.WPF - Version 9.9.5 (3 Feb 2019)
    * ReactiveUI.Fody - Version 9.9.5 (3 Feb 2019)

Look in the packages.config file to see a complete list of current 
packages and version numbers. (Don't be surprised if the README doesn't stay up 
to date with the actual repo :) 

(Note: I perhaps complicated the example a bit by using Fody, but its pretty 
straitforward to use, has given me very little trouble, and makes the code look 
cleaner.)

----
### Source Files
#### File: App.xaml
Nothing really notable here. Standard WPF layout created by the Visual Studio 
project creater.

#### File: App.xaml.cs
Additions to the base template are:
##### Namespaces
These namespaces are used by the Locator statement below.
````csharp
using ReactiveUI;
using Splat;
using System.Reflection;
````
##### App Class Constructor
Add the following constructor code to the boilerplate produced by VS:
````csharp
public App()
{
    Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetCallingAssembly());
}
````
Note 
* the Splat "Locater..." line magically connects 
the Views and ViewModels. I've seen various other incarnations of this line, but 
this is the one that worked for me.
* Editorial Warning: This line of code is apparently an example of the 'service 
locater' design pattern, which amongst the computer science 'in crowd' is almost 
as uncool as using 'goto' statements. 
(You'll find muted apologies for using it in various ReactiveUI documentation.) 
I'm not sure if there's a good alternative to this that could be used, 
but I expect the ReactiveUI developers will switch to something else if they 
can find or think up something better.

#### File: MainWindow.xaml

##### Window properties
* The top level XAML object is ReactiveWindow rather than a vanilla WPF Window.
* x:TypeArguments ties the specific ViewModel class to this ReactiveWindow.
* Namespaces aliases are also defined for the project and for reactiveui

````xml
<reactiveui:ReactiveWindow 
    x:Class="HelloWorldRUI.MainWindow"
    x:TypeArguments="helloworldrui:AppViewModel"
    xmlns:helloworldrui="clr-namespace:HelloWorldRUI"
    xmlns:reactiveui="http://reactiveui.net"
    ...
    Title="HelloWorld RUI">
    ...
</reactiveui:ReactiveWindow>
````

##### Window Layout

In this example we are only going to display two strings, each in its own TextBlock. 
In a normal WPF project you'd bind the UI elements to ViewModel properties explicitly 
within the XAML.  Here, we simply ensure that each TextBlock has a unique name. 
The binding is done in the MainWindow's constructor code, which references 
the x:Name values.

````xml
<StackPanel>
    <TextBlock x:Name="GreetingTextBlock" />
    <TextBlock x:Name="LangTextBlock" />
</StackPanel>
````


#### File: MainWindow.xaml.cs
In the HelloWorldRUI project, I've left both the View and ViewModel code in the 
same file.  This is not normally done, but it doesn't hurt anything, and I think 
makes it a little easier to read as an example.

In a WPF MVVM program, the View class constructor is normally responsible for calling 
InitializeComponent() and creating an instance of the ViewModel. In this ReactiveUI 
code the constructor also binds the View and ViewModel properties, as shown below.

##### View Code
````csharp
using ReactiveUI;
using System.Reactive.Disposables;

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
````

##### ViewModel Code
Assuming you understand Reactive Observable pipelines, this code is 
reasonably straightforward. 

Using Fody slightly changes the declarations of the Reactive properties and 
ObservableAsPropertyHelpers compared to how you've probably seen it described in 
ReactiveUI documentation. Setting ObservableAsPropertyHelper values using ToProperty()
changes slightly as well. 
See the Fody readme (referenced farther below) for a more detailed explanation.

The Observable.Interval pipeline creates an observable that fires every 2 seconds, 
eventually resulting in changes to the Language displayed on the UI. The take(100) 
function terminates the pipeline after 100 iterations. The Select() function
specified will increment the Lang to the next one in the Greetings dictionary. 
ObserveOn(RxApp.MainThreadScheduler) needs to be used when changing any property 
bound to the View. ToPropertyEx() will set the value of the ObservableAsProperty 
Lang to the language output in the Select() function. Since Lang is bound to the 
LangTextBlock in the View, the UI will automatically reflect this change.  The 
initial value of the Observable Lang is passed as an argument to ToPropertyEx(), 
as it cant be set otherwise. 

The final observable pipeline will take care of updating the Greeting. This pipeline
is set to 'tick' whenever the ViewModel's Lang property changes (as specified by
the WhenAnyValue function). The Where() clause ignores any values that are not 
contained in the Keys array. The Select() function looks up the Greeting associated 
with the Lang. Again, ObserveOn makes sure our change is visible to the UI, and 
the Subscribe() function simply sets the Reactive property Greeting with the 
result created by Select. Since Greeting is a 'simple' Reactive property (not an 
Observable) you can just set its value. Its initial value is specified on its 
declaration line.

(Experiment: try removing one of the ObserveOn lines and see what happens.
It's useful for future reference to see for yourself how that error manifests.)

````csharp
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace HelloWorldRUI
{
    public class AppViewModel : ReactiveObject
    {
        [Reactive] public string Greeting { get; set; } = "Greeting";
        public extern string Lang { [ObservableAsProperty] get; }

        public AppViewModel()
        {
            Dictionary<string, string> Greetings = new Dictionary<string, string>() {
                { "English", "Hello World!" },
                { "French", "Bonjour le monde!" },
                { "German", "Hallo Welt!" },
                { "Japanese", "Kon'nichiwa sekai!" },
                { "Spanish", "¡Hola Mundo!" },
            };
            string[] keys = Greetings.Keys.ToArray();

            // select next language every 2 seconds (100 times)
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
````
----

## Misc Notes
### Fody
If you create these source files by hand, you may get an error when building the 
project the first time indicating that the FodyWeavers.xml file couldn't be found 
and had to be generated for you. In that case simply rebuild the project and the 
error should resolve itself. (Rebuilding after the FodyWeavers file was created
also resolved an Intellisense error I had in the MainWindow.xaml file which claimed 
it couldn't find the AppViewModel.)

### Intellisense
Integration of ReactiveUI and Fody with Intellisense seems to be incomplete or 
slow at times. Occasionally I think I found Intellisense reported errors which 
resolved themselves by rebuilding the project.

## Useful References
<h3>
<img src="https://d33wubrfki0l68.cloudfront.net/1b88ade41838e8036700f92c2fb945455fb45fb3/97288/assets/img/logo.png"
width="25"/>
ReactiveUI Project
</h3>

They're updating their website [~1/20/19], so some of these links may break.

Links:
* Main Website: https://reactiveui.net/
* API: https://reactiveui.net/api/
* GitHub: https://github.com/reactiveui/ReactiveUI
* Getting Started example - ReactiveDemo.sln:
   * master: https://github.com/reactiveui/ReactiveUI/tree/master/samples/getting-started
   * snapshot of the version I looked at (in case it changes): [0a8d8fb4afa90fc839026a66d1193fccdfb44938](https://github.com/reactiveui/ReactiveUI/tree/0a8d8fb4afa90fc839026a66d1193fccdfb44938/samples/getting-started)

### ReactiveUI.Fody:
The project source code has been absorbed into the ReactiveUI GitHub repo, but I 
haven't found the README file from the original repo in ReactiveUI. The original 
project README has an example of how to use it.

Links:
* Current Source in ReactiveUI GitHub: 
    https://github.com/reactiveui/ReactiveUI/tree/master/src/ReactiveUI.Fody
* Old GitHub project: https://github.com/kswoll/ReactiveUI.Fody
* Readme.md: https://github.com/kswoll/ReactiveUI.Fody/blob/rxui7beta/Readme.md

### ReactiveX (C#)
Reactive Extensions (ReactiveX) is a project which provides Reactive libraries 
for a number of programming languages and platforms. One such supported combination 
is C#/DotNet (aka the System.Reactive package). The base form of Reactive 
programming has no direct support for Graphical User Interfaces (thats where 
ReactiveUI comes in.) Its been around for quite a while, and there's some pretty 
good documentation for it, though not necessarily all up to date for C#/DotNet. 
However the older stuff is still VERY useful.
(You may notice that some of the maintainers of dotnet/reactive are also the same 
people supporting ReactiveUI)

Links:
* ReactiveX Website: http://reactivex.io/
* Rx.NET aka dotnet/Reactive aka System.Reactive GitHub repo: 
    https://github.com/dotnet/reactive
* Introduction to Rx online Book (~2012): 
    http://introtorx.com/Content/v1.0.10621.0/00_Foreword.html


### You, I and ReactiveUI (Book by Kent Boogaart, April 2018)
At the time I'm writing this, this book is the only organized, well written, 
reasonably complete reference/tutorial for ReactiveUI that I could find. I'm 
still working my way through it, and I'm finding it very useful. (Thanks to the 
author for making the effort to write and publish it!)

The biggest shortcoming of the book, for me as a newcomer, was the way the sample
code was organized. I found that it made it very difficult for me to observe and 
emulate each example as a standalone application.

I offer the following observations for my fellow noobs:<br>
* The sample code is organized as one big project. The files for each example 
(app files, MainWindow and View files, and ViewModel files), are spread 
out far apart from one another, making for a bit of work when trying to 
browse the files that belong to a single example.
* The project makes use of an external WPF UI toolkit (MahApps.metro) 
that I haven't found discussed in the text (as far as I've read). 
* The service locator code used in the project's app.xaml.cs doesn't match 
what I'm using in my HelloWorld (which I copied from ReactiveUI's 'getting 
started' sample). In fact, I dont see a description of RegisterViewsForModels() 
in the book. (Note: the book doesn't claim to describe all the APIs.) 
Probably the way the book does it would work for me if I understood the service 
locator API better.
* (I haven't finished reading it all yet. I'm skipping around reading the parts 
I think I need to get my code working.) 

Links
* Book GitHub repo: https://github.com/kentcb/YouIandReactiveUI

[ReactiveUILogo]:https://d33wubrfki0l68.cloudfront.net/1b88ade41838e8036700f92c2fb945455fb45fb3/97288/assets/img/logo.png
