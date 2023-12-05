using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Windows;
using System.Drawing;
using Client.Commands;
using System.Net.Sockets;
using System.Windows.Input;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Runtime.CompilerServices;

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

    private Bitmap CaptureScreen() {

        Bitmap memoryImage = new Bitmap(1920, 1080);
        try {
            System.Drawing.Size s = new (memoryImage.Width, memoryImage.Height);

            Graphics memoryGraphics = Graphics.FromImage(memoryImage);

            memoryGraphics.CopyFromScreen(0, 0, 0, 0, s);

        }
        catch (Exception ex) {
            MessageBox.Show(ex.Message);
        }

        return memoryImage;
    }

    static byte[] ConvertBitmapToByteArray(Bitmap bitmap) {
        using (MemoryStream stream = new MemoryStream()) {

            bitmap.Save(stream, ImageFormat.Jpeg);

            return stream.ToArray();
        }
    }

    private void TakeScreenshot(object? param) {

        var client = new UdpClient();

        var connectEP = new IPEndPoint(IPAddress.Parse("127.0.0.2"), 27002);


        Task.Run(async () => {
            while (true) {

                var buffer = ConvertBitmapToByteArray(CaptureScreen());
                var chunks = buffer.Chunk(ushort.MaxValue - 29);

                foreach (var item in chunks) {
                    await client.SendAsync(item, item.Length, connectEP);
                }
            }
        });
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
