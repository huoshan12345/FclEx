namespace FclEx;

public delegate Task AsyncEventHandler<in TSender>(TSender sender);

public delegate void RefAction<T, in TMember>(ref T obj, TMember value);