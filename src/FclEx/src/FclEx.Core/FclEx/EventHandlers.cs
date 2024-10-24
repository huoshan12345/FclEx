namespace FclEx;

public delegate Task AsyncEventHandler<in TSender>(TSender sender);