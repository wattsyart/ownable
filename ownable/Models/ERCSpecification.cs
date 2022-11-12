namespace ownable.Models;

public sealed class ERCSpecification
{
    public byte[]? standardInterface;
    public byte[]? metadataInterface;
    public byte[]? uriInterface;
    public byte[]? nameInterface;
    public byte[]? symbolInterface;

    public static readonly ERCSpecification ERC721 = new()
    {
        standardInterface = InterfaceIds.ERC721,
        metadataInterface = InterfaceIds.ERC721Metadata,
        uriInterface = InterfaceIds.TokenURI,
        nameInterface = InterfaceIds.Name,
        symbolInterface = InterfaceIds.Symbol
    };

    public static readonly ERCSpecification ERC1155 = new()
    {
        standardInterface = InterfaceIds.ERC1155,
        metadataInterface = InterfaceIds.ERC1155Metadata,
        uriInterface = null,
        nameInterface = InterfaceIds.Name,
        symbolInterface = InterfaceIds.Symbol
    };
}