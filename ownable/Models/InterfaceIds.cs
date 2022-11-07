using Nethereum.Hex.HexConvertors.Extensions;

namespace ownable.Models;

internal static class InterfaceIds
{
    public static readonly byte[] ERC721 = "0x80ac58cd".HexToByteArray();
    public static readonly byte[] ERC721Metadata = "0x5b5e139f".HexToByteArray();

    public static readonly byte[] ERC1155 = "0xd9b67a26".HexToByteArray();
    public static readonly byte[] ERC1155Metadata = "0x0e89341c".HexToByteArray();

    public static readonly byte[] TokenURI = "0xc87b56dd".HexToByteArray();
    public static readonly byte[] Name = "0x06fdde03".HexToByteArray();
    public static readonly byte[] Symbol = "0x95d89b41".HexToByteArray();
}