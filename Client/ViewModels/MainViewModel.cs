using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Input;
using Client.Commands;
using System.Windows;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using System.Windows.Media;
using System.Collections.Generic;

namespace Client.ViewModels;

public class MainViewModel : INotifyPropertyChanged {

    // Private Fields

    private string fileName;
    private BitmapImage _imageData;

    // Binding Properties

    public BitmapImage? ImageData { get => _imageData;
        set {
            _imageData = value;
            OnProperty();
        }
    }
    public ICommand? ScreenshotButtonCommand { get; set; }

    // Constructor

    public MainViewModel() {

        SetCommands();
        ListenClientAsync();
    }

    // Functions

    private void SetCommands() {

        ScreenshotButtonCommand = new RelayCommand(TakeScreenshot);
    }

    private void TakeScreenshot(object? param) {

        var client = new UdpClient();

        var Ip = IPAddress.Parse("127.0.0.1");
        var Port = 27002;

        var remoteEP = new IPEndPoint(Ip, Port);

        var msg = "takescreenshot";
        var len = 0;
        var buffer = Array.Empty<byte>();

        buffer = Encoding.Default.GetBytes(msg);
        client.SendAsync(buffer, remoteEP);
    }

    private async Task ListenClientAsync() {

        UdpClient server = new UdpClient(27003);
        var buffer = new byte[ushort.MaxValue - 29];
        var list = new List<byte>();

        while (true) {
            while (true) {
                var result = await server.ReceiveAsync();
                buffer = result.Buffer;
                var len = buffer.Length;
                if (len != ushort.MaxValue - 29) break;
                list.AddRange(buffer);
            }
            list.AddRange(buffer);
            ImageData = LoadImage(list.ToArray());
            list.Clear();
        }
    }

    public static BitmapImage LoadImage(byte[] imageData) {
        if (imageData == null || imageData.Length == 0) return null;

        var image = new BitmapImage();

        using (var mem = new MemoryStream(imageData)) {
            mem.Position = 0;
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = null;
            image.StreamSource = mem;
            image.EndInit();
        }
        image.Freeze();
        return image;
    }

    // INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnProperty([CallerMemberName] string? name = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
