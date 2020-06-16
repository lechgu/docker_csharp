# Docker for the C# developers Crash Course

## What are we going to learn?

The goal is to help you achieve practical, working knowledge of Docker so you can build containerized micro-services in C# with relative ease. We are going to use .Net core as it is cross-platform and well supported on Linux-based Docker containers. Having said that, the Windows-based Docker containers exist too, but the haven't got much traction.

## What tools do I need?

Ironically, Windows is not an absolute prerequisite for this exercise. One can build C#-powered containerized micro-services on Mac or Linux too.
Windows will work too, of course.

- First, you need [Docker](https://www.docker.com/) installed. Follow the link and install `Docker desktop` for your system.
- Second, you need the [.Net core SDK](https://dotnet.microsoft.com). Install the latest version of .Net core SDK, which is 3.1 at the time of writing.
- You need a text editor. If you are not settled, consider [Visual Studio Code](https://code.visualstudio.com/) It is cross-platform, fast and it works great with C# and Docker.
- Many of the activities will be command-line based. Use the shell available to you. To keep things as cross-platform as possible, I am going to use Git Bash on Windows. If you don't know, Visual Studio Code has great support for working with the command line.

#### Sanity check

In the command line run

```
docker --version
```

and

```
dotnet --version
```

If you get some sane answer in both cases, you are all set.

## Your first encounter with Docker

Run

```
docker run alpine date
```

You should see current time displayed in the shell

#### What just happened?

by running this command you instructed docker to start a container based on the `alpine` image, execute the command `date` and display its output in the shell. If you did not have the `alpine` image on your box (most likely), docker downloaded it from the [Dockerhub](https://hub.docker.com/), the public repository of well known Docker images.

#### Containers, images... I am confused.

You can think about it this way: if an Image is analogous to an '.exe' file, container is the process which runs once you start this .exe file.

#### Ok, what is this alpine thing, anyway?

[Alpine](https://alpinelinux.org/) is a Linux distribution which is quite popular in the Docker community, because it is very minimalistic and small.

#### Your turn

instead of the `date` try to run other commands. `pwd`, `whoami` are good examples

## Explore the Alpine container

This time start the container with the following:

```
docker run -it alpine sh
```

the `it` flag indicates to start the container in the _interactive_ mode and run `sh` command which is Alpine's default shell. You will end up inside the container. Look around, explore what is there using `ls` and and `cd` commands. `cat` will display the content of the text file, for example

```
cat /etc/hosts
```

If you are familiar with the `vi` editor you can create or edit some files too.
By default, alpine does not have any fancy tools to access Internet.
This can be mitigated:

```
apk add curl
```

`apk` is Alpine's package installer, you can install various apps with it. Now, try to do

```
curl http://www.microsoft.com
```

You should get the Microsoft's home page, in the text format, of course.

When you done exploring, exit the container:

```
exit
```

You should get back to your normal shell

#### Your turn

The Alpine image is supposed to be small, But how small is it, exactly? Hint: `docker images` gives the list of images available on your system.

## C# console application

Now we are going to replicate the experience we had running `date` in the docker container, but this time with a C# app.
Let's create the app:

```
mkdir date
cd date
dotnet new console
```

This scaffolds a simple .net core command line application.
Go to the `Program.cs` and replace the line

```
Console.WriteLine("Hello World!");
```

with

```
Console.WriteLine(DateTime.UtcNow);
```

#### Sanity check

```
dotnet run
```

This should display the current UTC time.

## Dockerize C# command line application

Now, let's put our application to the Docker image. For that, we'll need a `Dockerfile`. `Dockerfile` is essentially a list of instructions which Docker is supposed to follow in order to build your image.
create a file Dockerfile` and put the following line into it:

```
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine
```

This instructs Docker to build our image on top of predefined Alpine-based image but which has .net core sdk 3.1 installed
Docker images are layered, every new layer adds additional functionality. We could have start from the bare Alpine image and install the .net core sdk ourselves, that is pretty straightforward but quite tedious. So we'll start with the base image Microsoft provides us.
Now we can try to build the image

```
docker build -t csharpdate .
```

the `-t` parameter specifies the name of the image, which is going to be `csharpdate` in this case. The argument `.`(dot) instructs Docker where to look for the `Dockerfile`, which is the current directory in our case.
After few seconds the image is going to be buit. As mentioned before, it is layered and has Alpine underneath, so we can explore it as we did before:

```
docker run -it csharpdate sh
```

We end up in the familiar Alpine shell. But this time it has also .net core tools installed, for instance, you can run `dotnet`.

Anyway, the next task is to copy the sources into the docker image and build them
Append the following lines to the `Dockerfile`

```
WORKDIR /app
COPY date.csproj Program.cs ./
```

The `WORKDIR` directive specifies what is the _current directory_ in the Docker image, `/app` in our case. It will be created if needed.
The COPY directive instructs Docker to copy our C# source file and associated `.csproject` file into the image's _current directory_
After that you will have everything you need to build the final binary in image.

#### Your turn

Rebuild the image, launch the container and verify that the files are indeed there. Also verify that the current directory is `/app`.

The last step is to actually build the application.
Append to the `Dockerfile`:

```
RUN dotnet publish -c Release -o build --self-contained=false
```

This builds the C# project in the release mode and places the output in the `build` subdirectory of the current directory, that is into `/app/build`

Once the image is rebuilt we can launch our program inside the container:

```
docker run csharpdate dotnet /app/build/date.dll
```

this will print again the current UTC time, but this time coming from our C# program!

Specifying the full path to the binary we want to run every time is quite tedious. Docker has a directive which allows to designate some command line as the default.
Append to your `DOCKERFILE`:

```
CMD [ "dotnet", "/app/build/date.dll" ]
```

Rebuild the image and now you can launch the binary simply running:

```
dotnet run csharpdate
```

#### Your turn

Experiment: Try to modify the code in the `Program.cs`, try to specify different build flags and so on

## C# microservice

Ok, containerizing a command line application is cool, but it does not look like a microservice, does it?
In this part we are going to create a simple service which returns the current UTC time from the REST endpoint.
We are going to use Asp.Net Core for that. But don't be scared! We are not going to use most of the Asp.Net Core machinery to get our service working. The secret is that, in fact, self-hosted Asp.Net Core applications are, in fact, command line applications!
So let's create a directory `datems` at the same level as the `date` directory and initialize a .Net Core command line application there:

```
dotnet new console
```

We will need to bring Asp.Net Core -specific dependencies into the project. This is hush-hush, but the simplest way to do int is to open the `datems.csproj` and replace the line

```
<Project Sdk="Microsoft.NET.Sdk">
```

with

```
<Project Sdk="Microsoft.NET.Sdk.Web">
```

Don't tell anyone, let them suffer through the official tutorials.

If we run the project

```
dotnet run

```

it will still happily run and display the ubiquitous "Hello World!".

Asp.Net Core is actually a pretty cool and rich framework; if you build Web apps or Rest services on a daily basis, I encourage to study it deeper. But we will use a bare minimum of its features to get our microservice working. So don't expect best Asp.Net practices here.

We'll need to use some namespaces. Here is the full list, feel free to add these into the `Program.cs`

```
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
```

To start a Asp.Net Core app we need to:

- create an instance of `IHostBuilder`
- ask it to _Build_ that host
- finally to run it.

We are going to do all these steps in our Main method:

```
 static void Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder
                .UseUrls("http://0.0.0.0:5000")
                .UseStartup<Program>();
        });

        builder.Build().Run();
    }
```

There is some Asp.Net Core magic here. Feel free to explore that on your own. For example, we indicate that we are going to listen on all network interfaces on port 5000.
The key here is the `webBuilder.UseStartup<Program>` call.
This indicates that our `Program` class defines the Asp.Net Core _pipeline_.

In order satisfy this Asp.Net Core requirement, we need to implement two methods, `ConfigureServices` and `Configure`
The simple ConfigureServices looks like this:

```
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();
}
```

In here we tell that we want to use AspNet Core Controllers which are going to serve as our REST endpoints.
the `Configure` looks like this:

```
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseRouting();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapDefaultControllerRoute();
    });
}
```

We simply tell Asp.Net Core that we will need to _route_ the requests depending on the URL, and that the default route is just fine.

#### Sanity Check

Run

```
dotnet run
```

If everything is ok, the app should start and listen for the Http request on the default port 5000
However, if we try to navigate in the browser to http://localhost:5000 we'll be greeted with tho "not found" error. What gives?

The answer is that we asked to wire up the default route which, by convention implies the "Home" controller with the "Index" action.

## The controller

Let's implement the required controller. Nothing fancy.
Create the class HomeController (you can use the same `Program.cs` file):

```
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return Ok(42);
    }
}
```

We are saying, return the http Ok response (200) with the payload "42".

#### Another sanity check

Restart the app and navigate in the browser to http://localhost:5000
You should see the "42" showing up on the page.

### return the Json response

The last step is to return an object formatted as JSON, instead of the answer to life, universe and everything.
Let's create a response class:

```
public class TimeResponse
{
    public DateTime Current { get { return DateTime.UtcNow; } }
}
```

and return that from the controller:

```
public IActionResult Index()
{
    return Ok(new TimeResponse());
}
```

Restart the app, refresh the browser and you should see the current time formatted as JSON.

Our "real" microservice is pretty much ready.

## Build Docker image for the microservice

Ironically our `Dockerfile` for the command line application should mostly work for the microservice, so you can just copy the `Dockerfile` from the `date` directory to the `datems` directory.
do the following tweaks in that `Dockefile`:

- correct the C# project file name from `date.csproj` to `datems.csproj`
- correct the app dll name in the `CMD` directive from `date.dll` to `datems.dll`

Build the new Docker image in the `datems` directory, this time calling the image `csharpms`:

```
docker build -t csharpms .
```

Let's start the container

```
docker run -d csharpms
```

The `-d` flag indicates to start the container in detached mode, so it detaches from the console

The Docker should start the container and print its id, a long hexadecimal number
let's dive into the running container.
Run

```
docker exec -it [container id] sh
```

Hint: you don't need to provide the full container id. A few of initial hexadecimal digits should be enough for the Docker to figure out which container you have in mind.
For example:

```
docker exec -it 9e544440bd sh
```

Your id will be obviously different.
You should end up inside the running container!
check if your app is running:

```
ps aux
```

You should see the something like

```
PID   USER     TIME  COMMAND
    1 root      0:00 dotnet /app/build/datems.dll
```

This shows that our service is running.
Try to see which ports are open:

```
netstat -na
```

you should see the port 5000 is listening. This is our microservice!

#### Your turn

Use curl inside the container to get the answer from the microservice.
Reminder, you can install curl in the Alpine based container by running

```
apk add curl
```

Well, the microservice seems to listen on port 5000, but how can we reach it, say from our box? Your machine has very different idea what is the port 5000 than the container.

## Port mapping

Exit the container and stop it by executing

```
docker rm -f [container_id]
```

If you forgot what is the container id, you can list all running containers with

```
docker ps
```

Port mapping in Docker allows to attach a certain port on the machine to a specific port in the container. Let's say that we want the port 5000 in the container correspond to the port 9090 on your machine.
Start the container this way:

```
docker run -d -p 9090:5000 csharpms
```

Now all tcp/ip traffic to and from port 9090 on our machine will actually end up in the port 5000 in the container

#### Sanity check

in the browser, navigate to http://localhost:9090. You should get your nice UTC time back.

## Shrinking image size with multi-stage builds

`docker images` will display all images we have on our box, together with their sizes.

If we check the size of the `csharpms` image it is quite big. On my machine it takes 422 MB. Large image cause higher resources' usage and slow downloads. Can we shrink our image? Sure we can! One of the most important reasons why the image is so bloated is that it contains whole .Net Core 3.1 SDK. Well, we need SDK to build our microservice binary, but we not necessary need it to run it. The leaner image might suffice.

Docker supports so called _multi-stage_ builds when you copy specific files not from the host machine but from another Docker image, possibly unrelated to one we are building.
So here is the plan:

- In the first (or _build_) stage we will use the SDK image as before to build the binary.
- The second (the _final_) will be based on a leaner baser image into which we will copy the binary produced by the _build_ stage.
  After the _final_ stage is ready, the _build_ stage will be effectively discarded

Here is the resulting `Dockerfile`:

```
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine as build

WORKDIR /app
COPY datems.csproj Program.cs ./
RUN dotnet publish -c Release -o build --self-contained=false

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine as final
RUN mkdir /app
COPY --from=build /app/build/* /app/

CMD [ "dotnet", "/app/datems.dll" ]
```

Rebuild the image and check its size

```
docker build -t csharpms .
docker images | grep csharpms
```

On my machine it takes 105 MB, 4 time smaller then original. Not bad!

#### Sanity check

Start the container from the final image

```
docker run -d -p 9090:5000 csharpms
```

check with the browser or `curl` that it works as well as before.

## Summary

Our microservice is containerized and ready. Granted, this is a toy microservice with trivial functionality. But now you have skills to build a Docker image with the C# service as complex as you want.
