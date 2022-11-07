﻿namespace ownable.Serialization;

public interface ISerialize<in TContext> where TContext : ISerializeContext
{
    void Serialize(TContext context);
    void WriteToFile(string path);
}