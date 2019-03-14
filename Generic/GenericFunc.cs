using System;
using System.Collections.Generic;

public static class GenericFunc{

    /// <summary>
    /// Get a random item of the given list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static T GetRandomOfList<T>(List<T> list)
    {
        Random random = new Random();
        return list[random.Next(0, list.Count - 1)];
    }
}
