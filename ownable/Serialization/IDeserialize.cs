﻿namespace ownable.Serialization;

public interface IDeserialize<in TContext> where TContext : IDeserializeContext
{
    void Deserialize(TContext context);
    void ReadFromFile(string path);
}