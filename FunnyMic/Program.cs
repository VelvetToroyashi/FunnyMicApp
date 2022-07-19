// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using NAudio.CoreAudioApi;
using NAudio.Wave;

var devices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);

var count = devices.Count;

Console.WriteLine();

Console.WriteLine("Pick a device to capture from:");

for (int i = 0; i < count; i++)
{
    var device = devices[i];
    Console.WriteLine("{0}: {1}", i, device.FriendlyName);
}

Console.Write("Selection: ");

var parsed = false;
int selection = 0;
do
{
    parsed = int.TryParse(Console.ReadLine(), out selection);
    
    if (selection < 0 || selection >= count)
    {
        Console.SetCursorPosition(11, Console.CursorTop); 
        Console.Write("                    ");
        Console.SetCursorPosition(11, Console.CursorTop);
        
        parsed = false;
    }
}
while (!parsed);

Console.Clear();

var inDevice = devices[selection];

devices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

Console.WriteLine("Pick a device to play the sound on:");
Console.WriteLine();

for (int i = 0; i < count; i++)
{
    var device = devices[i];
    Console.WriteLine("{0}: {1}", i, device.FriendlyName);
}

Console.Write("Selection: ");

parsed = false;
selection = 0;
do
{
    parsed = int.TryParse(Console.ReadLine().ToString(), out selection);
    
    if (selection < 0 || selection >= count)
    {
        Console.SetCursorPosition(11, Console.CursorTop); 
        Console.Write("                    ");
        Console.SetCursorPosition(11, Console.CursorTop);
        
        parsed = false;
    }
}
while (!parsed);

Console.Clear();
Console.WriteLine($"Capturing from {inDevice.FriendlyName}");
Console.WriteLine();

Console.WriteLine($"Playing on {devices[selection].FriendlyName}");
Console.WriteLine();

Console.WriteLine("Starting FFMpeg...");
var ffmpeg = Process.Start
(
    new ProcessStartInfo(
        "./ffmpeg",
        $"-hide_banner " +
        $"-v quiet " +
        $"-f dshow " +
        $"-re " + // in case you play music instead of mic audio :kioShrug:
        $@"-i audio=""{inDevice.FriendlyName}"" " +
        $"-f s16le " +
        $"-map 0:0 " +
        $"-ac 2 " +
        $"-ar 8000 " +
        $"pipe:1"
        )
{
    //CreateNoWindow = true,
    UseShellExecute = false,    
    RedirectStandardOutput = true
} );

// var buffer = new byte[8192];
//
// var sw = Stopwatch.StartNew();
//
// while (!ffmpeg.HasExited)
// {
//     var line = ffmpeg.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length);
//     Console.WriteLine($"{sw.ElapsedTicks} : Data: {line}");
// }
//
// return;

Console.WriteLine("FFMpeg started, initializing audio devices...");

//var opus = new OpusSource(ffmpeg.StandardOutput.BaseStream, 8000, 2);

Console.WriteLine("Initialized opus source");

using var sOut = new WasapiOut(devices[selection], AudioClientShareMode.Shared, true, 30);

sOut.Init(new RawSourceWaveStream(ffmpeg.StandardOutput.BaseStream, new(8000, 2)));

Console.WriteLine("Initialized audio devices");

sOut.Play();

Console.WriteLine("Audio should be active!");

var active = true;
Console.CancelKeyPress += (_, _) => active = false;

while (active)
{

    Thread.Sleep(40);
}
