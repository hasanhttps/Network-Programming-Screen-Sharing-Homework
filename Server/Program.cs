using System.IO;
using System.Net;
using System.Text;
using System.Drawing;
using System.Net.Sockets;
using System.Drawing.Imaging;

Bitmap? imageData = null;

void TakeScreenShot() {
    try {
        Console.WriteLine("Process started ...");

        Bitmap memoryImage = new Bitmap(1920, 1080);
        Size s = new Size(memoryImage.Width, memoryImage.Height);

        Graphics memoryGraphics = Graphics.FromImage(memoryImage);

        memoryGraphics.CopyFromScreen(0, 0, 0, 0, s);

        imageData = memoryImage;
    }
    catch (Exception ex) {
        Console.WriteLine(ex);
    }
}

static byte[] ConvertBitmapToByteArray(Bitmap bitmap) {
    using (MemoryStream stream = new MemoryStream()) {

        bitmap.Save(stream, ImageFormat.Jpeg);

        return stream.ToArray();
    }
}


var listener = new UdpClient(27002);
var client = new UdpClient(27001);

var remoteEP = new IPEndPoint(IPAddress.Any, 0);

var msg = "";
var len = 0;

var connectEP = new IPEndPoint(IPAddress.Parse("127.0.0.2"), 27003);

while (true) {

    var buffer = listener.Receive(ref remoteEP);

    await Task.Run(async () => {

        msg = Encoding.Default.GetString(buffer);

        if (msg == "takescreenshot") {

            while (true) {

                TakeScreenShot();
                byte[] image = ConvertBitmapToByteArray(imageData);
                var chunks = image.Chunk(ushort.MaxValue - 29);

                foreach (var item in chunks) {
                    await client.SendAsync(item, item.Length, connectEP);
                }
            }
        }
    });
}