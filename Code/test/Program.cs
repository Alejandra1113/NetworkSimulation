// See https://aka.ms/new-console-template for more information
/* using NetSimul.Component;
Frame hola1 = new Frame("af01", "ed04", "f5", new ParityCheck());
Frame hola2 = new Frame("ff02", "ed04", "a1f1", new ParityCheck());

bool[] h1;
Frame.PackFrame(hola1, out h1);
System.Console.WriteLine(string.Concat(h1.Select(x => x? '1': '0')));

bool[] h2;
Frame.PackFrame(hola2, out h2);
System.Console.WriteLine(string.Concat(h2.Select(x => x? '1': '0')));
System.Console.WriteLine($"{string.Concat(h1.Zip(h2).Select(x => (x.First ^ x.Second)? '1': '0'))}");

Frame? hola3;
Frame.DisPackFrame(h1.Zip(h2).Select(x => (x.First ^ x.Second)).ToArray(), out hola3);

System.Console.WriteLine($"{hola3.HexaFromMac}");
System.Console.WriteLine($"{hola3.HexaToMac}");
System.Console.WriteLine($"{hola3.HexaData}");
System.Console.WriteLine($"{hola3.HexaDataV}");

System.Console.WriteLine(new ParityCheck().GetVerificationData("1010000111110001")); */

/* using NetSimul.Component;

string ip1 = "192.168.41.1";
string ip2 = "192.168.41.2";

Datagram datagram = new Datagram(ip1, ip2, "FFAa", 0, 1);

string? pack;
Datagram? rDatagram;
Datagram.PackDatagram(datagram, out pack);
Datagram.DisPackDatagram(pack, out rDatagram);
System.Console.WriteLine(pack); */

//"1010111100000001111011010000010000001000000010001111010100110000"