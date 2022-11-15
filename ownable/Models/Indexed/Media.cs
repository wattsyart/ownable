using ownable.Serialization;

namespace ownable.Models.Indexed
{
    public class Media : Indexable
    {
        [Indexed]
        public string? ContentType { get; set; }

        [Indexed]
        public string? Processor { get; set; }

        [Indexed]
        public string? Path { get; set; }

        [Indexed]
        public string? Extension { get; set; }
        
        public override void Serialize(IndexSerializeContext context)
        {
            base.Serialize(context);
            context.bw.WriteNullableString(ContentType);
            context.bw.WriteNullableString(Processor);
            context.bw.WriteNullableString(Path);
            context.bw.WriteNullableString(Extension);
        }

        public override void Deserialize(IndexDeserializeContext context)
        {
            base.Deserialize(context);
            ContentType = context.br.ReadNullableString();
            Processor = context.br.ReadNullableString();
            Path = context.br.ReadNullableString();
            Extension = context.br.ReadNullableString();
        }
    }
}
