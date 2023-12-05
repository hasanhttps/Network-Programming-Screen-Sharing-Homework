using System;
using System.Net;
using System.Text;
using System.Drawing;
using System.Net.Sockets;
using System.Drawing.Imaging;
using System.Collections.Generic;


var listener = new UdpClient(27002);
var client = new UdpClient(27001);

var remoteEP = new IPEndPoint(IPAddress.Any, 0);

var msg = "";
var len = 0;
byte[] buffer;
var list = new List<byte>();

var connectEP = new IPEndPoint(IPAddress.Parse("127.0.0.2"), 27003);

while (true) {

    while (true) {
        var result = await listener.ReceiveAsync();
        buffer = result.Buffer;
        len = buffer.Length;
        list.AddRange(buffer);
        if (len != ushort.MaxValue - 29) break;
    }

    var chunks = list.Chunk(ushort.MaxValue - 29);

    foreach (var item in chunks)
        await client.SendAsync(item, item.Length, connectEP);
    list.Clear();
}