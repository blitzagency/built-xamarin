using System;

namespace BUILT.Shared
{
    public interface IModel<T>
    {
        T Model { get; set; }
    }
}

