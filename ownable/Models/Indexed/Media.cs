using ownable.Serialization;

namespace ownable.Models.Indexed
{
    public class Media : Indexable
    {
        [Indexed]
        public string? ContentType { get; set; }

        [Indexed]
        public string? Provider { get; set; }

        [Indexed]
        public string? Path { get; set; }


        public override void Serialize(IndexSerializeContext context)
        {
            base.Serialize(context);
            context.bw.WriteNullableString(ContentType);
            context.bw.WriteNullableString(Provider);
            context.bw.WriteNullableString(Path);
        }

        public override void Deserialize(IndexDeserializeContext context)
        {
            base.Deserialize(context);
            ContentType = context.br.ReadNullableString();
            Provider = context.br.ReadNullableString();
            Path = context.br.ReadNullableString();
        }
    }
}
